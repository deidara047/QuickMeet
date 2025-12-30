import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpTimeoutError } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { environment } from '@env/environment';

/**
 * Base HTTP service that uses environment configuration
 * 
 * Features:
 * - Configurable timeout from environment
 * - Automatic error handling and logging
 * - Base URL from environment.apiUrl
 * 
 * Usage:
 * - Inject ApiService in your component or service
 * - Use get<T>(), post<T>(), put<T>(), delete<T>()
 * - Endpoints should be relative paths (e.g., '/providers', '/appointments')
 * 
 * Example:
 * ```
 * constructor(private api: ApiService) {}
 * 
 * getProviders() {
 *   return this.api.get<Provider[]>('/providers');
 * }
 * ```
 */
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private timeout = environment.apiTimeout;

  constructor() {
    console.log(`[ApiService] Initialized with baseUrl: ${this.baseUrl}, timeout: ${this.timeout}ms`);
  }

  /**
   * GET request
   * @param endpoint Relative path (e.g., '/providers/username')
   */
  get<T>(endpoint: string): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.get<T>(url).pipe(
      timeout(this.timeout),
      catchError(this.handleError)
    );
  }

  /**
   * POST request
   * @param endpoint Relative path (e.g., '/appointments')
   * @param body Request body
   */
  post<T>(endpoint: string, body: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.post<T>(url, body).pipe(
      timeout(this.timeout),
      catchError(this.handleError)
    );
  }

  /**
   * PUT request
   * @param endpoint Relative path (e.g., '/appointments/123')
   * @param body Request body
   */
  put<T>(endpoint: string, body: any): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.put<T>(url, body).pipe(
      timeout(this.timeout),
      catchError(this.handleError)
    );
  }

  /**
   * DELETE request
   * @param endpoint Relative path (e.g., '/appointments/123')
   */
  delete<T>(endpoint: string): Observable<T> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.delete<T>(url).pipe(
      timeout(this.timeout),
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    let errorMessage = 'An error occurred';

    if (error instanceof HttpTimeoutError) {
      errorMessage = `Request timeout after ${environment.apiTimeout}ms`;
      console.error('[ApiService] Timeout Error:', errorMessage);
    } else if (error instanceof HttpErrorResponse) {
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        // Server-side error
        errorMessage = `Server Error ${error.status}: ${error.statusText}`;
      }
      console.error('[ApiService] HTTP Error:', errorMessage);
    } else {
      console.error('[ApiService] Unexpected Error:', error);
    }

    return throwError(() => new Error(errorMessage));
  }
}

