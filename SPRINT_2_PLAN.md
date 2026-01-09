# Sprint 2: Gestión de Perfil y Disponibilidad

**Objetivo:** Implementar configuración de perfil público y sistema de disponibilidad con generación automática de slots

**Duración Total:** 10-11 horas (6.5h Backend ✅ + 7.5-8.5h Frontend ⏳)

---

## 🎯 ESTADO ACTUAL (9 Enero 2026)

### ✅ BACKEND: COMPLETADO (Fases 1-6 + E2E)
- **Fase 1-3:** Entidades, servicios, controllers
- **Fase 4-5:** Unit tests (258 unitarios + integration tests)
- **Fase 6:** E2E Backend (14 E2E tests ProvidersController)
- **Total:** 275 tests pasando ✅

**Resultado:** Backend 100% operacional, DB con tablas, servicios listos + E2E coverage

### ⏳ FRONTEND: EN PROGRESO
- **Fases 7-8:** ✅ COMPLETADO (ProfileService + Models existentes)
- **Fases 9-11:** ✅ COMPLETADO (Dashboard, ProfileEditor, AvailabilityConfigurator básicos)
- **Fase 12:** ⏳ TODO (Component & Service Tests - 98 tests planeados)
- **Fase 13:** ⏳ TODO (E2E Playwright)

**Próximo Paso:** Comenzar Fase 12 - Component Tests (AuthService.spec.ts BLOQUEADOR)

---

## � BACKEND: FASES 1-6 (REFERENCIA - COMPLETADO)

Todas las fases backend (entidades, migraciones, servicios, controllers, unit tests, integration tests, E2E) completadas el 3 Enero 2026.

## 🎨 FRONTEND: FASES 7-13 (TRABAJO ACTUAL)

### FASE 7: ProfileService [30 min] ✅ COMPLETADO

**Backend (ProvidersController + ProviderService):**
- [x] Crear `ProvidersController.cs` con patrón similar a AuthController
- [x] Crear `IProviderService.cs` interface
- [x] Crear `ProviderService.cs` con lógica de negocio
- [x] Registrar en `Program.cs`
- [x] Endpoints implementados:
  - [x] `GET /api/providers/{providerId}` - obtener perfil
  - [x] `PUT /api/providers/{providerId}` - actualizar perfil
  - [x] `POST /api/providers/{providerId}/photo` - subir foto
- [x] Validaciones: auth, autorización (owner check), extensiones de imagen

**Frontend (ProfileService ya existía):**
- [x] `ProfileService` ya implementado en `core/services/profile.service.ts`
- [x] Métodos:
  - [x] `getProfile(providerId): Observable<ProviderProfile>`
  - [x] `updateProfile(providerId, profile): Observable<ProviderProfile>`
  - [x] `uploadPhoto(providerId, file): Observable<{ photoUrl: string }>`
- [x] Inyección HttpClient y uso de endpoints correctos

---

### FASE 8: Models & DTOs TypeScript [20 min] ⏳

**Ubicación:** `src/app/shared/models/`

**Interfaces a crear:**
- [ ] `ProviderProfile`
  - [ ] id: number
  - [ ] username: string
  - [ ] email: string
  - [ ] fullName: string
  - [ ] description: string
  - [ ] phone: string
  - [ ] photoUrl: string
  - [ ] appointmentDurationMinutes: number

- [ ] `AvailabilityConfig`
  - [ ] providerId: number
  - [ ] days: DayConfig[]
  - [ ] appointmentDurationMinutes: number
  - [ ] bufferMinutes: number

- [ ] `DayConfig`
  - [ ] dayOfWeek: number (0-6)
  - [ ] isWorking: boolean
  - [ ] startTime: string (HH:mm)
  - [ ] endTime: string (HH:mm)
  - [ ] breaks: BreakConfig[]

- [ ] `BreakConfig`
  - [ ] startTime: string (HH:mm)
  - [ ] endTime: string (HH:mm)

- [ ] `TimeSlot` (ya debería existir)
  - [ ] id: number
  - [ ] startTime: Date | string (ISO 8601)
  - [ ] endTime: Date | string (ISO 8601)
  - [ ] status: 'Available' | 'Reserved' | 'Blocked'
  - [ ] providerId: number

---

### FASE 9: Dashboard Container [45 min] ⏳

**Comando:**
```bash
ng generate component features/dashboard/dashboard
```

**Estructura de archivos:**
```
dashboard/
├── dashboard.component.ts
├── dashboard.component.html
└── dashboard.component.css
```

**Tareas:**
- [ ] Inyectar servicios:
  - [ ] AuthService
  - [ ] ProfileService
  - [ ] AvailabilityService

- [ ] En ngOnInit():
  - [ ] Cargar perfil: `profileService.getProfile()`
  - [ ] Almacenar en variable: `currentProfile: ProviderProfile`

- [ ] HTML Layout (PrimeNG):
  - [ ] p-card principal
  - [ ] Sección 1: Nombre y enlace público
    - [ ] Mostrar: "{{ currentProfile.fullName }}"
    - [ ] Enlace: `quickmeet.app/{{ currentProfile.username }}`
    - [ ] Botón copiar: Copy to clipboard
  
  - [ ] Sección 2: Dos columnas
    - [ ] Col izq: `<app-profile-editor>`
    - [ ] Col der: `<app-availability-configurator>`

- [ ] Rutas protegidas:
  - [ ] Usar authGuard
  - [ ] Redirect a /login si no autenticado

---

### FASE 10: ProfileEditorComponent [45 min] ⏳

**Comando:**
```bash
ng generate component features/dashboard/profile-editor
```

**Estructura de archivos:**
```
profile-editor/
├── profile-editor.component.ts
├── profile-editor.component.html
└── profile-editor.component.css
```

**Tareas:**

- [ ] FormBuilder: Crear Reactive Form
  - [ ] fullName: [required, minLength(3), maxLength(100)]
  - [ ] description: [maxLength(500)]
  - [ ] phone: [optional, pattern(/^\+?[0-9\s\-]{9,}$/)]
  - [ ] appointmentDurationMinutes: [required, (15,30,45,60)]

- [ ] File Upload Photo
  - [ ] Input file accept="image/*"
  - [ ] Change event → preview en img tag
  - [ ] Max 5MB validation

- [ ] Validaciones en tiempo real
  - [ ] Mostrar errores debajo de cada input
  - [ ] Deshabilitar botón "Guardar" si form inválido
  - [ ] Toast notification de success/error

- [ ] Botón "Guardar Perfil"
  - [ ] On click: `profileService.updateProfile(form.value)`
  - [ ] Show loading spinner durante submit
  - [ ] On success: Toast "Perfil actualizado"
  - [ ] On error: Toast con error message

- [ ] CSS: Usar TailwindCSS + PrimeNG
  - [ ] No CSS puro innecesario
  - [ ] Responsive (mobile-first)

---

### FASE 11: AvailabilityConfiguratorComponent [2h] 🔴 CRÍTICA

**Comando:**
```bash
ng generate component features/dashboard/availability-configurator
```

**Estructura de archivos:**
```
availability-configurator/
├── availability-configurator.component.ts
├── availability-configurator.component.html
└── availability-configurator.component.css
```

**Tareas:**

- [ ] FormBuilder: Crear Form con FormArray
  ```typescript
  form = this.fb.group({
    days: this.fb.array([...]), // 7 FormGroups
    appointmentDurationMinutes: [30, required],
    bufferMinutes: [0, required]
  });
  ```

- [ ] Sección 1: Horas de Trabajo (7 días)
  - [ ] Para cada día (Lunes-Domingo):
    - [ ] p-toggleswitch: isWorking
    - [ ] [disabled]: cuando isWorking = false
    - [ ] p-inputtext: startTime (HH:mm format)
    - [ ] p-inputtext: endTime (HH:mm format)
    - [ ] Validador: startTime < endTime (cuando isWorking = true)
  
  - [ ] Validación global: Al menos 1 día debe estar working
    - [ ] Error message: "Debe haber al menos un día de trabajo"
    - [ ] Deshabilitar submit si no hay días

- [ ] Sección 2: Descansos (Breaks)
  - [ ] Botón "+ Agregar Descanso"
  - [ ] FormArray anidado para breaks
  - [ ] Para cada break:
    - [ ] p-inputtext: startTime (HH:mm)
    - [ ] p-inputtext: endTime (HH:mm)
    - [ ] p-button: "Eliminar"
  - [ ] Validación: Break debe estar dentro de horarios working
    - [ ] Validador personalizado: BreakValidator
  - [ ] Mostrar error si break traslapado o fuera de horario

- [ ] Sección 3: Configuración de Citas
  - [ ] p-dropdown: appointmentDurationMinutes
    - [ ] Opciones: [15, 30, 45, 60] minutos
  - [ ] p-dropdown: bufferMinutes
    - [ ] Opciones: [0, 5, 10, 15] minutos

- [ ] Sección 4: Vista Previa de Slots
  - [ ] Disparador: `form.valueChanges | debounceTime(500)`
  - [ ] Llamar: `availabilityService.generatePreview(formValue)`
  - [ ] Mostrar: Próximos 3 días de ejemplo
  - [ ] Formato: Usar DisplaySlotPipe
  - [ ] Layout:
    ```
    Viernes 3 Enero:
      09:00-09:30 ✓
      09:40-10:10 ✓
      [BREAK 13:00-14:00]
      14:00-14:30 ✓
    ```

- [ ] Botón "Guardar Disponibilidad"
  - [ ] On click: `availabilityService.configure(form.value)`
  - [ ] Show loading spinner
  - [ ] Disable form during submit
  - [ ] On success: Toast "Disponibilidad configurada"
  - [ ] On error: Toast con error message
  - [ ] Preserve form data en caso de error (para retry)

- [ ] Validadores Personalizados
  - [ ] Ubicación: `src/app/shared/validators/`
  - [ ] `TimeRangeValidator`: startTime < endTime
  - [ ] `BreakValidator`: break dentro de horario working
  - [ ] `AtLeastOneDayValidator`: Al menos 1 día con isWorking=true

---

### FASE 12: Component & Service Tests (Vitest) [2h 30min] ⏳

**Status:** ✅ Vitest ya configurado + DisplaySlotPipe.spec.ts completado

**Configuración inicial Vitest (YA HECHO):**
- [x] `vitest.config.ts` en raíz del proyecto
- [x] `src/test.ts` con setup de TestBed
- [x] `package.json` scripts (test, test:run, test:coverage)
- [x] DisplaySlotPipe.spec.ts (263 líneas, 100% coverage)

**Estrategia de Testing:**
1. **Pipes:** SIN TestBed, instanciación directa (DisplaySlotPipe ya hecho ✅)
2. **Servicios:** Con `HttpTestingController`, SIN TestBed para lógica pura
3. **Componentes:** Con TestBed completo + mocking de dependencias
4. **Validadores:** SIN TestBed, instanciación directa

**Orden de implementación (CRÍTICO - respeta dependencias):**

---

## 📋 CHECKLIST TESTS - SERVICIOS [~40 tests, 45 min]

### 1️⃣ AuthService Tests (BLOQUEADOR - otros tests dependen)
**Archivo:** `src/app/core/services/auth.service.spec.ts`

**Setup:**
- [ ] Importar HttpClientTestingModule, HttpTestingController
- [ ] Mock LocalStorage/SessionStorage
- [ ] Crear fixture con usuarios de prueba

**Test Suite - Login [5 tests]:**
- [ ] 1.1: debería hacer POST a `/api/auth/login` con credentials
- [ ] 1.2: debería guardar token en localStorage al login exitoso
- [ ] 1.3: debería retornar error 401 si credenciales inválidas
- [ ] 1.4: debería limpiar localStorage si login falla
- [ ] 1.5: debería actualizar currentUser$ observable

**Test Suite - Register [5 tests]:**
- [ ] 2.1: debería hacer POST a `/api/auth/register` con datos
- [ ] 2.2: debería validar formato email antes de enviar
- [ ] 2.3: debería retornar error si email duplicado (409)
- [ ] 2.4: debería retornar error si username duplicado
- [ ] 2.5: debería retornar success con provider ID

**Test Suite - Token Management [4 tests]:**
- [ ] 3.1: debería obtener token desde localStorage
- [ ] 3.2: debería verificar si token válido (no expirado)
- [ ] 3.3: debería limpiar token al logout
- [ ] 3.4: debería retornar null si token no existe

**Test Suite - User State [3 tests]:**
- [ ] 4.1: debería obtener userId actual
- [ ] 4.2: debería obtener user actual desde localStorage
- [ ] 4.3: debería verificar si usuario autenticado

**Coverage Goal:** 90%

---

### 2️⃣ ProfileService Tests
**Archivo:** `src/app/core/services/profile.service.spec.ts`

**Setup:**
- [ ] Mock HttpTestingController
- [ ] Mock file uploads

**Test Suite - Get Profile [3 tests]:**
- [ ] 1.1: debería hacer GET a `/api/providers/{id}`
- [ ] 1.2: debería mapear respuesta a ProviderProfile
- [ ] 1.3: debería retornar 404 si provider no existe

**Test Suite - Update Profile [5 tests]:**
- [ ] 2.1: debería hacer PUT a `/api/providers/{id}` con datos
- [ ] 2.2: debería actualizar solo campos no-null
- [ ] 2.3: debería retornar perfil actualizado
- [ ] 2.4: debería retornar 400 si validación falla (fullName inválido)
- [ ] 2.5: debería retornar 403 si no es propietario

**Test Suite - Upload Photo [4 tests]:**
- [ ] 3.1: debería hacer POST a `/api/providers/{id}/photo`
- [ ] 3.2: debería enviar FormData con archivo
- [ ] 3.3: debería retornar photoUrl en respuesta
- [ ] 3.4: debería retornar 400 si extensión inválida

**Coverage Goal:** 85%

---

### 3️⃣ AvailabilityService Tests
**Archivo:** `src/app/core/services/availability.service.spec.ts`

**Setup:**
- [ ] Mock HttpTestingController
- [ ] Fixture con configuraciones de disponibilidad

**Test Suite - Configure [4 tests]:**
- [ ] 1.1: debería hacer POST a `/api/availability/configure`
- [ ] 1.2: debería validar al menos 1 día de trabajo
- [ ] 1.3: debería retornar slots generados
- [ ] 1.4: debería retornar 400 si rango horarios inválido

**Test Suite - Get Config [3 tests]:**
- [ ] 2.1: debería hacer GET a `/api/availability/{id}`
- [ ] 2.2: debería mapear respuesta a AvailabilityConfig
- [ ] 2.3: debería cachear resultado

**Test Suite - Preview Generation [3 tests]:**
- [ ] 3.1: debería generar preview de slots próximos 3 días
- [ ] 3.2: debería respetar breaks en generación
- [ ] 3.3: debería aplicar appointmentDuration y buffer

**Coverage Goal:** 85%

---

### 4️⃣ ApiService Tests
**Archivo:** `src/app/core/services/api.service.spec.ts`

**Setup:**
- [ ] Mock HttpClient
- [ ] Mock interceptors

**Test Suite - HTTP Helpers [4 tests]:**
- [ ] 1.1: debería construir URL correctamente
- [ ] 1.2: debería agregar headers de autorización
- [ ] 1.3: debería manejar errores HTTP (4xx, 5xx)
- [ ] 1.4: debería serializar parámetros

**Coverage Goal:** 80%

---

## 🎨 CHECKLIST TESTS - COMPONENTES [~60 tests, 1h 15min]

### 5️⃣ DashboardComponent Tests
**Archivo:** `src/app/features/dashboard/dashboard.component.spec.ts`

**Setup:**
- [ ] Mock AuthService
- [ ] Mock ProfileService
- [ ] Mock AvailabilityService
- [ ] Mock MessageService (PrimeNG)
- [ ] Mock Router

**Test Suite - Initialization [3 tests]:**
- [ ] 1.1: debería cargar perfil en ngOnInit
- [ ] 1.2: debería mostrar error si usuario no autenticado
- [ ] 1.3: debería generar enlace público

**Test Suite - Profile Loading [4 tests]:**
- [ ] 2.1: debería llamar profileService.getProfile()
- [ ] 2.2: debería mostrar loading spinner
- [ ] 2.3: debería mostrar perfil en template
- [ ] 2.4: debería mostrar toast error si falla

**Test Suite - Public Link [2 tests]:**
- [ ] 3.1: debería mostrar `quickmeet.app/username`
- [ ] 3.2: debería tener botón copy-to-clipboard

**Coverage Goal:** 80%

---

### 6️⃣ ProfileEditorComponent Tests
**Archivo:** `src/app/features/dashboard/profile-editor/profile-editor.component.spec.ts`

**Setup:**
- [ ] Mock ProfileService
- [ ] Mock MessageService (PrimeNG)
- [ ] TestBed con standalone component

**Test Suite - Form Rendering [4 tests]:**
- [ ] 1.1: debería renderizar input fullName
- [ ] 1.2: debería renderizar textarea description
- [ ] 1.3: debería renderizar input phone
- [ ] 1.4: debería renderizar select appointmentDurationMinutes

**Test Suite - FullName Validation [4 tests]:**
- [ ] 2.1: debería requerir fullName
- [ ] 2.2: debería validar minLength(3)
- [ ] 2.3: debería validar maxLength(100)
- [ ] 2.4: debería mostrar error message en template

**Test Suite - Description Validation [2 tests]:**
- [ ] 3.1: debería aceptar description opcional
- [ ] 3.2: debería validar maxLength(500)

**Test Suite - Phone Validation [2 tests]:**
- [ ] 4.1: debería validar patrón regex `/^\+?[0-9\s\-]{9,}$/`
- [ ] 4.2: debería ser opcional

**Test Suite - Duration Validation [2 tests]:**
- [ ] 5.1: debería aceptar solo [15, 30, 45, 60] minutos
- [ ] 5.2: debería mostrar opciones en select

**Test Suite - File Upload [4 tests]:**
- [ ] 6.1: debería mostrar preview de imagen
- [ ] 6.2: debería validar max 5MB
- [ ] 6.3: debería validar extensiones jpg, png, gif, webp
- [ ] 6.4: debería rechazar archivo vacío

**Test Suite - Submit [5 tests]:**
- [ ] 7.1: debería deshabilitar botón si form inválido
- [ ] 7.2: debería llamar profileService.updateProfile() on click
- [ ] 7.3: debería mostrar loading spinner durante submit
- [ ] 7.4: debería mostrar toast success
- [ ] 7.5: debería mostrar toast error si API falla

**Test Suite - Form State [2 tests]:**
- [ ] 8.1: debería preservar form data si falla submit
- [ ] 8.2: debería permitir retry después de error

**Coverage Goal:** 85%

---

### 7️⃣ AvailabilityConfiguratorComponent Tests
**Archivo:** `src/app/features/dashboard/availability-configurator/availability-configurator.component.spec.ts`

**Setup:**
- [ ] Mock AvailabilityService
- [ ] Mock MessageService (PrimeNG)
- [ ] TestBed con standalone component
- [ ] Fixture con FormArray

**Test Suite - Day Configuration [6 tests]:**
- [ ] 1.1: debería renderizar 7 toggles (Lun-Dom)
- [ ] 1.2: debería deshabilitar time inputs cuando toggle OFF
- [ ] 1.3: debería habilitar time inputs cuando toggle ON
- [ ] 1.4: debería validar startTime < endTime
- [ ] 1.5: debería mostrar error si startTime > endTime
- [ ] 1.6: debería marcar como invalid si ambos iguales

**Test Suite - Breaks Configuration [5 tests]:**
- [ ] 2.1: debería agregar break al click "+ Agregar"
- [ ] 2.2: debería eliminar break al click "Eliminar"
- [ ] 2.3: debería validar break dentro de horario working
- [ ] 2.4: debería mostrar error si break fuera de horario
- [ ] 2.5: debería validar sin traslape entre breaks

**Test Suite - Duration & Buffer [3 tests]:**
- [ ] 3.1: debería mostrar opciones [15, 30, 45, 60] minutos
- [ ] 3.2: debería mostrar opciones [0, 5, 10, 15] minutos buffer
- [ ] 3.3: debería usar default 30min duration, 0min buffer

**Test Suite - Preview Generation [4 tests]:**
- [ ] 4.1: debería generar preview on form valueChanges
- [ ] 4.2: debería usar DisplaySlotPipe para formatear
- [ ] 4.3: debería mostrar slots próximos 3 días
- [ ] 4.4: debería ocultar slots durante breaks

**Test Suite - Validation [3 tests]:**
- [ ] 5.1: debería requerir al menos 1 día activo
- [ ] 5.2: debería mostrar error global si sin días
- [ ] 5.3: debería deshabilitar botón submit

**Test Suite - Submit [4 tests]:**
- [ ] 6.1: debería llamar availabilityService.configure() on click
- [ ] 6.2: debería deshabilitar form durante submit
- [ ] 6.3: debería mostrar loading spinner
- [ ] 6.4: debería mostrar toast success/error

**Coverage Goal:** 85%

---

## ✅ CHECKLIST TESTS - VALIDADORES [~15 tests, 20 min]

### 8️⃣ TimeRangeValidator Tests
**Archivo:** `src/app/shared/validators/time-range.validator.spec.ts`

**Setup:**
- [ ] Instanciación directa (sin TestBed)
- [ ] Fixture con controls para testing

**Test Suite [4 tests]:**
- [ ] 1.1: debería retornar null si startTime < endTime
- [ ] 1.2: debería retornar error si startTime > endTime
- [ ] 1.3: debería retornar error si startTime === endTime
- [ ] 1.4: debería manejar valores nulos

**Coverage Goal:** 90%

---

### 9️⃣ BreakValidator Tests
**Archivo:** `src/app/shared/validators/break.validator.spec.ts`

**Setup:**
- [ ] Instanciación directa
- [ ] Fixture con form groups

**Test Suite [5 tests]:**
- [ ] 1.1: debería validar break dentro de horario working
- [ ] 1.2: debería retornar error si break antes del inicio
- [ ] 1.3: debería retornar error si break después del fin
- [ ] 1.4: debería validar sin traslape entre breaks
- [ ] 1.5: debería manejar breaks múltiples

**Coverage Goal:** 90%

---

### 🔟 AtLeastOneDayValidator Tests
**Archivo:** `src/app/shared/validators/at-least-one-day.validator.spec.ts`

**Setup:**
- [ ] Instanciación directa
- [ ] Fixture con FormArray de días

**Test Suite [3 tests]:**
- [ ] 1.1: debería retornar null si al menos 1 día isWorking=true
- [ ] 1.2: debería retornar error si todos los días isWorking=false
- [ ] 1.3: debería validar FormArray completo

**Coverage Goal:** 90%

---

## 📊 RESUMEN TESTS FASE 12

| Categoría | Archivo | Tests | Líneas Est. | Status |
|-----------|---------|-------|------------|--------|
| **Pipes** | display-slot.pipe.spec.ts | 15 | 263 | ✅ DONE |
| **Servicios** | 4 archivos | 19 | ~600 | ⏳ TODO |
| **Componentes** | 3 archivos | 52 | ~1500 | ⏳ TODO |
| **Validadores** | 3 archivos | 12 | ~300 | ⏳ TODO |
| **TOTAL** | - | **98 tests** | **~2600 líneas** | ⏳ |

**Métricas Esperadas:**
- ✅ DisplaySlotPipe: 100% coverage
- 🎯 Servicios: 85%+ coverage
- 🎯 Componentes: 85%+ coverage
- 🎯 Validadores: 90%+ coverage
- **GLOBAL:** 85%+ coverage objetivo

**Tiempo Estimado:**
1. AuthService (bloqueador): 15 min
2. ProfileService: 10 min
3. AvailabilityService: 10 min
4. ApiService: 10 min
5. DashboardComponent: 15 min
6. ProfileEditorComponent: 20 min
7. AvailabilityConfiguratorComponent: 25 min
8. Validadores (3): 15 min
9. Correcciones + Coverage: 20 min

**TOTAL: ~2h 30min**

---

## 🚀 EJECUCIÓN - PASO A PASO

**Orden CRÍTICO (respetar dependencias):**
```
1. AuthService.spec.ts (bloqueador)
   ↓
2. ProfileService.spec.ts (depende de AuthService)
3. AvailabilityService.spec.ts (depende de AuthService)
4. ApiService.spec.ts (independiente)
   ↓
5. DashboardComponent.spec.ts (usa AuthService + ProfileService)
6. ProfileEditorComponent.spec.ts (usa ProfileService)
7. AvailabilityConfiguratorComponent.spec.ts (usa AvailabilityService)
   ↓
8. TimeRangeValidator.spec.ts (independiente)
9. BreakValidator.spec.ts (independiente)
10. AtLeastOneDayValidator.spec.ts (independiente)
   ↓
11. npm run test:coverage → Verificar 85%+
12. npm run test:ui → Review visual
```

**Comandos para ejecutar:**
```bash
# Modo watch (desarrollo)
npm test

# Modo UI (debugging)
npm run test:ui

# Cobertura completa
npm run test:coverage

# Test individual
npm run test:run -- profile.service.spec.ts
```

---

### FASE 13: E2E Frontend Tests (Playwright) [1.5h] 🎖️ BROCHE DE ORO

**Setup Playwright:**
- [ ] Crear `e2e/fixtures/auth.fixture.ts` - Login setup
- [ ] Crear `e2e/pages/dashboard.page.ts` - Page Object Model
- [ ] Crear `e2e/helpers/data.helper.ts` - Datos de prueba únicos

**Patrón:** Usar `storageState` para autenticación persistente entre tests

---

#### Test Suite 1: Complete Dashboard Setup Flow [1 test]

**Test: debería completar flujo completo de setup (profile + availability)**

**Precondiciones:**
- [ ] Provider registrado y autenticado
- [ ] Página `/dashboard` cargada

**Sección 1: Profile Editor (Happy Path)**
- [ ] Verificar campo nombre visible
- [ ] Llenar: "Dr. Juan Pérez"
- [ ] Llenar descripción: "Especialista en medicina general"
- [ ] Llenar teléfono: "+34 612 345 678"
- [ ] Upload imagen válida (JPG/PNG)
- [ ] Verificar preview de imagen
- [ ] Click "Guardar Perfil"
- [ ] Verificar toast success
- [ ] Refresh página
- [ ] Verificar datos persisten

**Sección 2: Availability Configurator (Happy Path)**
- [ ] Verificar 7 toggles visibles
- [ ] Toggle ON: Lunes, Miércoles, Viernes
- [ ] Lunes: 09:00 - 18:00
- [ ] Miércoles: 09:00 - 18:00
- [ ] Viernes: 10:00 - 17:00
- [ ] Click "+ Agregar Break"
- [ ] Break 1: 13:00 - 14:00
- [ ] Click "+ Agregar Break" (segunda vez)
- [ ] Break 2: 15:00 - 15:15
- [ ] Eliminar segundo break
- [ ] Duration: 30 minutos
- [ ] Buffer: 10 minutos
- [ ] Verificar preview con slots correctos
- [ ] Click "Guardar Disponibilidad"
- [ ] Verificar toast success
- [ ] Refresh página
- [ ] Verificar configuración persiste

---

#### Test Suite 2: Form Validation [4 tests]

**Test: debería validar campos de formulario antes de submit**
- [ ] Profile name requerido (error si vacío)
- [ ] Profile name min 3 chars (error si 2)
- [ ] Profile name max 100 chars (error si 101)
- [ ] Teléfono: validar patrón regex

**Test: debería validar rango de horas**
- [ ] StartTime > EndTime → mostrar error
- [ ] Botón "Guardar" deshabilitado
- [ ] Arreglar tiempo
- [ ] Error desaparece, botón habilitado

**Test: debería validar breaks dentro de horario**
- [ ] Break fuera de horario (ej: 08:00 antes de 09:00) → error
- [ ] Error: "El descanso debe estar dentro del horario laboral"
- [ ] Arreglar break time
- [ ] Error desaparece

**Test: debería requerir al menos 1 día trabajado**
- [ ] Desactivar todos los días
- [ ] Intentar submit
- [ ] Error: "Debe haber al menos un día de trabajo"
- [ ] Botón "Guardar" deshabilitado

---

#### Test Suite 3: Error Handling [3 tests]

**Test: debería manejar errores de API 400 (validación)**
- [ ] Mockear API para retornar 400
- [ ] Submit formulario válido (frontend)
- [ ] Verificar toast error: "Error de validación"
- [ ] Verificar form data preservado
- [ ] Verificar botón "Reintentar" disponible

**Test: debería manejar errores de API 401 (sesión expirada)**
- [ ] Mockear API para retornar 401
- [ ] Click "Guardar"
- [ ] Verificar redirección a `/login`
- [ ] Verificar mensaje: "Tu sesión expiró"

**Test: debería manejar errores de API 500 (servidor)**
- [ ] Mockear API para retornar 500
- [ ] Click "Guardar"
- [ ] Verificar toast error: "Error del servidor"
- [ ] Verificar botón "Reintentar"

---

#### Test Suite 4: Responsive Design [3 tests]

**Test: debería funcionar en desktop (1920x1080)**
- [ ] Verificar layout de dos columnas (Profile izq, Availability der)
- [ ] Todos los campos accesibles sin scroll horizontal
- [ ] Preview visible

**Test: debería funcionar en tablet (768x1024)**
- [ ] Verificar layout apilado (vertical)
- [ ] Verificar buttons con tamaño mínimo 48px
- [ ] Verificar form fields usables con touch

**Test: debería funcionar en mobile (375x667)**
- [ ] Verificar layout single column
- [ ] Verificar sin scroll horizontal
- [ ] Verificar todos los inputs accesibles

---

#### Test Suite 5: Data Persistence & Sync [2 tests]

**Test: debería sincronizar profile y availability data**
- [ ] Actualizar profile → GET /api/providers/me retorna datos nuevos
- [ ] Actualizar availability → GET /api/availability retorna config nueva
- [ ] Verificar no hay mezcla de datos entre endpoints

**Test: debería persistir datos en reload**
- [ ] Llenar profile y availability
- [ ] Save ambos
- [ ] F5 reload
- [ ] Verificar datos sin cambios
- [ ] Verificar estado de sesión intacto (no redirige a login)

---

#### Test Suite 6: UX & Usability [2 tests]

**Test: debería mostrar loading states y success**
- [ ] Submit profile → spinner visible
- [ ] Submit completa → spinner desaparece
- [ ] Toast success visible con mensaje
- [ ] Verificar button re-enabled

**Test: debería aplicar heurísticas de usabilidad Nielsen**
- [ ] Visibilidad de estado: Spinners, toasts informativos
- [ ] Prevención de errores: Validaciones en tiempo real
- [ ] Control del usuario: Botones "Reintentar", preservar datos
- [ ] Diseño minimalista: Sin información innecesaria
- [ ] Recuperación de errores: Mensajes claros + opciones para retry

---

## 📋 TIMELINE FINAL SPRINT 2

```
BACKEND (✅ COMPLETADO 9 Enero 2026)
├─ Fases 1-6: ✅ COMPLETADO (3 Enero)
│  ├─ 258 Unit Tests (70 validators + 23 service + 165 auth)
│  └─ 38 Integration Tests (ProvidersController)
├─ E2E Backend: ✅ COMPLETADO (9 Enero)
│  └─ 14 E2E Tests (ProvidersControllerE2ETests)
└─ TOTAL: 275 tests + 14 E2E ✅

FRONTEND (⏳ EN PROGRESO)
├─ Fases 7-8: ✅ COMPLETADO (ProfileService + Models)
├─ Fases 9-11: ✅ COMPLETADO (Dashboard + ProfileEditor + AvailabilityConfigurator)
├─ Fase 12: ⏳ TODO [2h 30min] Component & Service Tests (98 tests)
│  ├─ AuthService.spec.ts [15 min] - BLOQUEADOR
│  ├─ ProfileService.spec.ts [10 min]
│  ├─ AvailabilityService.spec.ts [10 min]
│  ├─ ApiService.spec.ts [10 min]
│  ├─ DashboardComponent.spec.ts [15 min]
│  ├─ ProfileEditorComponent.spec.ts [20 min]
│  ├─ AvailabilityConfiguratorComponent.spec.ts [25 min]
│  └─ Validadores (3 x 5 min) [15 min]
└─ Fase 13: ⏳ TODO [1h 30min] E2E Playwright (20+ tests) 🎖️

FRONTEND REMAINING: 4h (2h 30min tests + 1h 30min E2E)
```

---

## ✅ DEFINICIÓN DE COMPLETITUD SPRINT 2

**BACKEND:** ✅ COMPLETADO 9 Enero 2026
- [x] Entidades, migraciones, servicios, controllers
- [x] 258 tests unitarios pasando
- [x] 38 tests de integración pasando
- [x] 14 E2E Backend tests 100% pasando
- [x] **TOTAL: 310 tests** ✅

**FRONTEND:** ⏳ EN PROGRESO (Fase 12-13)
- [x] Fase 7-8: ProfileService + Models (completado)
- [x] Fase 9-11: Componentes (completado)
- [ ] Fase 12: Component Tests (98 tests → 85%+ coverage)
  - [ ] 4 Service tests (19 tests)
  - [ ] 3 Component tests (52 tests)
  - [ ] 3 Validator tests (12 tests)
  - [ ] 1 Pipe test (15 tests - YA HECHO ✅)
- [ ] Fase 13: E2E Playwright (20+ tests)

**SPRINT COMPLETADO CUANDO:**
- ✅ Backend: 310 tests (HECHO)
- ✅ Frontend Componentes: Compilando sin errores (HECHO)
- ⏳ Frontend Unit/Component: 98 tests, 85%+ coverage (PRÓXIMO)
- ⏳ Frontend E2E: 20+ tests, todos pasando (DESPUÉS)
- ✅ Manual verification en http://localhost:4200/dashboard
- ✅ Todos los tests pasando (unit + E2E)

**Hito Crítico:** AuthService.spec.ts bloquea todo - debe ser PRIMERO




---

## 🔬 REFERENCIA: Análisis Técnico Vitest (OPCIONAL - para entender decisiones)

**¿Por qué Vitest y no Jest para Angular 21?**

Jest requiere `zone.js` obligatoriamente + ng-mocks incompatible. Angular 21 optimizado para "Zoneless". Vitest:
- ✅ Native ESM (3-5x más rápido)
- ✅ Angular 21 standalone components
- ✅ Angular oficial recomienda para v18+
- ✅ Ya en package.json (v4.0.8)

**Config requerida:**
```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';
import angular from '@angular/build/vite';

export default defineConfig({
  plugins: [angular()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test.ts']
  }
});

// src/test.ts
import 'zone.js';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';

getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
```

**Diferencias Jest vs Vitest:**
- DisplaySlotPipe: `new DisplaySlotPipe()` directamente (pipe pura)
- Components: TestBed normal, pero con `vi.fn()` en vez de `jest.fn()`
- Async: `fakeAsync/tick` igual en ambos
- Speed: Vitest 3-5x más rápido

---

## 📋 RESUMEN FINAL SPRINT 2

**✅ Backend COMPLETADO (3 Enero 2026):**
- Entidades, migraciones, servicios, controllers
- 41 unit tests + 15 integration tests + E2E tests
- Base de datos 100% operacional

**⏳ Frontend EN PROGRESO:**
- Fase 7-8: ProfileService + Models (50 min)
- Fase 9-11: Dashboard + ProfileEditor + AvailabilityConfigurator (3h 30min)
- Fase 12: Component Tests Vitest (1h 30min)
- Fase 13: E2E Playwright (1h 30min) 🎖️

**Total remaining:** ~7h 50min

---


