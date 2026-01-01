import { test, expect } from '../../fixtures/page.fixture';
import { selectors, verifyToastMessage } from '../../helpers/ui.helper';
import { generateUniqueUser } from '../../helpers/test-data.helper';
import { seedUser, cleanupUser } from '../../helpers/test-api.helper';

test.describe('Autenticación - Registro', () => {

  test('Happy Path: Registro exitoso → redirige a dashboard', async ({ page }) => {
    const testUser = generateUniqueUser('register-happy');

    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, testUser.email);
    await page.fill(selectors.registerUsername, testUser.username);
    await page.fill(selectors.registerFullName, testUser.fullName);
    await page.fill(selectors.registerPassword, testUser.password);
    await page.fill(selectors.registerPasswordConfirmation, testUser.password);

    await page.click(selectors.registerButton);

    await page.waitForURL('**/dashboard', { timeout: 8000 });
    expect(page.url()).toContain('/dashboard');

    const accessToken = await page.evaluate(() => localStorage.getItem('access_token'));
    const refreshToken = await page.evaluate(() => localStorage.getItem('refresh_token'));
    expect(accessToken).toBeTruthy();
    expect(refreshToken).toBeTruthy();

    console.log(`Usuario registrado: ${testUser.email}`);
    await cleanupUser(page, testUser.email);
  });

  test('Unhappy Path: Email duplicado → muestra error', async ({ page }) => {
    const testUser = generateUniqueUser('register-duplicate');

    try {
      await seedUser(page, testUser);

      await page.safeGoto('/register');

      await page.fill(selectors.registerEmail, testUser.email);
      await page.fill(selectors.registerUsername, 'different-username');
      await page.fill(selectors.registerFullName, 'Different User');
      await page.fill(selectors.registerPassword, testUser.password);
      await page.fill(selectors.registerPasswordConfirmation, testUser.password);

      await page.click(selectors.registerButton);

      // Usar helper para verificar el toast de error
      await verifyToastMessage(page, 'Email ya existe', 'error');

      // Validar que NO redirigió a dashboard
      expect(page.url()).toContain('/register');
      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      expect(token).toBeNull();

      console.log('Email duplicado rechazado correctamente - Error mostrado en toast');
    } finally {
      await cleanupUser(page, testUser.email);
    }
  });

  test('Unhappy Path: Username duplicado → muestra error', async ({ page }) => {
    const testUser = generateUniqueUser('register-dup-user');

    try {
      await seedUser(page, testUser);

      await page.safeGoto('/register');

      await page.fill(selectors.registerEmail, 'new-email@test.com');
      await page.fill(selectors.registerUsername, testUser.username);
      await page.fill(selectors.registerFullName, 'Another User');
      await page.fill(selectors.registerPassword, testUser.password);
      await page.fill(selectors.registerPasswordConfirmation, testUser.password);

      await page.click(selectors.registerButton);

      // Usar helper para verificar el toast de error
      await verifyToastMessage(page, 'Usuario ya existe', 'error');

      // Validar que NO redirigió a dashboard
      expect(page.url()).toContain('/register');
      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      expect(token).toBeNull();

      console.log('Username duplicado rechazado correctamente - Error mostrado en toast');
    } finally {
      await cleanupUser(page, testUser.email);
    }
  });

  test('Unhappy Path: Email inválido → validación del formulario', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'not-an-email');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con email inválido');
  });

  test('Unhappy Path: Username con caracteres no permitidos → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'user@invalid!');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con username inválido');
  });

  test('Unhappy Path: Password sin mayúscula → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'validpass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'validpass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con password sin mayúscula');
  });

  test('Unhappy Path: Password sin número → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con password sin número');
  });

  test('Unhappy Path: Password sin carácter especial → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con password sin carácter especial');
  });

  test('Unhappy Path: Password muy corta → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'Short1!');
    await page.fill(selectors.registerPasswordConfirmation, 'Short1!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con password muy corta');
  });

  test('Unhappy Path: Contraseñas no coinciden → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'DifferentPass456@');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con contraseñas no coincidentes');
  });

  test('Unhappy Path: Username muy corto → validación', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'ab');
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con username muy corto');
  });

  test('Unhappy Path: Username muy largo → validación', async ({ page }) => {
    await page.safeGoto('/register');

    const longUsername = 'a'.repeat(51);

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, longUsername);
    await page.fill(selectors.registerFullName, 'Valid User');
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con username muy largo');
  });

  test('Unhappy Path: FullName muy largo → validación', async ({ page }) => {
    await page.safeGoto('/register');

    const longName = 'a'.repeat(257);

    await page.fill(selectors.registerEmail, 'test@example.com');
    await page.fill(selectors.registerUsername, 'validuser');
    await page.fill(selectors.registerFullName, longName);
    await page.fill(selectors.registerPassword, 'ValidPass123!');
    await page.fill(selectors.registerPasswordConfirmation, 'ValidPass123!');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con fullName muy largo');
  });

  test('Edge Case: Todos los campos vacíos → botón deshabilitado', async ({ page }) => {
    await page.safeGoto('/register');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con todos los campos vacíos');
  });

  test('Edge Case: Solo email completado → botón deshabilitado', async ({ page }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, 'test@example.com');

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeDisabled();

    console.log('Botón submit deshabilitado con solo email completado');
  });

  test('Edge Case: Username con números y guiones → válido', async ({ page }) => {
    const testUser = generateUniqueUser('register-nums');

    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, testUser.email);
    await page.fill(selectors.registerUsername, 'user-123-456');
    await page.fill(selectors.registerFullName, testUser.fullName);
    await page.fill(selectors.registerPassword, testUser.password);
    await page.fill(selectors.registerPasswordConfirmation, testUser.password);

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeEnabled();

    console.log('Username con números y guiones es válido');

    await cleanupUser(page, testUser.email);
  });

  test('Edge Case: Password con exactamente 8 caracteres → válido', async ({ page }) => {
    const testUser = generateUniqueUser('register-8chars');

    await page.safeGoto('/register');

    const password8 = 'Pass123!';

    await page.fill(selectors.registerEmail, testUser.email);
    await page.fill(selectors.registerUsername, testUser.username);
    await page.fill(selectors.registerFullName, testUser.fullName);
    await page.fill(selectors.registerPassword, password8);
    await page.fill(selectors.registerPasswordConfirmation, password8);

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeEnabled();

    console.log('Password con exactamente 8 caracteres es válido');

    await cleanupUser(page, testUser.email);
  });

  test('Edge Case: FullName con 1 carácter → válido', async ({ page }) => {
    const testUser = generateUniqueUser('register-name-1');

    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, testUser.email);
    await page.fill(selectors.registerUsername, testUser.username);
    await page.fill(selectors.registerFullName, 'A');
    await page.fill(selectors.registerPassword, testUser.password);
    await page.fill(selectors.registerPasswordConfirmation, testUser.password);

    const submitButton = page.locator(selectors.registerButton);
    await expect(submitButton).toBeEnabled();

    console.log('FullName con 1 carácter es válido');

    await cleanupUser(page, testUser.email);
  });

});
