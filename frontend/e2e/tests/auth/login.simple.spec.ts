import { test, expect } from '../../fixtures/page.fixture';
import { selectors } from '../../helpers/ui.helper';
import { seedUser, cleanupUser } from '../../helpers/test-api.helper';
import { generateUniqueUser } from '../../helpers/test-data.helper';

test.describe('Login - Prueba Simple', () => {
  
  test('Login exitoso: usuario válido va a dashboard', async ({ page }) => {
    const testUser = generateUniqueUser('login-simple');
    
    try {
      await seedUser(page, testUser);
      console.log(`Usuario creado: ${testUser.email}`);
      
      await page.safeGoto('/login');
      
      await page.fill(selectors.loginEmail, testUser.email);
      await page.fill(selectors.loginPassword, testUser.password);
      
      await page.click(selectors.loginButton);
      
      await page.waitForURL('**/dashboard', { timeout: 8000 });
      
      expect(page.url()).toContain('/dashboard');
      console.log('Login exitoso, redirigido a dashboard');
    } finally {
      await cleanupUser(page, testUser.email);
      console.log(`Usuario eliminado: ${testUser.email}`);
    }
  });

});
