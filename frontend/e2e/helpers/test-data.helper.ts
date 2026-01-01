export interface TestUser {
  email: string;
  username: string;
  fullName: string;
  password: string;
}

export function generateUniqueUser(baseEmail: string = 'test'): TestUser {
  const timestamp = Date.now();
  const randomSuffix = Math.random().toString(36).substring(7);

  return {
    email: `${baseEmail}+${timestamp}+${randomSuffix}@test.com`,
    username: `testuser_${timestamp}`,
    fullName: `Test User ${timestamp}`,
    password: 'Test@123456',
  };
}

export function generateMultipleUsers(count: number, baseEmail: string = 'test'): TestUser[] {
  return Array.from({ length: count }, (_, i) => 
    generateUniqueUser(`${baseEmail}_${i}`)
  );
}
