import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

export interface RegisterPayload {
  email: string;
  username: string;
  fullName: string;
  password: string;
  passwordConfirmation: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface VerifyEmailPayload {
  token: string;
}

export interface AuthResponse {
  providerId: string;
  email: string;
  username: string;
  fullName: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface AuthUser {
  providerId: string;
  email: string;
  username: string;
  fullName: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = inject(ApiService);
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';

  private currentUserSubject = new BehaviorSubject<AuthUser | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(!!this.getAccessToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  register(payload: RegisterPayload): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/register', payload).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/login', payload).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  verifyEmail(token: string): Observable<{ message: string }> {
    return this.api.post<{ message: string }>('/auth/verify-email', { token });
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUserSubject.value;
  }

  getCurrentUserId(): string | null {
    return this.currentUserSubject.value?.providerId || null;
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);

    const user: AuthUser = {
      providerId: response.providerId,
      email: response.email,
      username: response.username,
      fullName: response.fullName
    };

    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private getUserFromStorage(): AuthUser | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    return userStr ? JSON.parse(userStr) : null;
  }
}
