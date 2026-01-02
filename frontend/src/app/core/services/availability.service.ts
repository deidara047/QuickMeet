import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AvailabilityConfig, AvailabilityResponse, TimeSlot } from '../../shared/models/availability.model';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private readonly apiUrl = `${environment.apiUrl}/api/availability`;

  constructor(private http: HttpClient) {}

  configureAvailability(config: AvailabilityConfig): Observable<AvailabilityResponse> {
    return this.http.post<AvailabilityResponse>(`${this.apiUrl}/configure`, config);
  }

  getAvailability(providerId: string): Observable<AvailabilityConfig> {
    return this.http.get<AvailabilityConfig>(`${this.apiUrl}/${providerId}`);
  }

  updateAvailability(providerId: string, config: AvailabilityConfig): Observable<AvailabilityResponse> {
    return this.http.put<AvailabilityResponse>(`${this.apiUrl}/${providerId}`, config);
  }

  getAvailableSlots(providerId: string, date: string): Observable<TimeSlot[]> {
    const params = new HttpParams().set('date', date);
    return this.http.get<TimeSlot[]>(
      `${this.apiUrl}/slots/${providerId}`,
      { params }
    );
  }
}
