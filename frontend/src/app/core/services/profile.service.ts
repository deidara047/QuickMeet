import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProviderProfile } from '../../shared/models/availability.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly apiUrl = `${environment.apiUrl}/api/providers`;
  private readonly allowedPhotoTypes = ['image/jpeg', 'image/png', 'image/webp'];
  private readonly maxPhotoSize = 5 * 1024 * 1024; // 5MB

  // ✅ Usar inject() en lugar de constructor injection
  // Esto NO requiere metadatos de reflection
  private readonly http = inject(HttpClient);

  /**
   * Get profile by ID
   * @param providerId Numeric or string provider ID
   * @throws Error if providerId is not a valid number
   */
  getProfile(providerId: number | string): Observable<ProviderProfile> {
    const id = typeof providerId === 'string' ? Number(providerId) : providerId;
    if (!Number.isInteger(id) || id <= 0) {
      return throwError(() => new Error('Invalid provider ID. Must be a positive integer.'));
    }
    return this.http.get<ProviderProfile>(`${this.apiUrl}/${id}`);
  }

  /**
   * Update provider profile
   * @param providerId Numeric or string provider ID
   * @param profile Partial profile data to update
   * @throws Error if providerId is invalid
   */
  updateProfile(providerId: number | string, profile: Partial<ProviderProfile>): Observable<ProviderProfile> {
    const id = typeof providerId === 'string' ? Number(providerId) : providerId;
    if (!Number.isInteger(id) || id <= 0) {
      return throwError(() => new Error('Invalid provider ID. Must be a positive integer.'));
    }
    return this.http.put<ProviderProfile>(`${this.apiUrl}/${id}`, profile);
  }

  /**
   * Upload profile photo with client-side validation
   * @param providerId Numeric or string provider ID
   * @param file Image file to upload
   * @throws Error if file type is invalid
   * @throws Error if file size exceeds 5MB
   * @throws Error if providerId is invalid
   */
  uploadPhoto(providerId: number | string, file: File): Observable<{ photoUrl: string }> {
    const id = typeof providerId === 'string' ? Number(providerId) : providerId;

    // Validate file type (client-side before sending to server)
    if (!this.allowedPhotoTypes.includes(file.type)) {
      const errorMsg = `Invalid file type. Allowed types: ${this.allowedPhotoTypes.join(', ')}`;
      return throwError(() => new Error(errorMsg));
    }

    // Validate file size (client-side before sending to server)
    if (file.size > this.maxPhotoSize) {
      const sizeInMB = (this.maxPhotoSize / (1024 * 1024)).toFixed(0);
      const errorMsg = `File size exceeds ${sizeInMB}MB limit. Your file is ${(file.size / (1024 * 1024)).toFixed(2)}MB`;
      return throwError(() => new Error(errorMsg));
    }

    // Validate provider ID
    if (!Number.isInteger(id) || id <= 0) {
      return throwError(() => new Error('Invalid provider ID. Must be a positive integer.'));
    }

    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/${id}/photo`, formData);
  }
}
