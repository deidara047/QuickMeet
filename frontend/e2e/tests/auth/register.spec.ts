import { test, expect } from '../../fixtures/page.fixture';
import { selectors } from '../shared/test-helpers';

test.describe('Autenticación - Registro', () => {
  test('Flujo 1: Registro exitoso y redirección a dashboard', async ({ page, testUser }) => {
    await page.safeGoto('/register');

    await page.fill(selectors.registerEmail, testUser.email);
    await page.fill(selectors.registerUsername, testUser.username);
    await page.fill(selectors.registerFullName, testUser.fullName);
    await page.fill(selectors.registerPassword, testUser.password);
    await page.fill(selectors.registerPasswordConfirmation, testUser.password);

    await page.click(selectors.registerButton);

    // El registro es exitoso si redirige a dashboard (genera token automáticamente)
    await page.waitForURL('/dashboard', { timeout: 5000 });

    // Verificar token guardado
    const token = await page.evaluate(() => localStorage.getItem('access_token'));
    expect(token).toBeTruthy();
  });
});
