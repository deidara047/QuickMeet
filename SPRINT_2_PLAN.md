# Sprint 2: Gestión de Perfil y Disponibilidad

**Estado:** En Progreso (Fase 2)  
**Duración estimada:** 7-8 horas  
**Inicio:** Enero 1, 2026  
**Fase 1 Completada:** ✅ Enero 1, 2026 - Entidades y Migraciones  
**Objetivo:** Implementar configuración de perfil público y sistema de disponibilidad con generación automática de slots

---

## 📋 FASE 1: Entidades de Base de Datos ✅ COMPLETADA

### Backend Entities
- [ ] Crear entidad `ProviderAvailability`
  - [ ] Id (INT NOT NULL IDENTITY)
  - [ ] ProviderId (FK a Provider)
  - [ ] DayOfWeek (enum: Monday-Sunday)
  - [ ] StartTime (TimeSpan)
  - [ ] EndTime (TimeSpan)
  - [ ] AppointmentDurationMinutes (int, default 30)
  - [ ] BufferMinutes (int, default 0)
  - [ ] CreatedAt (datetimeoffset)
  - [ ] UpdatedAt (datetimeoffset)
  - [ ] Relación con Provider (ICollection)

- [ ] Crear entidad `TimeSlot`
  - [ ] Id (INT NOT NULL IDENTITY)
  - [ ] ProviderId (FK a Provider)
  - [ ] StartTime (datetimeoffset)
  - [ ] EndTime (datetimeoffset)
  - [ ] Status (enum: Available, Reserved, Blocked)
  - [ ] CreatedAt (datetimeoffset)
  - [ ] UpdatedAt (datetimeoffset)
  - [ ] Relación con Provider (ICollection)

- [ ] Crear entidad `Break`
  - [ ] Id (INT NOT NULL IDENTITY)
  - [ ] ProviderAvailabilityId (FK a ProviderAvailability)
  - [ ] StartTime (TimeSpan)
  - [ ] EndTime (TimeSpan)
  - [ ] CreatedAt (datetimeoffset)
  - [ ] Relación con ProviderAvailability

### DbContext
- [ ] Actualizar `QuickMeetDbContext` con DbSets para las 3 entidades
- [ ] Configurar relaciones foráneas en OnModelCreating
- [ ] Configurar índices (ej: IX_TimeSlot_ProviderId_StartTime)
- [ ] Configurar default values (SYSUTCDATETIME para fechas)

### Migration
- [ ] Crear migración con nombre descriptivo: `AddAvailabilityAndTimeSlotEntities`
- [ ] Ejecutar migración en base de datos local
- [ ] Verificar tablas creadas correctamente en SQL Server

---

## 🔧 FASE 2: Servicios Backend ✅ COMPLETADA

### AvailabilityService (Core/Services)
- [x] Crear clase `AvailabilityService` con métodos:
  - [x] `ConfigureAvailability(providerId, configDto)` → void
  - [x] `GenerateTimeSlots(providerId, startDate, endDate)` → IEnumerable<TimeSlot>
  - [x] `GetAvailableSlotsForDate(providerId, date)` → IEnumerable<TimeSlotDto>

### DTOs (API/DTOs)
- [x] `AvailabilityConfigDto`
- [x] `DayConfigDto`
- [x] `BreakDto`
- [x] `TimeSlotDto`
- [x] `AvailabilityResponseDto`

### Validadores (FluentValidation)
- [x] Crear `AvailabilityConfigValidator`
- [x] Crear `DayConfigValidator`
- [x] Crear `BreakConfigValidator`

### Inyección de Dependencias
- [x] Registrar `IAvailabilityService` en Program.cs

---

## 🌐 FASE 3: Controller

### AvailabilityController (API/Controllers)
- [ ] Crear clase `AvailabilityController`
  - [ ] POST `/api/availability/configure` [Autorizado]
    - Parámetro: `AvailabilityConfigDto`
    - Validar DTO con FluentValidation
    - Obtener ProviderId del usuario autenticado
    - Llamar a AvailabilityService.ConfigureAvailability
    - Retornar HTTP 200 con AvailabilityResponseDto
  
  - [ ] GET `/api/availability/{providerId}` [Autorizado]
    - Validar que ProviderId = usuario logueado (solo su propia config)
    - Retornar AvailabilityConfigDto actual
    - Si no existe, retornar HTTP 404
  
  - [ ] PUT `/api/availability/{providerId}` [Autorizado]
    - Similar a POST pero actúa como actualización
    - Limpiar slots futuros antes de regenerar
    - Retornar HTTP 200 con slots actualizados

- [ ] Manejo de errores
  - [ ] Try-catch para excepciones
  - [ ] Retornar HTTP 400 para validaciones fallidas
  - [ ] Retornar HTTP 401 para no autorizados
  - [ ] Retornar HTTP 500 para errores del servidor

---

## 🎨 FASE 4: Frontend Components

### ProfileService
- [ ] Crear servicio en `core/services/profile.service.ts`
  - [ ] `updateProfile(profileData): Observable<ProviderProfile>`
  - [ ] `getProfile(): Observable<ProviderProfile>`
  - [ ] `uploadPhoto(file): Observable<string>` (retorna URL)

### DTO/Models (Frontend)
- [ ] Crear interfaces TypeScript:
  - [ ] `ProviderProfile`
  - [ ] `AvailabilityConfig`
  - [ ] `DayConfig`
  - [ ] `BreakConfig`
  - [ ] `TimeSlot`

### Dashboard Component (Container)
- [ ] Generar con `ng generate component features/dashboard/dashboard`
  - [ ] Estructura: dashboard.ts, dashboard.html, dashboard.css
  - [ ] Inyectar AuthService, ProfileService, AvailabilityService
  - [ ] Mostrar:
    - [ ] Nombre del profesional
    - [ ] Enlace público copiable: `quickmeet.app/[username]`
    - [ ] Resumen de citas del día (placeholder para Sprint 4)
    - [ ] Botones a secciones: "Editar Perfil", "Configurar Disponibilidad"
  - [ ] Componentes internos:
    - [ ] `<app-profile-editor>`
    - [ ] `<app-availability-configurator>`

### ProfileEditorComponent
- [ ] Generar con `ng generate component features/dashboard/profile-editor`
  - [ ] Formulario reactivo con:
    - [ ] Nombre público (input text, requerido, 3-100 chars)
    - [ ] Descripción (textarea, opcional, max 500 chars)
    - [ ] Teléfono (input tel, opcional, validación de formato)
    - [ ] Foto (file upload, preview)
    - [ ] Duración estándar (select: 15/30/45/60 minutos)
  - [ ] Botón "Guardar"
  - [ ] Mostrar estado de carga y errores
  - [ ] Guardar en BD via ProfileService

### AvailabilityConfiguratorComponent (CRÍTICO)
- [ ] Generar con `ng generate component features/dashboard/availability-configurator`
  - [ ] Formulario reactivo con FormBuilder/FormArray
  - [ ] **Sección 1: Horas de Trabajo**
    - [ ] 7 toggles (Lunes-Domingo)
    - [ ] Para cada día activo:
      - [ ] Input hora inicio (ej: 09:00)
      - [ ] Input hora fin (ej: 18:00)
      - [ ] Validación: StartTime < EndTime
  
  - [ ] **Sección 2: Descansos**
    - [ ] Botón "+ Agregar Break"
    - [ ] Para cada break: hora inicio y fin
    - [ ] Validación: Break dentro de horario laboral
    - [ ] Botón eliminar break
  
  - [ ] **Sección 3: Configuración de Citas**
    - [ ] Select duración: 15, 30, 45, 60 minutos
    - [ ] Select buffer: 0, 5, 10, 15 minutos
  
  - [ ] **Sección 4: Vista Previa**
    - [ ] Mostrar ejemplo de slots generados para próximas 3 días
    - [ ] Ejemplo:
      ```
      Lunes (15 Ene):
        09:00-09:30 ✓
        09:40-10:10 ✓
        [BREAK 13:00-14:00]
        14:00-14:30 ✓
      ```
  
  - [ ] Botón "Guardar Disponibilidad"
  - [ ] Loading state durante guardado
  - [ ] Mostrar mensajes de éxito/error
  - [ ] Toast notification con resultado

### Layout Adjustments
- [ ] Actualizar routing para agregar /dashboard
- [ ] Agregar navegación en header (si existe)
- [ ] Proteger /dashboard con authGuard

---

## 🧪 FASE 5: Testing

### Unit Tests - Backend (xUnit + Moq)

#### AvailabilityServiceTests
- [ ] `ConfigureAvailability_WithValidData_GenerateSlotsSuccessfully`
  - [ ] Arrange: Profesional ID, AvailabilityConfigDto válido
  - [ ] Act: Llamar ConfigureAvailability
  - [ ] Assert: Slots generados en BD, status Available

- [ ] `GenerateTimeSlots_WithBreak_SkipsBreakTime`
  - [ ] Lunes 09:00-18:00, break 13:00-14:00, duración 30min, buffer 10min
  - [ ] Verificar que NO hay slot en 13:00-14:00
  - [ ] Verificar slot anterior termina en 12:50, siguiente empieza en 14:00

- [ ] `GenerateTimeSlots_MultiDays_GeneratesForAll`
  - [ ] Config con múltiples días (Lunes, Miércoles, Viernes)
  - [ ] Verificar slots generados solo para esos días
  - [ ] 60 días de anticipación

- [ ] `ValidateAvailabilityConfiguration_WithoutWorkingDays_ReturnsFalse`
  - [ ] Todos los días con IsWorking = false
  - [ ] Debe fallar validación

- [ ] `ValidateAvailabilityConfiguration_InvalidTimeRange_ReturnsFalse`
  - [ ] StartTime > EndTime
  - [ ] Debe fallar validación

- [ ] `GetAvailableSlotsForDate_ReturnsOnlyAvailableSlots`
  - [ ] Algunos slots Reserved, otros Available
  - [ ] Query retorna solo Available

#### AvailabilityControllerTests
- [ ] `ConfigureAvailability_WithValidData_Returns200`
  - [ ] POST /api/availability/configure
  - [ ] Retorna HTTP 200 con slots

- [ ] `ConfigureAvailability_Unauthorized_Returns401`
  - [ ] Sin token JWT
  - [ ] Retorna HTTP 401

- [ ] `GetAvailability_WithValidProviderId_Returns200`
  - [ ] GET /api/availability/{providerId}
  - [ ] Retorna configuración actual

### Component Tests - Frontend (Vitest)

#### AvailabilityConfiguratorComponent
- [ ] `should render 7 day toggles`
  - [ ] Verificar presencia de toggles (Lunes-Domingo)

- [ ] `should disable time inputs when day toggle is off`
  - [ ] Toggle day OFF
  - [ ] Inputs StartTime/EndTime deshabilitados

- [ ] `should calculate slots preview correctly`
  - [ ] Llenar forma con datos válidos
  - [ ] Click "Guardar"
  - [ ] Verificar preview muestra slots correctos

- [ ] `should show error for invalid time range`
  - [ ] StartTime = 18:00, EndTime = 09:00
  - [ ] Mostrar mensaje de error

- [ ] `should call AvailabilityService.configure on submit`
  - [ ] Mockear servicio
  - [ ] Verificar que se llama con datos correctos

#### ProfileEditorComponent
- [ ] `should require name field`
  - [ ] Nombre vacío
  - [ ] Botón Guardar deshabilitado

- [ ] `should show file preview on image upload`
  - [ ] Upload imagen
  - [ ] Mostrar preview

### E2E Tests (Playwright)
- [ ] `should complete full availability setup flow`
  - [ ] Login como profesional
  - [ ] Navegar a /dashboard/availability
  - [ ] Configurar horarios (Lun-Vie, 09:00-18:00)
  - [ ] Agregar break (13:00-14:00)
  - [ ] Seleccionar duración 30min, buffer 10min
  - [ ] Click Guardar
  - [ ] Verificar mensaje de éxito
  - [ ] Recargar página
  - [ ] Verificar configuración se persistió

- [ ] `should show preview of generated slots`
  - [ ] Llenar formulario
  - [ ] Verificar preview visible con slots correctos

### Coverage Goals
- [ ] Backend: >= 80% overall
- [ ] AvailabilityService: 85%+
- [ ] Frontend Components: 75%+

---

## 🕐 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

```
1. Backend Entities + Migration              [30 min]
   - Crear entidades
   - DbContext updates
   - Migración y verificación BD

2. Backend AvailabilityService               [1h 30min]
   - ConfigureAvailability
   - GenerateTimeSlots
   - GetAvailableSlotsForDate
   - Validadores

3. Backend Controller + DTOs                 [45 min]
   - AvailabilityController
   - DTOs TypeScript-compatible
   - Error handling

4. Backend Unit Tests                        [1h]
   - Tests de lógica crítica
   - Tests de validación
   - Cobertura 80%+

5. Frontend ProfileService                   [30 min]
   - Crear servicio
   - Interfaces TypeScript

6. Frontend Dashboard Container              [45 min]
   - Layout principal
   - Navegación

7. Frontend AvailabilityConfigurator         [2h]
   - Formulario reactivo completo
   - Vista previa de slots
   - Validaciones

8. Frontend ProfileEditor                    [45 min]
   - Formulario de perfil
   - Upload de foto

9. Frontend Component Tests                  [1h]
   - Tests de componentes
   - Mocking de servicios

10. Frontend E2E Tests (Playwright)          [1h]
    - Flow completo de usuario
    - Verificaciones visuales
```

**Total estimado:** 9-10 horas

---

## ✅ DEFINICIÓN DE COMPLETITUD

Sprint 2 se considera **COMPLETADO** cuando:

- [ ] Todas las entidades creadas y migraciones ejecutadas
- [ ] AvailabilityService implementado con lógica de cálculo de slots
- [ ] Controller expone 3 endpoints (POST configure, GET config, PUT update)
- [ ] DTOs validados con FluentValidation
- [ ] Dashboard renderiza correctamente
- [ ] AvailabilityConfigurator permite configurar completo
- [ ] ProfileEditor permite editar datos públicos
- [ ] Unit tests: 80%+ cobertura en backend
- [ ] Component tests: 75%+ en frontend
- [ ] E2E test: Flow completo funciona
- [ ] Sin errores de compilación (backend y frontend)
- [ ] Verificación manual en navegador: http://localhost:4200/dashboard
- [ ] Verificación manual de BD: tablas créadas con datos

---

## 📝 NOTAS IMPORTANTES

1. **Generación de Slots es Crítica**
   - Este algoritmo se reutiliza en Sprint 3
   - Probar exhaustivamente con diferentes configuraciones
   - Edge cases: duración rara (23 min), buffer grande (60 min), etc.

2. **Validaciones**
   - Frontend: UX feedback inmediato
   - Backend: Seguridad, validar siempre en servidor
   - Ambas capas, nunca confiar solo en cliente

3. **Relaciones a Futuro**
   - TimeSlot se relacionará con Appointment en Sprint 3
   - Asegurar que FK ProviderId esté correcta

4. **Fechas en UTC**
   - Todos los datetimeoffset deben ser UtcNow
   - Frontend convierte a tiempo local del usuario para UI

5. **Testing**
   - No omitir tests aunque parezca tedioso
   - Son parte de la Definition of Done
   - Casos edge son donde está el verdadero valor

---

## 🚀 SIGUIENTE PASO

Una vez completado Sprint 2, pasamos a **Sprint 3: Agendamiento de Citas (CRÍTICO)**, que usa:
- TimeSlots generados en Sprint 2
- Validaciones de disponibilidad
- Transacciones atómicas para evitar double-booking
