import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProviderProfile } from '../../shared/models/availability.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly apiUrl = `${environment.apiUrl}/api/providers`;

  constructor(private http: HttpClient) {}

  getProfile(providerId: string): Observable<ProviderProfile> {
    return this.http.get<ProviderProfile>(`${this.apiUrl}/${providerId}`);
  }

  updateProfile(providerId: string, profile: Partial<ProviderProfile>): Observable<ProviderProfile> {
    return this.http.put<ProviderProfile>(`${this.apiUrl}/${providerId}`, profile);
  }

  uploadPhoto(providerId: string, file: File): Observable<{ photoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/${providerId}/photo`, formData);
  }
}
