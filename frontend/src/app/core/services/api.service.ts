import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { environment } from '@env/environment.ts';

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
    let errorResponse: any = { error: errorMessage };

    if (error.name === 'TimeoutError') {
      errorMessage = `Request timeout after ${environment.apiTimeout}ms`;
      errorResponse = { error: errorMessage };
      console.error('[ApiService] Timeout Error:', errorMessage);
    } else if (error instanceof HttpErrorResponse) {
      // Si el servidor devolvió un JSON con estructura {error: "..."}, lo preservamos
      if (error.error && typeof error.error === 'object' && error.error.error) {
        errorResponse = error.error;
        errorMessage = error.error.error;
      } else if (error.error && typeof error.error === 'string') {
        // Si es un string simple
        errorResponse = { error: error.error };
        errorMessage = error.error;
      } else {
        // Fallback a mensaje genérico
        errorMessage = `Server Error ${error.status}: ${error.statusText}`;
        errorResponse = { error: errorMessage };
      }
      console.error('[ApiService] HTTP Error:', errorMessage);
    } else {
      console.error('[ApiService] Unexpected Error:', error);
    }

    // Lanzar un error que preserve la estructura del backend
    const err = new Error(errorMessage);
    (err as any).error = errorResponse;
    return throwError(() => err);
  }
}

