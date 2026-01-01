import { Page, expect } from '@playwright/test';

export const selectors = {
  loginEmail: '[data-testid="login-email-input"]',
  loginPassword: '[data-testid="login-password-input"] input',
  loginButton: '[data-testid="login-submit-button"] button',
  registerEmail: '[data-testid="register-email-input"]',
  registerUsername: '[data-testid="register-username-input"]',
  registerFullName: '[data-testid="register-fullname-input"]',
  registerPassword: '[data-testid="register-password-input"] input',
  registerPasswordConfirmation: '[data-testid="register-password-confirmation-input"] input',
  registerButton: '[data-testid="register-submit-button"] button',
  registerToast: '[data-testid="register-toast"]',
  toastMessage: 'p-toast .p-toast-message-text',
  successToast: '[data-testid="register-toast"] .ng-star-inserted.p-toast-message-success',
  errorToast: '[data-testid="register-toast"] .ng-star-inserted.p-toast-message-error',
  warningToast: '[data-testid="register-toast"] .ng-star-inserted.p-toast-message-warning',
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
  // Selector primario (específico)
  const primarySelector = severity === 'success' 
    ? selectors.successToast 
    : severity === 'error' 
    ? selectors.errorToast 
    : selectors.warningToast;

  // Selector secundario más general (más robusto)
  const severityClass = severity === 'success' ? 'p-toast-message-success' 
    : severity === 'error' ? 'p-toast-message-error'
    : 'p-toast-message-warning';
  
  const fallbackSelector = `.p-toast-message.${severityClass}`;

  // Intentar con el selector primario, si no funciona usar el fallback
  let toast = page.locator(primarySelector);
  let isVisible = await toast.isVisible().catch(() => false);
  
  if (!isVisible) {
    toast = page.locator(fallbackSelector);
    isVisible = await toast.isVisible().catch(() => false);
  }

  // Si aún no es visible, esperar con timeout más largo
  if (!isVisible) {
    await expect(page.locator(fallbackSelector)).toBeVisible({ timeout: 8000 });
    toast = page.locator(fallbackSelector);
  }
  
  const messageElement = toast.locator('.p-toast-message-text');
  const actualMessage = await messageElement.textContent();
  
  console.log(`Toast mostrado con mensaje: "${actualMessage}"`);
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
