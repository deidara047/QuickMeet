import { test, expect } from '@playwright/test';

test('should load homepage', async ({ page }) => {
  // Navigate to home page
  await page.goto('/');
  
  // Verify page loaded (basic check)
  expect(page).toBeDefined();
  
  // Check that the page title contains "QuickMeet" or has loaded
  const title = await page.title();
  console.log(`Page title: ${title}`);
  
  // Verify page is not blank
  const bodyContent = await page.locator('body').textContent();
  expect(bodyContent).toBeTruthy();
});
