import { test as setup, expect } from '@playwright/test';
import users from './users.json';

setup('authenticate user', async ({ page }) => {
  await page.goto('/login');
  
  await page.fill('[data-testid="login-email-input"]', users.testUser.email);
  await page.fill('[data-testid="login-password-input"] input', users.testUser.password);
  
  await page.click('[data-testid="login-submit-button"]');
  await page.waitForURL('/dashboard');
  
  const accessToken = await page.evaluate(() => localStorage.getItem('access_token'));
  expect(accessToken).toBeTruthy();
  
  await page.context().storageState({ path: 'playwright/.auth/user.json' });
});
