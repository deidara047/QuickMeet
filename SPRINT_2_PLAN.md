# Sprint 2: Gestión de Perfil y Disponibilidad

**Estado:** En Progreso (Fase 6 - Backend E2E)  
**Duración estimada:** 10-11 horas total (6.5h Backend + 7.5-8.5h Frontend)  
**Inicio:** Enero 1, 2026  
**Fase 1 Completada:** ✅ Enero 1, 2026 - Entidades y Migraciones  
**Fase 2 Completada:** ✅ Enero 1, 2026 - Servicios Backend  
**Fase 3 Completada:** ✅ Enero 1, 2026 - AvailabilityController  
**Fase 4 Completada:** ✅ Enero 1, 2026 - Integration Tests Backend
**Fase 5 Completada:** ✅ Enero 3, 2026 - AvailabilityControllerIntegrationTests  
**Fase 6 En Progreso:** ⏳ E2E Backend (Broche de Oro Backend)  
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

## 🌐 FASE 3: Controller ✅ COMPLETADA

### AvailabilityController (API/Controllers)
- [x] Crear clase `AvailabilityController`
  - [x] POST `/api/availability/configure` [Autorizado]
  - [x] GET `/api/availability/{providerId}` [Autorizado]
  - [x] PUT `/api/availability/{providerId}` [Autorizado]
  - [x] GET `/api/availability/slots/{providerId}` [Público]

- [x] Manejo de errores
  - [x] Try-catch para excepciones
  - [x] Retornar HTTP 400 para validaciones fallidas
  - [x] Retornar HTTP 401 para no autorizados
  - [x] Retornar HTTP 500 para errores del servidor

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

#### AvailabilityServiceTests ✅ COMPLETADO
- [x] `ConfigureAvailability_WithValidData_GenerateSlotsSuccessfully`
  - [x] Arrange: Profesional ID, AvailabilityConfigDto válido
  - [x] Act: Llamar ConfigureAvailability
  - [x] Assert: Slots generados en BD, status Available

- [x] `GenerateTimeSlots_WithBreak_SkipsBreakTime`
  - [x] Lunes 09:00-18:00, break 13:00-14:00, duración 30min, buffer 10min
  - [x] Verificar que NO hay slot en 13:00-14:00
  - [x] Verificar slot anterior termina en 12:50, siguiente empieza en 14:00

- [x] `GenerateTimeSlots_MultiDays_GeneratesForAll`
  - [x] Config con múltiples días (Lunes, Miércoles, Viernes)
  - [x] Verificar slots generados solo para esos días
  - [x] 60 días de anticipación

- [x] `ValidateAvailabilityConfiguration_WithoutWorkingDays_ReturnsFalse`
  - [x] Todos los días con IsWorking = false
  - [x] Debe fallar validación

- [x] `ValidateAvailabilityConfiguration_InvalidTimeRange_ReturnsFalse`
  - [x] StartTime > EndTime
  - [x] Debe fallar validación

- [x] `GetAvailableSlotsForDate_ReturnsOnlyAvailableSlots`
  - [x] Algunos slots Reserved, otros Available
  - [x] Query retorna solo Available

#### AvailabilityControllerTests ✅ COMPLETADO
- [x] `ConfigureAvailability_WithValidData_Returns200` - Verificado
  - [x] POST /api/availability/configure
  - [x] Retorna HTTP 200 con slots generados
  - [x] Persiste en base de datos correctamente

- [x] `ConfigureAvailability_Unauthorized_Returns401` - Verificado
  - [x] Sin token JWT retorna 401 Unauthorized
  - [x] Con token inválido retorna 401 Unauthorized

- [x] `GetAvailability_WithValidProviderId_Returns200` - Verificado
  - [x] GET /api/availability/{providerId}
  - [x] Retorna configuración actual existente

- [x] Validaciones de negocio - Verificadas
  - [x] Sin días trabajados → BadRequest 400
  - [x] Rango de horas inválido → BadRequest 400
  - [x] Break fuera de horas → BadRequest 400

- [x] UpdateAvailability - Verificado
  - [x] PUT /api/availability/{providerId} actualiza correctamente
  - [x] Requiere autorización (401 sin token)

### E2E Tests - Backend (xUnit + HttpClient) [BROCHE DE ORO BACKEND]

**Patrón:** Heredan de `IntegrationTestBase`, prueban flujos completos de usuario vía HTTP real

#### Complete Availability Configuration Flow
- [ ] `E2E_FullAvailabilitySetup_Success`
  - [ ] **Setup:** Register provider + obtain JWT token
  - [ ] **Step 1:** POST `/api/availability/configure` with valid multi-day config
    - [ ] Data: Lunes-Viernes, 09:00-18:00, break 13:00-14:00, duration 30min, buffer 10min
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Response body contains configuration data
  - [ ] **Step 2:** Verify database persistence
    - [ ] Query ProviderAvailabilities table → verify 5 records (one per day)
    - [ ] Query TimeSlots table → verify ~250 records generated
    - [ ] Query Breaks table → verify 5 records (one per working day)
  - [ ] **Step 3:** GET `/api/availability/{providerId}` with token
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Response matches configuration sent
  - [ ] **Step 4:** Update configuration
    - [ ] PUT `/api/availability/{providerId}` with new schedule (reduced hours)
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Database shows old slots deleted
    - [ ] Assert: New TimeSlots generated with new schedule
  - [ ] **Cleanup:** Verify final state in database

#### Authorization & Security Edge Cases
- [ ] `E2E_UnauthorizedRequests_Fail`
  - [ ] POST without token → Assert HTTP 401
  - [ ] POST with expired token → Assert HTTP 401
  - [ ] POST with invalid token format → Assert HTTP 401
  - [ ] GET with another provider's token → Assert HTTP 403 Forbidden
  - [ ] PUT with different provider's token → Assert HTTP 403 Forbidden

#### Data Validation at API Boundary
- [ ] `E2E_InvalidConfigurations_ReturnBadRequest`
  - [ ] POST with no working days → Assert HTTP 400 + error message
  - [ ] POST with StartTime > EndTime → Assert HTTP 400 + error message
  - [ ] POST with break outside working hours → Assert HTTP 400 + error message
  - [ ] POST with negative buffer → Assert HTTP 400 + error message
  - [ ] POST with zero appointment duration → Assert HTTP 400 + error message
  - [ ] POST with malformed JSON → Assert HTTP 400

#### Concurrent Request Handling
- [ ] `E2E_ConcurrentUpdates_HandledCorrectly`
  - [ ] Register Provider A and Provider B simultaneously
  - [ ] Provider A updates availability while Provider B updates
    - [ ] Assert: Both requests succeed (HTTP 200)
    - [ ] Assert: Data is not mixed in database
    - [ ] Assert: Each provider's slots match their config
  - [ ] Same provider sends two POST requests rapidly (idempotency)
    - [ ] Assert: Second request overwrites first
    - [ ] Assert: Database has only latest configuration

#### Time Zone & DateTime Consistency
- [ ] `E2E_TimeSlots_GeneratedInUTCISO8601`
  - [ ] POST configuration with times (09:00, 18:00)
  - [ ] GET response TimeSlots
    - [ ] Assert: Response format is ISO 8601 (e.g., "2026-01-15T09:00:00Z")
    - [ ] Assert: All dates end with "Z" (UTC indicator)
  - [ ] Query database directly via DbContext
    - [ ] Assert: Stored dates are datetimeoffset in UTC
    - [ ] Assert: No timezone conversion issues

---

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

### E2E Tests - Frontend (Playwright en Navegador) [FASE 4 FINAL]

#### Complete User Journey: Profile Setup & Availability Configuration
- [ ] `should_complete_full_dashboard_setup_flow`
  - [ ] **Precondition:** Provider account exists, logged in, at /dashboard
  - [ ] **Section 1 - Profile Editor**
    - [ ] Visible: Name field, Description textarea, Phone input, Photo upload, Duration select
    - [ ] Fill: Name = "Dr. Juan Pérez", Description = "Especialista en...", Phone = "+34 612 345 678"
    - [ ] Upload: Valid JPG/PNG image (< 5MB)
    - [ ] Assert: Image preview shows immediately
    - [ ] Click: "Guardar Perfil"
    - [ ] Assert: Success toast notification appears
    - [ ] Refresh page
    - [ ] Assert: Profile data persists (name, phone visible)
  
  - [ ] **Section 2 - Availability Configurator**
    - [ ] **Step 1: Working Hours**
      - [ ] Assert: 7 day toggles visible (Lun-Dom)
      - [ ] Toggle ON: Lunes, Miércoles, Viernes
      - [ ] Assert: StartTime/EndTime inputs appear for those days
      - [ ] For Lunes: Set 09:00 - 18:00
      - [ ] For Miércoles: Set 09:00 - 18:00
      - [ ] For Viernes: Set 10:00 - 17:00
      - [ ] Assert: Other days (Martes, Jueves, etc) inputs disabled
    
    - [ ] **Step 2: Add Breaks**
      - [ ] Click: "+ Agregar Break"
      - [ ] Assert: New break form appears
      - [ ] Fill: StartTime = 13:00, EndTime = 14:00
      - [ ] Click: "+ Agregar Break" again (second break)
      - [ ] Fill: StartTime = 15:00, EndTime = 15:15
      - [ ] Assert: Both breaks visible in list
      - [ ] Click delete on second break
      - [ ] Assert: Second break removed, only one remains
    
    - [ ] **Step 3: Appointment Configuration**
      - [ ] Select: Duration = "30 minutos"
      - [ ] Select: Buffer = "10 minutos"
    
    - [ ] **Step 4: Preview Generation**
      - [ ] Assert: Preview section shows slots for next 3 days
      - [ ] Verify Lunes (09:00-18:00 with break):
        - [ ] First slot: 09:00-09:30 ✓
        - [ ] Next slot: 09:40-10:10 ✓ (gap due to 10min buffer)
        - [ ] [BREAK 13:00-14:00]
        - [ ] Slot after break: 14:00-14:30 ✓
        - [ ] Last slot before end: 17:30-18:00 ✓
      - [ ] Verify no slots during break time
      - [ ] Verify Viernes (10:00-17:00) has different slot count than Lunes
      - [ ] Verify Jueves NOT shown (not working day)
    
    - [ ] **Step 5: Save Configuration**
      - [ ] Click: "Guardar Disponibilidad"
      - [ ] Assert: Loading spinner appears
      - [ ] Wait for response
      - [ ] Assert: Success toast: "Disponibilidad actualizada exitosamente"
      - [ ] Assert: Form remains with saved data
  
  - [ ] **Verification: Reload & Persistence**
    - [ ] Refresh page (F5)
    - [ ] Assert: Redirected to /dashboard (not logged out)
    - [ ] Assert: Profile section shows saved name/phone
    - [ ] Assert: Availability section shows all days/times exactly as configured
    - [ ] Assert: Preview recalculated correctly

#### Component Interaction & Form Validation
- [ ] `should_validate_form_inputs_before_submit`
  - [ ] **Name Field Validation**
    - [ ] Leave empty → Error: "El nombre es requerido"
    - [ ] Enter 2 chars → Error: "Mínimo 3 caracteres"
    - [ ] Enter 101 chars → Error: "Máximo 100 caracteres"
    - [ ] Enter valid name → No error, button enabled
  
  - [ ] **Time Range Validation**
    - [ ] Set StartTime = 18:00, EndTime = 09:00 (inverted)
    - [ ] Assert: Error message appears immediately
    - [ ] Assert: "Guardar" button disabled
    - [ ] Fix time range
    - [ ] Assert: Error disappears, button enabled
  
  - [ ] **Break Validation**
    - [ ] Add break with StartTime = 08:00 (before working hours 09:00)
    - [ ] Assert: Error: "El descanso debe estar dentro del horario laboral"
    - [ ] Fix break time
    - [ ] Assert: Error resolves
  
  - [ ] **All Days Toggle OFF**
    - [ ] Toggle all days to OFF
    - [ ] Try to submit
    - [ ] Assert: Error: "Debe haber al menos un día trabajado"
    - [ ] Assert: "Guardar" button disabled

#### Error Handling & Recovery
- [ ] `should_handle_error_scenarios_gracefully`
  - [ ] **API Error 400 (Validation)**
    - [ ] Configure with valid frontend form
    - [ ] Simulate backend validation failure (mock API)
    - [ ] Assert: Error toast shows: "Error de validación: [message]"
    - [ ] Assert: Form data preserved (not cleared)
    - [ ] User can fix and retry
  
  - [ ] **API Error 401 (Unauthorized)**
    - [ ] Token expires during form editing
    - [ ] Click "Guardar"
    - [ ] Assert: Redirected to /login
    - [ ] Assert: Message: "Tu sesión expiró, inicia sesión nuevamente"
  
  - [ ] **API Error 500 (Server Error)**
    - [ ] Simulate server error
    - [ ] Assert: Error toast: "Error del servidor, intenta más tarde"
    - [ ] Assert: "Reintentar" button available
    - [ ] User can retry after API recovers

#### Responsive Design & Accessibility
- [ ] `should_work_on_mobile_and_desktop_viewports`
  - [ ] **Desktop (1920x1080)**
    - [ ] All sections visible on one screen (or minimal scroll)
    - [ ] Preview section beside form (or below depending on layout)
    - [ ] All inputs accessible and properly sized
  
  - [ ] **Tablet (768x1024)**
    - [ ] Sections stack vertically
    - [ ] Form fields maintain usability
    - [ ] Buttons clearly clickable (min 48px height)
  
  - [ ] **Mobile (375x667)**
    - [ ] Vertical layout (single column)
    - [ ] All toggles/inputs accessible with touch
    - [ ] Preview might be collapsed/expandable
    - [ ] No horizontal scroll

#### Performance & Loading States
- [ ] `should_display_loading_and_success_states`
  - [ ] **Initial Load**
    - [ ] Page loads with skeleton/spinner
    - [ ] Assert: Dashboard renders within 2 seconds
    - [ ] Assert: Profile data appears
    - [ ] Assert: Availability form appears
  
  - [ ] **Form Submission**
    - [ ] Click "Guardar Disponibilidad"
    - [ ] Assert: "Guardar" button becomes disabled + shows loading spinner
    - [ ] Assert: Form inputs become read-only during submission
    - [ ] Wait for response (timeout after 5 seconds)
    - [ ] Assert: Button re-enabled, spinner gone
    - [ ] Assert: Toast notification shows result

#### Integration with Profile Service
- [ ] `should_sync_profile_and_availability_data`
  - [ ] Update profile name → API call succeeds
  - [ ] Update availability config → API call succeeds
  - [ ] GET profile endpoint → returns latest name
  - [ ] GET availability endpoint → returns latest config
  - [ ] Verify no data mixing between endpoints

#### Coverage Goals
- [ ] Backend Unit Tests: >= 80% overall
- [ ] AvailabilityService: 85%+
- [ ] AvailabilityController: 90%+
- [ ] Backend E2E (Integration): 100% of happy path + critical edge cases (xUnit + HttpClient)
- [ ] Frontend Components: 75%+ (after frontend implementation)

---

## 🕐 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

```
╔════════════════════════════════════════════════════════════════════════════╗
║                   FASE BACKEND - CIERRE CON E2E                           ║
╚════════════════════════════════════════════════════════════════════════════╝

1. Backend Entities + Migration              [30 min] ✅ COMPLETADO
   - Crear entidades
   - DbContext updates
   - Migración y verificación BD

2. Backend AvailabilityService               [1h 30min] ✅ COMPLETADO
   - ConfigureAvailability
   - GenerateTimeSlots
   - GetAvailableSlotsForDate
   - Validadores

3. Backend Controller + DTOs                 [45 min] ✅ COMPLETADO
   - AvailabilityController
   - DTOs TypeScript-compatible
   - Error handling

4. Backend Unit Tests                        [1h] ✅ COMPLETADO
   - Tests de lógica crítica
   - Tests de validación
   - Cobertura 80%+

5. Backend Integration Tests                 [1h] ✅ COMPLETADO
   - AvailabilityControllerIntegrationTests
   - 15 tests (happy path + validations + auth)
   - Assertions en HTTP + BD + lógica

═══════════════════════════════════════════════════════════════════════════════
6. 🎖️  BACKEND E2E TESTS - xUnit + HttpClient (BROCHE DE ORO BACKEND) [1.5h] ⏳ SIGUIENTE PASO
   - Complete availability configuration flow (register → configure → get → update)
   - Authorization & security edge cases (no token, invalid token, forbidden)
   - Data validation at API boundary (6 invalid scenarios)
   - Concurrent request handling (race conditions, idempotency)
   - UTC/ISO 8601 format verification (API response + DB storage)
   
   Patrón: Heredan de IntegrationTestBase, prueban flujos completos vía HTTP
   Naming: E2E_[Scenario]_[Result] (ej: E2E_FullAvailabilitySetup_Success)
   
   ► SI TODOS LOS E2E BACKEND PASAN: BACKEND TERMINADO ✅
   ► Si no pasan: FIX y re-run hasta pasar 100%
═══════════════════════════════════════════════════════════════════════════════


╔════════════════════════════════════════════════════════════════════════════╗
║                 FASE FRONTEND - CIERRE CON E2E                            ║
╚════════════════════════════════════════════════════════════════════════════╝

7. Frontend ProfileService                   [30 min]
   - Crear servicio en core/services/
   - Interfaces TypeScript (ProviderProfile, BreakConfig, etc)
   - Métodos: updateProfile, getProfile, uploadPhoto

8. Frontend Models & DTOs                    [20 min]
   - ProviderProfile interface
   - AvailabilityConfig interface
   - TimeSlot interface
   - Align con backend response format

9. Frontend Dashboard Container              [45 min]
   - ng generate component features/dashboard/dashboard
   - Layout principal con sections
   - Inyección: AuthService, ProfileService, AvailabilityService
   - Componentes hijos: ProfileEditor, AvailabilityConfigurator

10. Frontend ProfileEditor Component         [45 min]
    - ng generate component features/dashboard/profile-editor
    - Formulario reactivo con nombre, descripción, teléfono, foto
    - Upload con preview
    - Validaciones en tiempo real
    - Save → ProfileService.updateProfile()

11. Frontend AvailabilityConfigurator        [2h]
    - ng generate component features/dashboard/availability-configurator
    - Formulario reactivo con FormArray
    - 7 day toggles con controles de tiempo
    - Add/remove breaks functionality
    - Duración y buffer selects
    - Local preview calculation
    - Save → AvailabilityService.configure()

12. Frontend Component Tests (Vitest)        [1h 30min]
    - AvailabilityConfiguratorComponent tests
    - ProfileEditorComponent tests
    - Mocking de servicios
    - Validación de formularios
    - Coverage 75%+

═══════════════════════════════════════════════════════════════════════════════
13. 🎖️  FRONTEND E2E TESTS (BROCHE DE ORO TOTAL)  [1.5h] ⏳ FASE FINAL
    - Complete dashboard setup flow (Profile + Availability)
    - Form validation & error handling
    - Responsive design verification (Desktop, Tablet, Mobile)
    - Performance & loading states
    - Integration with Profile Service
    - Reload & persistence verification
    
    ► SI TODOS LOS E2E FRONTEND PASAN: SPRINT 2 COMPLETADO ✅✅✅
    ► Si no pasan: FIX y re-run hasta pasar 100%
═══════════════════════════════════════════════════════════════════════════════
```

**Total estimado:** 10-11 horas
- Backend: 5 horas (Fases 1-5) + 1.5h E2E Backend (Fase 6) = 6.5h ✅ [EN PROGRESO]
- Frontend: 6-7 horas (Fases 7-12) + 1.5h E2E Frontend (Fase 13) = 7.5-8.5h [PRÓXIMO]

---

## ✅ DEFINICIÓN DE COMPLETITUD

### FASE BACKEND COMPLETA CUANDO:
- [x] Fases 1-5 completadas ✅
- [x] Unit tests: 41 tests pasando (26 Auth + 15 Availability) ✅
- [x] Integration tests: 15 tests pasando (AvailabilityController) ✅
- [ ] **FASE 6: E2E Backend: TODOS los tests pasando** ⏳ **[BROCHE DE ORO BACKEND]**

### FASE FRONTEND COMPLETA CUANDO:
- [ ] Fases 7-12 completadas (Servicios + Componentes + Unit Tests)
- [ ] Component tests: 75%+ cobertura
- [ ] **FASE 13: E2E Frontend: TODOS los tests pasando** ⏳ **[BROCHE DE ORO FRONTEND - CIERRE DE SPRINT 2]**

### SPRINT 2 COMPLETADO CUANDO:
✅ BACKEND:
- [x] Entidades y migraciones ejecutadas
- [x] AvailabilityService con lógica completa
- [x] Controller con 4 endpoints
- [x] DTOs validados con FluentValidation
- [x] Unit tests pasando
- [x] Integration tests pasando
- [ ] **E2E Backend tests PASANDO** ⏳ [PENDIENTE]

✅ FRONTEND:
- [ ] Dashboard, ProfileEditor, AvailabilityConfigurator implementados
- [ ] Component tests pasando
- [ ] **E2E Frontend tests PASANDO** ⏳ [PENDIENTE]

✅ GENERAL:
- [ ] Sin errores de compilación (backend y frontend)
- [ ] Verificación manual en navegador: http://localhost:4200/dashboard
- [ ] Verificación manual de BD: tablas creadas y datos persistidos
- [ ] Backend E2E resultados documentados
- [ ] Frontend E2E resultados documentados
- [ ] **PROYECTO LISTO PARA SPRINT 3** 🚀

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
