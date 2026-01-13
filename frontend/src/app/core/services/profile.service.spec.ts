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
    it('debería hacer una solicitud GET a /api/providers/{id}', async () => {
      const mockProfile = createMockProfile();

      const promise = firstValueFrom(service.getProfile(providerId));
      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);

      expect(req.request.method).toBe('GET');
      req.flush(mockProfile);

      const result = await promise;
      expect(result).toEqual(mockProfile);
    });

    it('debería mapear la respuesta a ProviderProfile con tipos correctos', async () => {
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

    it('debería manejar 404 cuando el proveedor no existe', async () => {
      const promise = firstValueFrom(service.getProfile(providerId)).catch(
        (error) => error
      );

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.flush('Provider not found', { status: 404, statusText: 'Not Found' });

      const error = await promise;
      expect(error.status).toBe(404);
    });

    it('debería rechazar ID de proveedor inválido (número negativo)', async () => {
      const promise = firstValueFrom(service.getProfile(-1)).catch(
        (error) => error
      );

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
      expect(error.message).toContain('positive integer');
    });

    it('debería rechazar ID de proveedor inválido (cero)', async () => {
      const promise = firstValueFrom(service.getProfile(0)).catch(
        (error) => error
      );

      const error = await promise;
      expect(error.message).toContain('Invalid provider ID');
    });

    it('debería rechazar ID de proveedor no entero', async () => {
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
    it('debería hacer una solicitud PUT a /api/providers/{id} con datos actualizados', async () => {
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

    it('debería actualizar solo campos no nulos', async () => {
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

    it('debería retornar datos de perfil actualizados', async () => {
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

    it('debería manejar error de validación 400 (fullName inválido)', async () => {
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

    it('debería manejar 403 cuando el usuario no es el dueño del perfil', async () => {
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

    it('debería rechazar ID de proveedor inválido para actualización', async () => {
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
    it('debería hacer una solicitud POST a /api/providers/{id}/photo', async () => {
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

    it('debería enviar FormData con archivo', async () => {
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

    it('debería retornar photoUrl en la respuesta', async () => {
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

    it('debería rechazar tipo de archivo inválido antes de la solicitud HTTP (solo JPEG/PNG/WebP)', async () => {
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

    it('debería rechazar image/gif (no en tipos permitidos)', async () => {
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

    it('debería rechazar archivo que excede 5MB antes de la solicitud HTTP', async () => {
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

    it('debería aceptar archivo JPEG válido', async () => {
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

    it('debería aceptar archivo PNG válido', async () => {
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

    it('debería aceptar archivo WebP válido', async () => {
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

    it('debería rechazar ID de proveedor inválido para carga de foto', async () => {
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
    it('debería manejar el error de servidor 500 de forma elegante', async () => {
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

    it('debería manejar timeout de red', async () => {
      const promise = firstValueFrom(
        service.getProfile(providerId)
      ).catch((error) => error);

      const req = httpMock.expectOne(`${apiUrl}/${providerId}`);
      req.error(new ProgressEvent('Network error'));

      const error = await promise;
      expect(error).toBeDefined();
    });

    it('debería manejar 401 no autorizado para actualización', async () => {
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

    it('debería manejar 413 Payload Too Large del servidor', async () => {
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