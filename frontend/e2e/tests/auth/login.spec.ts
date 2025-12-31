import { test, expect } from '../../fixtures/page.fixture';
import { selectors } from '../shared/test-helpers';
import users from '../../fixtures/users.json';

test.describe('Login - Test Aislado', () => {
  test('Debería cargar la página /login', async ({ page }) => {
    await page.safeGoto('/login');
  });

  test('Debería llenar el campo de email', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginEmail, users.testUser.email);
    const emailValue = await page.inputValue(selectors.loginEmail);
    
    expect(emailValue).toBe(users.testUser.email);
  });

  test('Debería llenar el campo de password', async ({ page }) => {
    await page.safeGoto('/login');
    
    await page.fill(selectors.loginPassword, users.testUser.password);
    const passwordValue = await page.inputValue(selectors.loginPassword);
    
    expect(passwordValue).toBe(users.testUser.password);
  });

  test('Flujo completo: Login exitoso con redirección a dashboard', async ({ page }) => {
    await page.safeGoto('/login');
    
    // Llenar formulario
    await page.fill(selectors.loginEmail, users.testUser.email);
    await page.fill(selectors.loginPassword, users.testUser.password);
    
    // Escuchar respuesta del API
    let apiResponse = null;
    page.on('response', response => {
      if (response.url().includes('/api/auth/login')) {
        apiResponse = response;
        response.text().then(body => console.log('API Response:', response.status(), body));
      }
    });
    
    // Hacer submit
    await page.click(selectors.loginButton);
    
    // Esperar a que se procese
    await page.waitForTimeout(2000);
    
    // Verificar localStorage
    const token = await page.evaluate(() => localStorage.getItem('access_token'));
    console.log('Token in localStorage:', token ? 'Presente' : 'No encontrado');
    
    // Esperar redirección a dashboard
    await page.waitForURL('/dashboard', { timeout: 5000 });
  });
});
