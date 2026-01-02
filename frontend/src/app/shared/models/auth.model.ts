export interface AuthUser {
  id: number;
  email: string;
  username: string;
  fullName: string;
}

export interface AuthResponse {
  providerId: number;
  email: string;
  username: string;
  fullName: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
