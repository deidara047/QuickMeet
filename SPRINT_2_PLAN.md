# Sprint 2: Gestión de Perfil y Disponibilidad

**Estado:** En Progreso (Fase 4)  
**Duración estimada:** 7-8 horas  
**Inicio:** Enero 1, 2026  
**Fase 1 Completada:** ✅ Enero 1, 2026 - Entidades y Migraciones  
**Fase 2 Completada:** ✅ Enero 1, 2026 - Servicios Backend  
**Fase 3 Completada:** ✅ Enero 1, 2026 - AvailabilityController  
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

### E2E Tests - Backend (Playwright con HttpClient)

#### Complete Availability Setup Flow (Backend Only)
- [ ] `should_complete_full_availability_configuration_flow`
  - [ ] **Setup:** Register provider + obtain JWT token
  - [ ] **Step 1:** POST `/api/availability/configure` with valid multi-day config
    - [ ] Data: Lunes-Viernes, 09:00-18:00, break 13:00-14:00, duration 30min, buffer 10min
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Response contains 250 generated TimeSlots (5 days × 50 slots)
    - [ ] Assert: Status = "Available" for all slots
  - [ ] **Step 2:** Verify database persistence
    - [ ] Query ProviderAvailabilities table → should have 5 records (one per day)
    - [ ] Query TimeSlots table → should have ~250 records
    - [ ] Query Breaks table → should have 5 records (one per working day)
  - [ ] **Step 3:** GET `/api/availability/{providerId}` with token
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Response matches configuration sent
  - [ ] **Step 4:** Update configuration
    - [ ] PUT `/api/availability/{providerId}` with new schedule (reduced hours)
    - [ ] Assert: HTTP 200 received
    - [ ] Assert: Old TimeSlots are deleted
    - [ ] Assert: New TimeSlots generated with new schedule
  - [ ] **Cleanup:** Verify final state in database

#### Authorization & Security Edge Cases
- [ ] `should_reject_unauthorized_availability_requests`
  - [ ] POST without token → HTTP 401
  - [ ] POST with expired token → HTTP 401
  - [ ] POST with invalid token → HTTP 401
  - [ ] GET with another provider's token → HTTP 403 (Forbidden)
  - [ ] PUT with different provider's token → HTTP 403 (Forbidden)

#### Data Validation at API Boundary
- [ ] `should_reject_invalid_configurations_at_api_level`
  - [ ] POST with no working days → HTTP 400 + error message
  - [ ] POST with StartTime > EndTime → HTTP 400 + error message
  - [ ] POST with break outside working hours → HTTP 400 + error message
  - [ ] POST with negative buffer → HTTP 400 + error message
  - [ ] POST with zero appointment duration → HTTP 400 + error message
  - [ ] POST with malformed JSON → HTTP 400

#### Concurrent Request Handling
- [ ] `should_handle_concurrent_availability_updates`
  - [ ] Provider A updates availability simultaneously with Provider B
    - [ ] Assert: Both requests succeed (HTTP 200)
    - [ ] Assert: Data is not mixed in database
    - [ ] Assert: Each provider's slots match their config
  - [ ] Same provider sends two POST requests rapidly
    - [ ] Assert: Second request overwrites first (idempotent)
    - [ ] Assert: Database has only latest configuration

#### Time Zone & DateTime Consistency
- [ ] `should_generate_slots_in_utc_iso8601_format`
  - [ ] POST configuration with times (09:00, 18:00)
  - [ ] Assert: Response TimeSlots have ISO 8601 format (e.g., "2026-01-15T09:00:00Z")
  - [ ] Assert: All dates end with "Z" (UTC indicator)
  - [ ] Query database directly
  - [ ] Assert: Stored dates are datetimeoffset in UTC
  - [ ] Assert: No timezone conversion issues

#### Coverage Goals
- [ ] Backend Unit Tests: >= 80% overall
- [ ] AvailabilityService: 85%+
- [ ] AvailabilityController: 90%+
- [ ] Backend E2E: 100% of happy path + critical edge cases
- [ ] Frontend Components: 75%+ (after frontend implementation)

---

## 🕐 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

```
FASE BACKEND (COMPLETADA)
═══════════════════════════

1. Backend Entities + Migration              [30 min] ✅
   - Crear entidades
   - DbContext updates
   - Migración y verificación BD

2. Backend AvailabilityService               [1h 30min] ✅
   - ConfigureAvailability
   - GenerateTimeSlots
   - GetAvailableSlotsForDate
   - Validadores

3. Backend Controller + DTOs                 [45 min] ✅
   - AvailabilityController
   - DTOs TypeScript-compatible
   - Error handling

4. Backend Unit Tests                        [1h] ✅
   - Tests de lógica crítica
   - Tests de validación
   - Cobertura 80%+

5. Backend E2E Tests (Playwright + HttpClient) [1.5h] ⏳ [DEBE EJECUTARSE ANTES DE FRONTEND]
   - Complete availability configuration flow
   - Authorization & security edge cases
   - Data validation at API boundary
   - Concurrent request handling
   - UTC/ISO 8601 format verification


FASE FRONTEND (EN PROGRESO)
═════════════════════════════

6. Frontend ProfileService                   [30 min]
   - Crear servicio
   - Interfaces TypeScript
   - Métodos: updateProfile, getProfile, uploadPhoto

7. Frontend Dashboard Container              [45 min]
   - Layout principal
   - Inyección de dependencias
   - Navegación a ProfileEditor y AvailabilityConfigurator

8. Frontend AvailabilityConfigurator         [2h]
   - Formulario reactivo con FormArray
   - Vista previa de slots (cálculo local)
   - Validaciones en tiempo real
   - Manejo de breaks
   - Save/Update API calls

9. Frontend ProfileEditor                    [45 min]
   - Formulario de perfil
   - Upload de foto con preview
   - Validaciones
   - Save API calls

10. Frontend Component Tests (Vitest)        [1h 30min]
    - Tests de AvailabilityConfigurator
    - Tests de ProfileEditor
    - Mocking de servicios
    - Validación de formularios

11. Frontend E2E Tests (Playwright)          [1.5h] ⏳ [FASE FINAL DE SPRINT 2]
    - Complete dashboard setup flow
    - Form validation & error handling
    - Responsive design verification
    - Performance & loading states
    - Integration with Profile Service
    - Reload & persistence verification
```

**Total estimado:** 10-11 horas (Backend ✅ completado, Frontend 6-7h restantes)

---

## ✅ DEFINICIÓN DE COMPLETITUD

Sprint 2 se considera **COMPLETADO** cuando:

- [x] Todas las entidades creadas y migraciones ejecutadas ✅
- [x] AvailabilityService implementado con lógica de cálculo de slots ✅
- [x] Controller expone 3 endpoints (POST configure, GET config, PUT update) ✅
- [x] DTOs validados con FluentValidation ✅
- [ ] Backend E2E tests ejecutados y TODOS pasando ⏳ **[BLOQUEADOR ANTES DE FRONTEND]**
- [ ] Dashboard renderiza correctamente (Fase 4)
- [ ] AvailabilityConfigurator permite configurar completo (Fase 4)
- [ ] ProfileEditor permite editar datos públicos (Fase 4)
- [ ] Unit tests: 80%+ cobertura en backend ✅
- [ ] Component tests: 75%+ en frontend (Fase 4)
- [ ] E2E test Frontend: Flow completo funciona (Fase 4 Final)
- [ ] Sin errores de compilación (backend y frontend)
- [ ] Verificación manual en navegador: http://localhost:4200/dashboard
- [ ] Verificación manual de BD: tablas creadas con datos
- [ ] Backend E2E resultados documentados
- [ ] Frontend E2E resultados documentados

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
