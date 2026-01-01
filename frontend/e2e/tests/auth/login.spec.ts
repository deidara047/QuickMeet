import { test, expect } from '../../fixtures/page.fixture';
import { selectors, clearLocalStorage, verifyRedirection, verifyLocalStorage } from '../../helpers/ui.helper';
import { seedUser, cleanupUser, resetDatabase, pingTestEndpoint } from '../../helpers/test-api.helper';
import { generateUniqueUser } from '../../helpers/test-data.helper';

test.describe('Login - Pruebas E2E', () => {
  
  let dbResetDone = false;

  test.beforeEach(async ({ page }) => {
    await clearLocalStorage(page);
    
    if (!dbResetDone) {
      try {
        const response = await pingTestEndpoint(page);
        console.log(`Backend ejecutándose en: ${response.environment}`);
        
        if (response.environment !== 'Development') {
          throw new Error('Backend debe estar en modo Development para tests E2E');
        }
        
        await resetDatabase(page);
        console.log('Base de datos reiniciada para suite de Login');
        dbResetDone = true;
      } catch (error) {
        throw new Error(
          `Error al verificar backend. Asegúrate que está corriendo con:\n` +
          `  - ASPNETCORE_ENVIRONMENT=Development\n` +
          `  - AllowDangerousOperations=true\n\n` +
          `Error: ${error instanceof Error ? error.message : String(error)}`
        );
      }
    }
  });

  test.afterEach(async ({ page }) => {
    await clearLocalStorage(page);
  });

  test('Backend: TestController está activo', async ({ page }) => {
    const response = await pingTestEndpoint(page);
    expect(response.message).toContain('TestController is active');
    expect(response.environment).toBe('Development');
    console.log('Backend health check OK');
  });

  test('UI: Formulario de login renderiza correctamente', async ({ page }) => {
    await page.safeGoto('/login');
    
    await expect(page.locator(selectors.loginEmail)).toBeVisible();
    await expect(page.locator(selectors.loginPassword)).toBeVisible();
    await expect(page.locator(selectors.loginButton)).toBeVisible();
    
    await page.fill(selectors.loginEmail, 'test@example.com');
    expect(await page.inputValue(selectors.loginEmail)).toBe('test@example.com');
    
    await page.fill(selectors.loginPassword, 'Test@123456');
    expect(await page.inputValue(selectors.loginPassword)).toBe('Test@123456');
    
    console.log('UI del formulario verificada');
  });

  test('Login exitoso: flujo completo con validación de tokens', async ({ page }) => {
    const testUser = generateUniqueUser('login-test');
    
    try {
      const seededUser = await seedUser(page, testUser);
      expect(seededUser.email).toBe(testUser.email);
      console.log(`Usuario creado: ${seededUser.email}`);

      await page.safeGoto('/login');

      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, testUser.password);

      await page.click(selectors.loginButton);

      await verifyRedirection(page, '/dashboard', 8000);

      const accessToken = await verifyLocalStorage(page, 'access_token', true);
      const refreshToken = await verifyLocalStorage(page, 'refresh_token', true);
      const authUser = await page.evaluate(() => localStorage.getItem('auth_user'));

      expect(accessToken).toBeTruthy();
      expect(accessToken).toMatch(/^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$/);
      expect(refreshToken).toBeTruthy();
      expect(authUser).toBeTruthy();

      const user = JSON.parse(authUser!);
      expect(user.email).toBe(testUser.email);
      expect(user.username).toBe(testUser.username);
      
      console.log('Login exitoso, todos los tokens y estado verificados');
    } finally {
      await cleanupUser(page, testUser.email);
      console.log(`Usuario eliminado: ${testUser.email}`);
    }
  });

  test('Login fallido: credenciales incorrectas', async ({ page }) => {
    const testUser = generateUniqueUser('login-fail-test');
    
    try {
      await seedUser(page, testUser);
      console.log('Usuario creado para test de fallo');

      await page.safeGoto('/login');
      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, 'WrongPassword123!');
      await page.click(selectors.loginButton);

      await page.waitForTimeout(2000);
      expect(page.url()).toContain('/login');

      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      expect(token).toBeNull();
      
      console.log('Login fallido correctamente rechazado');
    } finally {
      await cleanupUser(page, testUser.email);
    }
  });

  test('Login fallido: usuario no existe', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginEmail, 'noexiste@example.com');
    await page.fill(selectors.loginPassword, 'Password123!');
    await page.click(selectors.loginButton);

    await page.waitForTimeout(2000);
    expect(page.url()).toContain('/login');

    const token = await page.evaluate(() => localStorage.getItem('access_token'));
    expect(token).toBeNull();
    
    console.log('Login con usuario inexistente correctamente rechazado');
  });

  test('Seguridad: mensaje de error genérico para casos de fallo', async ({ page }) => {
    const testUser = generateUniqueUser('security-test');
    
    try {
      await seedUser(page, testUser);

      await page.safeGoto('/login');
      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, 'WrongPassword!');
      await page.click(selectors.loginButton);
      await page.waitForTimeout(1000);
      
      await page.safeGoto('/login');
      await page.fill(selectors.loginEmail, 'noexiste@test.com');
      await page.fill(selectors.loginPassword, 'SomePassword!');
      await page.click(selectors.loginButton);
      await page.waitForTimeout(1000);

      expect(page.url()).toContain('/login');
      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      expect(token).toBeNull();
      
      console.log('Comportamiento de seguridad verificado: errores no revelan información');
    } finally {
      await cleanupUser(page, testUser.email);
    }
  });
});
