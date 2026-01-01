import { test, expect } from '../../fixtures/page.fixture';
import { selectors, verifyToastMessage } from '../../helpers/ui.helper';
import { seedUser, cleanupUser } from '../../helpers/test-api.helper';
import { generateUniqueUser } from '../../helpers/test-data.helper';

test.describe('Login - E2E Completo', () => {

  test('Happy Path: Login exitoso → redirige a dashboard', async ({ page }) => {
    const testUser = generateUniqueUser('login-happy');
    
    try {
      await seedUser(page, testUser);
      console.log(`Usuario creado: ${testUser.email}`);
      
      await page.safeGoto('/login');
      
      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, testUser.password);
      
      await page.click(selectors.loginButton);
      
      await page.waitForURL('**/dashboard', { timeout: 8000 });
      
      expect(page.url()).toContain('/dashboard');
      
      const accessToken = await page.evaluate(() => localStorage.getItem('access_token'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refresh_token'));
      const authUser = await page.evaluate(() => localStorage.getItem('auth_user'));
      
      expect(accessToken).toBeTruthy();
      expect(refreshToken).toBeTruthy();
      expect(authUser).toBeTruthy();
      
      const user = JSON.parse(authUser!);
      expect(user.email).toBe(testUser.email);
      
      console.log('Login exitoso, redirigido a dashboard con tokens guardados');
    } finally {
      await cleanupUser(page, testUser.email);
      console.log(`Usuario eliminado: ${testUser.email}`);
    }
  });

  test('Unhappy Path: Credenciales inválidas → muestra error', async ({ page }) => {
    const testUser = generateUniqueUser('login-invalid');
    
    try {
      await seedUser(page, testUser);
      console.log(`Usuario creado: ${testUser.email}`);
      
      await page.safeGoto('/login');
      
      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, 'WrongPassword123!');
      
      await page.click(selectors.loginButton);
      
      await page.waitForTimeout(3000);
      
      expect(page.url()).toContain('/login');
      
      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      expect(token).toBeNull();
      
      console.log('Login fallido correctamente, no redirigió a dashboard');
    } finally {
      await cleanupUser(page, testUser.email);
    }
  });

  test('Unhappy Path: Usuario no existe → muestra error', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginEmail, 'noexiste@test.com');
    await page.fill(selectors.loginPassword, 'ValidPassword123!');
    
    await page.click(selectors.loginButton);
    
    await page.waitForTimeout(3000);
    
    expect(page.url()).toContain('/login');
    
    const token = await page.evaluate(() => localStorage.getItem('access_token'));
    expect(token).toBeNull();
    
    console.log('Usuario no existe, login rechazado');
  });

  test('Edge Case: Campo email vacío → validación del formulario', async ({ page }) => {
    await page.safeGoto('/login');
    
    const submitButton = page.locator(selectors.loginButton);
    
    // El botón debe estar deshabilitado cuando los campos están vacíos
    await expect(submitButton).toBeDisabled();
    console.log('Botón submit deshabilitado con campos vacíos');
  });

  test('Edge Case: Email sin formato válido → validación del formulario', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginEmail, 'invalid-email');
    await page.fill(selectors.loginPassword, 'ValidPassword123!');
    
    const submitButton = page.locator(selectors.loginButton);
    
    // El botón debe estar deshabilitado con email inválido
    await expect(submitButton).toBeDisabled();
    console.log('Botón submit deshabilitado con email inválido');
  });

  test('Edge Case: Password muy corta → validación del formulario', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginEmail, 'test@example.com');
    await page.fill(selectors.loginPassword, 'short');
    
    const submitButton = page.locator(selectors.loginButton);
    
    // El botón debe estar deshabilitado con password muy corta
    await expect(submitButton).toBeDisabled();
    console.log('Botón submit deshabilitado con password muy corta');
  });

});
