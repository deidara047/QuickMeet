import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProfileService } from './profile.service';
import { ProviderProfile } from '../../shared/models/availability.model';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

// ============================================================================
// TEST SETUP
// ============================================================================
// TestBed is initialized globally via test-setup.ts (projects setupFiles)

// ============================================================================
// TEST SUITE
// ============================================================================

describe('ProfileService', () => {
  let service: ProfileService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/api/providers`;
  const providerId = 123;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProfileService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProfileService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    // Reset TestBed after each test to allow next test to configure it
    TestBed.resetTestingModule();
  });

  // Helper function to create test profile data
  function createMockProfile(overrides?: Partial<ProviderProfile>): ProviderProfile {
    return {
      id: providerId,
      email: 'doctor@example.com',
      username: 'dr_juan',
      fullName: 'Dr. Juan Pérez',
      description: 'Especialista en medicina general',
      photoUrl: 'https://example.com/photos/dr-juan.jpg',
      phoneNumber: '+34 612 345 678',
      appointmentDurationMinutes: 30,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      ...overrides
    };
  }

  // ========================================================================
  // GET PROFILE TESTS [6 tests]
  // ========================================================================

  describe('getProfile', () => {
    it('should make GET request to /api/providers/{id}', async () => {
      const mockProfile = createMockProfile();

      const promise = firstValueFrom(service.getProfile(providerId));
      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);

      expect(req.request.method).toBe('GET');
      req.flush(mockProfile);

      const result = await promise;
      expect(result).toEqual(mockProfile);
    });

    it('should map response to ProviderProfile with correct types', async () => {
      const mockProfile = createMockProfile({
        fullName: 'Dr. María García',
        email: 'maria@example.com',
        id: 456
      });

      const promise = firstValueFrom(service.getProfile(456));
      const req = httpMock.expectOne(`${apiUrl}/456`);
      req.flush(mockProfile);

      const result = await promise;
      expect(result.fullName).toBe('Dr. María García');
      expect(result.email).toBe('maria@example.com');
      expect(result.id).toBe(456);
      expect(typeof result.id).toBe('number');
    });

    it('should handle 404 when provider does not exist', async () => {
      const promise = firstValueFrom(service.getProfile(providerId)).catch(
        (error) => error
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush('Provider not found', { status: 404, statusText: 'Not Found' });

      const error = await promise;
      expect(error.status).toBe(404);
    });

    it('should reject invalid provider ID (negative number)', async () => {
      const promise = firstValueFrom(service.getProfile(-1)).catch(
        (error) => error
      );

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
      expect(error.message).toContain('positive integer');
    });

    it('should reject invalid provider ID (zero)', async () => {
      const promise = firstValueFrom(service.getProfile(0)).catch(
        (error) => error
      );

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
    });

    it('should reject non-integer provider ID', async () => {
      const promise = firstValueFrom(service.getProfile(3.14 as any)).catch(
        (error) => error
      );

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
    });
  });

  // ========================================================================
  // UPDATE PROFILE TESTS [7 tests]
  // ========================================================================

  describe('updateProfile', () => {
    it('should make PUT request to /api/providers/{id} with updated data', async () => {
      const updatedProfile = createMockProfile({
        fullName: 'Dr. Juan Pérez Updated'
      });
      const updatePayload: Partial<ProviderProfile> = {
        fullName: 'Dr. Juan Pérez Updated'
      };

      const promise = firstValueFrom(
        service.updateProfile(providerId, updatePayload)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updatePayload);
      req.flush(updatedProfile);

      const result = await promise;
      expect(result.fullName).toBe('Dr. Juan Pérez Updated');
    });

    it('should update only non-null fields', async () => {
      const updatePayload: Partial<ProviderProfile> = {
        description: 'Nuevo especialista'
      };
      const expectedProfile = createMockProfile({
        description: 'Nuevo especialista'
      });

      const promise = firstValueFrom(
        service.updateProfile(providerId, updatePayload)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      expect(req.request.body).toEqual(updatePayload);
      req.flush(expectedProfile);

      const result = await promise;
      expect(result.description).toBe('Nuevo especialista');
    });

    it('should return updated profile data', async () => {
      const updatedProfile = createMockProfile({
        phoneNumber: '+34 987 654 321'
      });
      const updatePayload: Partial<ProviderProfile> = {
        phoneNumber: '+34 987 654 321'
      };

      const promise = firstValueFrom(
        service.updateProfile(providerId, updatePayload)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush(updatedProfile);

      const result = await promise;
      expect(result.phoneNumber).toBe('+34 987 654 321');
      expect(result.id).toBe(providerId);
    });

    it('should handle 400 validation error (invalid fullName)', async () => {
      const updatePayload: Partial<ProviderProfile> = {
        fullName: 'A'
      };

      const promise = firstValueFrom(
        service.updateProfile(providerId, updatePayload)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush(
        { message: 'Full name must be between 3 and 100 characters' },
        { status: 400, statusText: 'Bad Request' }
      );

      const error = await promise;
      expect(error.status).toBe(400);
    });

    it('should handle 403 when user is not the profile owner', async () => {
      const updatePayload: Partial<ProviderProfile> = {
        fullName: 'Dr. Someone Else'
      };

      const promise = firstValueFrom(
        service.updateProfile(999, updatePayload)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/999`);
      req.flush(
        { message: 'You can only update your own profile' },
        { status: 403, statusText: 'Forbidden' }
      );

      const error = await promise;
      expect(error.status).toBe(403);
    });

    it('should reject invalid provider ID for update', async () => {
      const updatePayload: Partial<ProviderProfile> = { fullName: 'Updated' };
      const promise = firstValueFrom(
        service.updateProfile(-1, updatePayload)
      ).catch((error) => error);

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
      expect(() => httpMock.expectOne(`${apiUrl}/-1`)).toThrow();
    });
  });

  // ========================================================================
  // UPLOAD PHOTO TESTS [12 tests]
  // ========================================================================

  describe('uploadPhoto', () => {
    it('should make POST request to /api/providers/{id}/photo', async () => {
      const mockFile = new File(['photo'], 'profile.jpg', {
        type: 'image/jpeg'
      });
      const responseData = {
        photoUrl: 'https://example.com/photos/profile-123.jpg'
      };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, mockFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      expect(req.request.method).toBe('POST');
      req.flush(responseData);

      const result = await promise;
      expect(result.photoUrl).toBe('https://example.com/photos/profile-123.jpg');
    });

    it('should send FormData with file', async () => {
      const mockFile = new File(['photo content'], 'avatar.png', {
        type: 'image/png'
      });
      const responseData = {
        photoUrl: 'https://example.com/photos/avatar-123.png'
      };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, mockFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      expect(req.request.body instanceof FormData).toBe(true);

      const formData = req.request.body as FormData;
      expect(formData.has('file')).toBe(true);

      req.flush(responseData);
      await promise;
    });

    it('should return photoUrl in response', async () => {
      const mockFile = new File([''], 'photo.jpg', { type: 'image/jpeg' });
      const photoUrl = 'https://cdn.example.com/photos/profile-uuid.jpg';
      const responseData = { photoUrl };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, mockFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      req.flush(responseData);

      const result = await promise;
      expect(result.photoUrl).toBe(photoUrl);
    });

    it('should reject invalid file type before HTTP request (JPEG/PNG/WebP only)', async () => {
      const pdfFile = new File(['content'], 'document.pdf', {
        type: 'application/pdf'
      });

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, pdfFile)
      ).catch((error) => error);

      const error = await promise;
      expect(error.message).toContain('Invalid file type');
      expect(error.message).toContain('image/jpeg');
      expect(error.message).toContain('image/png');
      expect(error.message).toContain('image/webp');
      expect(() => httpMock.expectOne(`${apiUrl}/${providerId}/photo`)).toThrow();
    });

    it('should reject image/gif (not in allowed types)', async () => {
      const gifFile = new File(['gif content'], 'animation.gif', {
        type: 'image/gif'
      });

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, gifFile)
      ).catch((error) => error);

      const error = await promise;
      expect(error.message).toContain('Invalid file type');
      expect(() => httpMock.expectOne(`${apiUrl}/${providerId}/photo`)).toThrow();
    });

    it('should reject file exceeding 5MB before HTTP request', async () => {
      const oversizedFile = new File(['x'], 'huge.jpg', { type: 'image/jpeg' });
      Object.defineProperty(oversizedFile, 'size', {
        value: 6 * 1024 * 1024
      });

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, oversizedFile)
      ).catch((error) => error);

      const error = await promise;
      expect(error.message).toContain('File size exceeds');
      expect(error.message).toContain('5MB');
      expect(error.message).toContain('6.00MB');
      expect(() => httpMock.expectOne(`${apiUrl}/${providerId}/photo`)).toThrow();
    });

    it('should accept valid JPEG file', async () => {
      const validFile = new File(['jpeg'], 'profile.jpg', { type: 'image/jpeg' });
      const responseData = { photoUrl: 'https://example.com/p.jpg' };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, validFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      req.flush(responseData);

      const result = await promise;
      expect(result.photoUrl).toBe('https://example.com/p.jpg');
    });

    it('should accept valid PNG file', async () => {
      const validFile = new File(['png'], 'profile.png', { type: 'image/png' });
      const responseData = { photoUrl: 'https://example.com/p.png' };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, validFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      req.flush(responseData);

      const result = await promise;
      expect(result.photoUrl).toBe('https://example.com/p.png');
    });

    it('should accept valid WebP file', async () => {
      const validFile = new File(['webp'], 'profile.webp', { type: 'image/webp' });
      const responseData = { photoUrl: 'https://example.com/p.webp' };

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, validFile)
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      req.flush(responseData);

      const result = await promise;
      expect(result.photoUrl).toBe('https://example.com/p.webp');
    });

    it('should reject invalid provider ID for photo upload', async () => {
      const validFile = new File(['jpg'], 'photo.jpg', { type: 'image/jpeg' });
      const promise = firstValueFrom(
        service.uploadPhoto(-1, validFile)
      ).catch((error) => error);

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
      expect(() => httpMock.expectOne(`${apiUrl}/-1/photo`)).toThrow();
    });
  });

  // ========================================================================
  // ERROR HANDLING TESTS [4 tests]
  // ========================================================================

  describe('error handling', () => {
    it('should handle 500 server error gracefully', async () => {
      const promise = firstValueFrom(
        service.getProfile(providerId)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush(
        { message: 'Internal server error' },
        { status: 500, statusText: 'Internal Server Error' }
      );

      const error = await promise;
      expect(error.status).toBe(500);
    });

    it('should handle network timeout', async () => {
      const promise = firstValueFrom(
        service.getProfile(providerId)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.error(new ProgressEvent('Network error'));

      const error = await promise;
      expect(error).toBeDefined();
    });

    it('should handle 401 unauthorized for update', async () => {
      const updatePayload: Partial<ProviderProfile> = {
        fullName: 'Updated Name'
      };

      const promise = firstValueFrom(
        service.updateProfile(providerId, updatePayload)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush(
        { message: 'Unauthorized - session expired' },
        { status: 401, statusText: 'Unauthorized' }
      );

      const error = await promise;
      expect(error.status).toBe(401);
    });

    it('should handle 413 Payload Too Large from server', async () => {
      const mockFile = new File(['content'], 'huge.jpg', { type: 'image/jpeg' });

      const promise = firstValueFrom(
        service.uploadPhoto(providerId, mockFile)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}/photo`);
      req.flush(
        { message: 'Payload too large' },
        { status: 413, statusText: 'Payload Too Large' }
      );

      const error = await promise;
      expect(error.status).toBe(413);
    });
  });
});