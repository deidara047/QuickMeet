import '@angular/compiler';
import { AuthService, AuthResponse, RegisterPayload, LoginPayload, AuthUser } from './auth.service';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { of, throwError, firstValueFrom } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { Injector, runInInjectionContext } from '@angular/core';

// Mock StorageService en memoria
class MockStorageService implements StorageService {
  private storage = new Map<string, string>();

  getItem(key: string): string | null {
    return this.storage.get(key) ?? null;
  }

  setItem(key: string, value: string): void {
    this.storage.set(key, value);
  }

  removeItem(key: string): void {
    this.storage.delete(key);
  }

  clear(): void {
    this.storage.clear();
  }
}

// Factories para datos de prueba
const createAuthResponse = (overrides?: Partial<AuthResponse>): AuthResponse => ({
  providerId: 'test-provider-123',
  email: 'test@example.com',
  username: 'testuser',
  fullName: 'Test User',
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
  expiresAt: new Date(Date.now() + 3600000).toISOString(),
  ...overrides
});

const createRegisterPayload = (overrides?: Partial<RegisterPayload>): RegisterPayload => ({
  email: 'test@example.com',
  username: 'testuser',
  fullName: 'Test User',
  password: 'password123',
  passwordConfirmation: 'password123',
  ...overrides
});

const createLoginPayload = (overrides?: Partial<LoginPayload>): LoginPayload => ({
  email: 'test@example.com',
  password: 'password123',
  ...overrides
});

const createAuthUser = (overrides?: Partial<AuthUser>): AuthUser => ({
  providerId: 'test-provider-123',
  email: 'test@example.com',
  username: 'testuser',
  fullName: 'Test User',
  ...overrides
});

// Helper para crear servicio con mocks
function createAuthServiceWithMocks() {
  const mockStorage = new MockStorageService();
  const mockApi = { post: vi.fn() };

  const injector = Injector.create({
    providers: [
      AuthService,
      { provide: ApiService, useValue: mockApi },
      { provide: StorageService, useValue: mockStorage }
    ]
  });

  const service = runInInjectionContext(injector, () => injector.get(AuthService));

  return { service, mockApi, mockStorage };
}

describe('AuthService', () => {
  let service: AuthService;
  let mockApi: any;
  let mockStorage: MockStorageService;

  beforeEach(() => {
    ({ service, mockApi, mockStorage } = createAuthServiceWithMocks());
  });

  afterEach(() => vi.clearAllMocks());

  describe('register', () => {
    it('debe hacer POST a /auth/register con el payload', async () => {
      const payload = createRegisterPayload();
      mockApi.post.mockReturnValue(of(createAuthResponse()));

      await firstValueFrom(service.register(payload));

      expect(mockApi.post).toHaveBeenCalledWith('/auth/register', payload);
    });

    it('debe guardar access y refresh token tras registro exitoso', async () => {
      const response = createAuthResponse({ accessToken: 't-123', refreshToken: 'r-456' });
      mockApi.post.mockReturnValue(of(response));

      await firstValueFrom(service.register(createRegisterPayload()));

      expect(mockStorage.getItem('access_token')).toBe('t-123');
      expect(mockStorage.getItem('refresh_token')).toBe('r-456');
    });

    it('debe actualizar currentUser$ e isAuthenticated$ tras registro exitoso', async () => {
      const response = createAuthResponse();
      mockApi.post.mockReturnValue(of(response));

      await firstValueFrom(service.register(createRegisterPayload()));
      const user = await firstValueFrom(service.currentUser$);
      const auth = await firstValueFrom(service.isAuthenticated$);

      expect(user?.email).toBe(response.email);
      expect(auth).toBe(true);
    });

    it('debe retornar la respuesta del API', async () => {
      const response = createAuthResponse();
      mockApi.post.mockReturnValue(of(response));

      const result = await firstValueFrom(service.register(createRegisterPayload()));

      expect(result).toEqual(response);
    });

    it('no debe guardar tokens si el registro falla', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Email ya registrado')));

      await expect(firstValueFrom(service.register(createRegisterPayload())))
        .rejects.toThrow('Email ya registrado');

      expect(mockStorage.getItem('access_token')).toBeNull();
      expect(mockStorage.getItem('refresh_token')).toBeNull();
    });
  });

  describe('login', () => {
    it('debe hacer POST a /auth/login con credenciales', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse()));

      await firstValueFrom(service.login(createLoginPayload()));

      expect(mockApi.post).toHaveBeenCalledWith('/auth/login', expect.any(Object));
    });

    it('debe guardar tokens tras login exitoso', async () => {
      const response = createAuthResponse();
      mockApi.post.mockReturnValue(of(response));

      await firstValueFrom(service.login(createLoginPayload()));

      expect(mockStorage.getItem('access_token')).toBe(response.accessToken);
      expect(mockStorage.getItem('refresh_token')).toBe(response.refreshToken);
    });

    it('debe poner isAuthenticated$ en true tras login exitoso', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse()));

      await firstValueFrom(service.login(createLoginPayload()));
      const auth = await firstValueFrom(service.isAuthenticated$);

      expect(auth).toBe(true);
    });

    it('debe actualizar currentUser$ tras login exitoso', async () => {
      const response = createAuthResponse();
      mockApi.post.mockReturnValue(of(response));

      await firstValueFrom(service.login(createLoginPayload()));
      const user = await firstValueFrom(service.currentUser$);

      expect(user?.email).toBe(response.email);
    });

    it('debe fallar con error cuando credenciales inválidas', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Credenciales inválidas')));

      await expect(firstValueFrom(service.login(createLoginPayload())))
        .rejects.toThrow('Credenciales inválidas');
    });

    it('debe manejar error de cuenta suspendida', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Cuenta suspendida')));

      await expect(firstValueFrom(service.login(createLoginPayload())))
        .rejects.toThrow('Cuenta suspendida');

      expect(mockStorage.getItem('access_token')).toBeNull();
    });
  });

  describe('verifyEmail', () => {
    it('debe hacer POST a /auth/verify-email con token', async () => {
      const token = 'token-verificacion-123';
      mockApi.post.mockReturnValue(of({ message: 'Email verificado' }));

      const result = await firstValueFrom(service.verifyEmail(token));

      expect(mockApi.post).toHaveBeenCalledWith('/auth/verify-email', { token });
      expect(result.message).toBe('Email verificado');
    });

    it('debe fallar con token inválido o expirado', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Token de verificación expirado')));

      await expect(firstValueFrom(service.verifyEmail('token-mal')))
        .rejects.toThrow('Token de verificación expirado');
    });
  });

  describe('logout', () => {
    it('debe limpiar todos los datos de autenticación del storage', () => {
      mockStorage.setItem('access_token', 't');
      mockStorage.setItem('refresh_token', 'r');
      mockStorage.setItem('auth_user', '{}');

      service.logout();

      expect(mockStorage.getItem('access_token')).toBeNull();
      expect(mockStorage.getItem('refresh_token')).toBeNull();
      expect(mockStorage.getItem('auth_user')).toBeNull();
    });

    it('debe emitir null en currentUser$ al hacer logout', async () => {
      mockStorage.setItem('auth_user', JSON.stringify(createAuthUser()));

      service.logout();
      const user = await firstValueFrom(service.currentUser$);

      expect(user).toBeNull();
    });

    it('debe emitir false en isAuthenticated$ al hacer logout', async () => {
      mockStorage.setItem('access_token', 't');

      service.logout();
      const auth = await firstValueFrom(service.isAuthenticated$);

      expect(auth).toBe(false);
    });
  });

  describe('gestión de tokens', () => {
    it('debe obtener access token del storage', () => {
      mockStorage.setItem('access_token', 'abc123');
      expect(service.getAccessToken()).toBe('abc123');
    });

    it('debe retornar null si no hay access token', () => {
      expect(service.getAccessToken()).toBeNull();
    });

    it('debe obtener refresh token del storage', () => {
      mockStorage.setItem('refresh_token', 'xyz789');
      expect(service.getRefreshToken()).toBe('xyz789');
    });

    it('debe retornar null si no hay refresh token', () => {
      expect(service.getRefreshToken()).toBeNull();
    });
  });

  describe('gestión de usuario', () => {
    it('debe retornar usuario actual', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse({ providerId: 'u-777' })));
      await firstValueFrom(service.login(createLoginPayload()));

      expect(service.getCurrentUser()?.providerId).toBe('u-777');
    });

    it('debe retornar ID del usuario actual', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse({ providerId: 'xyz-999' })));
      await firstValueFrom(service.login(createLoginPayload()));

      expect(service.getCurrentUserId()).toBe('xyz-999');
    });

    it('debe retornar null cuando no está autenticado', () => {
      expect(service.getCurrentUser()).toBeNull();
    });

    it('debe retornar null ID cuando no está autenticado', () => {
      expect(service.getCurrentUserId()).toBeNull();
    });
  });

  describe('estado de autenticación', () => {
    it('debe retornar true si existe access token válido', () => {
      mockStorage.setItem('access_token', 'valid');
      expect(service.isAuthenticated()).toBe(true);
    });

    it('debe retornar false si no hay access token', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('flujos integrados', () => {
    it('debe manejar ciclo completo: register → login → logout', async () => {
      const response = createAuthResponse();
      mockApi.post.mockReturnValue(of(response));

      await firstValueFrom(service.register(createRegisterPayload()));
      expect(service.isAuthenticated()).toBe(true);

      service.logout();
      expect(service.isAuthenticated()).toBe(false);

      await firstValueFrom(service.login(createLoginPayload()));
      expect(service.isAuthenticated()).toBe(true);
    });

    it('debe persistir estado entre instancias del servicio', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse({ email: 'persist@ex.com' })));
      await firstValueFrom(service.login(createLoginPayload()));

      const newInjector = Injector.create({
        providers: [
          AuthService,
          { provide: ApiService, useValue: mockApi },
          { provide: StorageService, useValue: mockStorage }
        ]
      });

      const newService = runInInjectionContext(newInjector, () => newInjector.get(AuthService));

      expect(newService.isAuthenticated()).toBe(true);
      expect(newService.getCurrentUser()?.email).toBe('persist@ex.com');
    });

    it('debe manejar registros secuenciales con usuarios diferentes', async () => {
      mockApi.post.mockReturnValueOnce(of(createAuthResponse({ providerId: 'u1', email: 'u1@ex.com' })));
      await firstValueFrom(service.register(createRegisterPayload()));
      expect(service.getCurrentUser()?.email).toBe('u1@ex.com');

      service.logout();

      mockApi.post.mockReturnValueOnce(of(createAuthResponse({ providerId: 'u2', email: 'u2@ex.com' })));
      await firstValueFrom(service.register(createRegisterPayload({ email: 'u2@ex.com', username: 'u2' })));
      expect(service.getCurrentUser()?.email).toBe('u2@ex.com');
    });
  });

  describe('comportamiento de observables', () => {
    it('debe emitir estado inicial no autenticado', async () => {
      expect(await firstValueFrom(service.currentUser$)).toBeNull();
      expect(await firstValueFrom(service.isAuthenticated$)).toBe(false);
    });

    it('debe emitir actualización de usuario al hacer login', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse({ email: 'new@ex.com' })));

      await firstValueFrom(service.login(createLoginPayload()));
      expect((await firstValueFrom(service.currentUser$))?.email).toBe('new@ex.com');
    });

    it('debe emitir cambios de estado de autenticación', async () => {
      mockApi.post.mockReturnValue(of(createAuthResponse()));

      expect(await firstValueFrom(service.isAuthenticated$)).toBe(false);

      await firstValueFrom(service.login(createLoginPayload()));
      expect(await firstValueFrom(service.isAuthenticated$)).toBe(true);

      service.logout();
      expect(await firstValueFrom(service.isAuthenticated$)).toBe(false);
    });
  });

  describe('manejo de errores', () => {
    it('debe manejar errores de red', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Fallo de red')));

      await expect(firstValueFrom(service.login(createLoginPayload())))
        .rejects.toThrow('Fallo de red');
    });

    it('debe manejar error de token expirado', async () => {
      mockApi.post.mockReturnValue(throwError(() => new Error('Token expirado')));

      await expect(firstValueFrom(service.login(createLoginPayload())))
        .rejects.toThrow('Token expirado');
    });

    it('debe manejar JSON corrupto en storage sin fallar', () => {
      const badStorage = new MockStorageService();
      badStorage.setItem('auth_user', 'json-invalido-{');

      const injector = Injector.create({
        providers: [
          AuthService,
          { provide: ApiService, useValue: { post: vi.fn() } },
          { provide: StorageService, useValue: badStorage }
        ]
      });

      const serviceWithBadData = runInInjectionContext(injector, () => injector.get(AuthService));

      expect(serviceWithBadData.getCurrentUser()).toBeNull();
      expect(badStorage.getItem('auth_user')).toBeNull(); // Limpieza automática
    });

    it('no debe fallar al limpiar storage múltiples veces', () => {
      mockStorage.setItem('access_token', 't');

      expect(() => {
        service.logout();
        service.logout();
        service.logout();
      }).not.toThrow();
    });
  });

  describe('funcionalidades futuras (no implementadas)', () => {
    it.skip('debe limpiar tokens automáticamente en respuesta 401 via AuthInterceptor', async () => {
      // TODO: Implementar cuando AuthInterceptor maneje 401
      mockStorage.setItem('access_token', 'expired');

      const error401 = new Error('No autorizado');
      (error401 as any).status = 401;
      mockApi.post.mockReturnValue(throwError(() => error401));

      await expect(firstValueFrom(service.login(createLoginPayload()))).rejects.toThrow();
      expect(mockStorage.getItem('access_token')).toBeNull();
    });

    it.skip('debe validar expiración de token y refrescar automáticamente', async () => {
      // TODO: Implementar cuando endpoint refresh-token esté listo
      const soonExpire = new Date(Date.now() + 4 * 60000).toISOString();
      mockStorage.setItem('expires_at', soonExpire);

      // await service.ensureTokenValid();
      // expect(mockApi.post).toHaveBeenCalledWith('/auth/refresh-token', expect.anything());
    });

    it.skip('debe manejar intentos concurrentes de login sin problemas', async () => {
      // TODO: Implementar cuando se haga deduplicación de requests
      mockApi.post.mockReturnValue(of(createAuthResponse()));

      const [r1, r2] = await Promise.all([
        firstValueFrom(service.login(createLoginPayload())),
        firstValueFrom(service.login(createLoginPayload()))
      ]);

      expect(r1).toBeDefined();
      expect(r2).toBeDefined();
      expect(mockApi.post).toHaveBeenCalledTimes(2);
    });

    it.skip('debe validar validez del refresh token antes de usarlo', async () => {
      // TODO: Implementar cuando se haga validación de refresh token
      mockStorage.setItem('refresh_token', 'expired');

      // const valid = await service.isRefreshTokenValid();
      // expect(valid).toBe(false);
    });
  });
});