import { Page, expect } from '@playwright/test';

export const selectors = {
  // Login form
  loginEmail: '[data-testid="login-email-input"]',
  loginPassword: '[data-testid="login-password-input"] input',
  loginButton: '[data-testid="login-submit-button"]',

  // Register form
  registerEmail: '[data-testid="register-email-input"]',
  registerUsername: '[data-testid="register-username-input"]',
  registerFullName: '[data-testid="register-fullname-input"]',
  registerPassword: '[data-testid="register-password-input"] input',
  registerPasswordConfirmation: '[data-testid="register-password-confirmation-input"] input',
  registerButton: '[data-testid="register-submit-button"]',

  // Toast messages
  toastMessage: 'p-toast .p-toast-message-text',
  successToast: 'p-toast .ng-star-inserted.p-toast-message-success',
  errorToast: 'p-toast .ng-star-inserted.p-toast-message-error',
  warningToast: 'p-toast .ng-star-inserted.p-toast-message-warning',
};

export async function fillForm(
  page: Page,
  fields: Record<string, string>,
  selectorMap: Record<string, string>
): Promise<void> {
  for (const [fieldName, value] of Object.entries(fields)) {
    const selector = selectorMap[fieldName];
    if (!selector) throw new Error(`Selector not found for field: ${fieldName}`);
    await page.fill(selector, value);
  }
}

export async function verifyToastMessage(
  page: Page,
  expectedMessage: string,
  severity: 'success' | 'error' | 'warn' = 'success'
): Promise<void> {
  const toastSelector = severity === 'success' 
    ? selectors.successToast 
    : severity === 'error' 
    ? selectors.errorToast 
    : selectors.warningToast;

  const toast = page.locator(toastSelector);
  await expect(toast).toBeVisible({ timeout: 5000 });
  
  const messageElement = toast.locator('.p-toast-message-text');
  const actualMessage = await messageElement.textContent();
  expect(actualMessage?.toLowerCase()).toContain(expectedMessage.toLowerCase());
}

export async function verifyRedirection(
  page: Page,
  expectedUrl: string,
  timeout: number = 5000
): Promise<void> {
  await page.waitForURL(expectedUrl, { timeout });
  expect(page.url()).toContain(expectedUrl);
}

export async function verifyLocalStorage(
  page: Page,
  key: string,
  shouldExist: boolean = true
): Promise<string | null> {
  const value = await page.evaluate((k) => localStorage.getItem(k), key);
  
  if (shouldExist) {
    expect(value).toBeTruthy();
  } else {
    expect(value).toBeNull();
  }
  
  return value;
}

export async function clearLocalStorage(page: Page): Promise<void> {
  await page.evaluate(() => localStorage.clear());
}

export async function waitForLoadingToComplete(page: Page, timeout: number = 5000): Promise<void> {
  const loadingSpinner = page.locator('.p-spinner, [role="progressbar"]');
  
  if (await loadingSpinner.isVisible().catch(() => false)) {
    await loadingSpinner.waitFor({ state: 'hidden', timeout });
  }
}
