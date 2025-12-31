import { test as base, expect, Page } from '@playwright/test';

type PageWithHelpers = Page & {
  safeGoto: (route: string) => Promise<void>;
};

type TestUser = {
  email: string;
  username: string;
  fullName: string;
  password: string;
};

export const test = base.extend<{ 
  page: PageWithHelpers;
  testUser: TestUser;
}>({
  page: async ({ page }, use) => {
    page.safeGoto = async (route: string) => {
      await page.goto(route);
      expect(page.url()).toContain(route);
      
      const bodyContent = await page.locator('body').textContent();
      expect(bodyContent).toBeTruthy();
    };

    await use(page);
  },

  testUser: async ({ page }, use) => {
    // Setup: Generar usuario único basado en timestamp
    const timestamp = Date.now();
    const user: TestUser = {
      email: `test-${timestamp}@quickmeet.local`,
      username: `testuser${timestamp}`,
      fullName: `Test User ${timestamp}`,
      password: 'TestPassword123!'
    };

    await use(user);

    // Teardown: Limpiar localStorage después del test
    await page.evaluate(() => {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('auth_user');
    });
  }
});

export { expect };
