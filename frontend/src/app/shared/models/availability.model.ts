export interface ProviderProfile {
  id: number;
  email: string;
  username: string;
  fullName: string;
  description?: string;
  photoUrl?: string;
  phoneNumber?: string;
  appointmentDurationMinutes: number;
  createdAt: string;
  updatedAt?: string;
  emailVerifiedAt?: string;
}

export interface TimeSlot {
  id: number;
  startTime: string;
  endTime: string;
  status: 'Available' | 'Reserved' | 'Blocked';
}

export interface BreakConfig {
  startTime: string;
  endTime: string;
}

export interface DayConfig {
  dayOfWeek: number;
  enabled: boolean;
  startTime: string;
  endTime: string;
  breaks: BreakConfig[];
}

export interface AvailabilityConfig {
  daysConfiguration: DayConfig[];
  appointmentDurationMinutes: number;
  bufferMinutes: number;
}

export interface AvailabilityResponse {
  id: number;
  providerId: number;
  daysConfiguration: DayConfig[];
  appointmentDurationMinutes: number;
  bufferMinutes: number;
  createdAt: string;
  updatedAt: string;
}

export interface AvailabilityResponse {
  success: boolean;
  message: string;
  generatedSlots: TimeSlot[];
}
