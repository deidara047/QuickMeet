import { Page } from '@playwright/test';
import type { TestUser } from './test-data.helper';

const API_URL = process.env.NG_APP_API_URL || 'http://localhost:5173/api';

interface SeedUserResponse {
  providerId: number;
  email: string;
  username: string;
  fullName: string;
  accessToken: string;
  refreshToken: string;
}

interface PingResponse {
  message: string;
  environment: string;
  timestamp: string;
}

interface MessageResponse {
  message: string;
}

interface ErrorResponse {
  error: string;
  details?: string;
}

export async function seedUser(page: Page, userData: TestUser): Promise<SeedUserResponse> {
  const response = await page.request.post(`${API_URL}/test/seed-user`, {
    data: {
      email: userData.email,
      username: userData.username,
      fullName: userData.fullName,
      password: userData.password,
    },
  });

  if (!response.ok()) {
    const error = await response.json() as ErrorResponse;
    throw new Error(`Failed to seed user ${userData.email}: ${error.error}`);
  }

  return response.json() as Promise<SeedUserResponse>;
}

export async function cleanupUser(page: Page, email: string): Promise<void> {
  const response = await page.request.delete(`${API_URL}/test/cleanup-user/${email}`);

  if (!response.ok() && response.status() !== 404) {
    const error = await response.json() as ErrorResponse;
    throw new Error(`Failed to cleanup user ${email}: ${error.error}`);
  }
}

export async function resetDatabase(page: Page): Promise<void> {
  const response = await page.request.post(`${API_URL}/test/reset-database`);

  if (!response.ok()) {
    const error = await response.json() as ErrorResponse;
    throw new Error(`Failed to reset database: ${error.error}`);
  }
}

export async function pingTestEndpoint(page: Page): Promise<PingResponse> {
  const response = await page.request.get(`${API_URL}/test/ping`);

  if (!response.ok()) {
    throw new Error(
      'TestController endpoints not available. ' +
      'Ensure backend is running with ASPNETCORE_ENVIRONMENT=Development (or Test) ' +
      'and AllowDangerousOperations=true'
    );
  }

  return response.json() as Promise<PingResponse>;
}
