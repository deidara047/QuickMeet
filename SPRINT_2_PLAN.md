# Sprint 2: Gestión de Perfil y Disponibilidad

**Objetivo:** Implementar configuración de perfil público y sistema de disponibilidad con generación automática de slots

**Duración Total:** 10-11 horas (6.5h Backend ✅ + 7.5-8.5h Frontend ⏳)

---

## 🎯 ESTADO ACTUAL (4 Enero 2026)

### ✅ BACKEND: COMPLETADO (Fases 1-6)
- **Fase 1-3:** Entidades, servicios, controllers
- **Fase 4-5:** Unit tests + Integration tests (41 tests pasando)
- **Fase 6:** E2E Backend (Broche de Oro) - COMPLETADO 3 Enero

**Resultado:** Backend 100% operacional, DB con tablas, servicios listos

### ⏳ FRONTEND: PENDIENTE (Fases 7-13)
- **Fases 7-8:** Setup & Models
- **Fases 9-11:** Componentes
- **Fases 12-13:** Testing (Unit + E2E)

**Próximo Paso:** Comenzar Fase 7 (ProfileService)

---

## � BACKEND: FASES 1-6 (REFERENCIA - COMPLETADO)

Todas las fases backend (entidades, migraciones, servicios, controllers, unit tests, integration tests, E2E) completadas el 3 Enero 2026.


---

## 🚨 PARADA DE EMERGENCIA: Backend Testing (ProvidersController + ProviderService)

**Problema identificado:** Fase 7 creó ProvidersController + ProviderService sin tests asociados.

**Acción correctiva (ANTES de Fase 8):**
- [X] Unit Tests: ProviderService (métodos de negocio)
- [X] Integration Tests: ProvidersController (3 endpoints)
- [ ] E2E Tests: Flujos completos vía HTTP
- [ ] Actualizar E2E Backend tests existentes para incluir provider endpoints

**Estimación:** 2 horas (1h unit + 0.5h integration + 0.5h E2E)

**Regla aplicada:** "Nunca se va a dar un sprint como terminado si no hay tests que lo avalen"

---

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

### FASE 12: Component Tests (Vitest) [1h 30min] ⏳

**Configuración inicial Vitest:**
- [ ] Crear `vitest.config.ts` en raíz del proyecto
- [ ] Crear `src/test.ts` con setup de TestBed
- [ ] Actualizar `package.json` scripts (ya está hecho: test, test:run, test:coverage)

**Estrategia de Testing:**
- ✅ DisplaySlotPipe: Pipe pura, SIN TestBed (instantiation directa)
- ProfileEditorComponent: Con TestBed, mocking ProfileService
- AvailabilityConfiguratorComponent: Con TestBed, mocking AvailabilityService

---

#### ProfileEditorComponent Tests [15 tests]

**Tareas:**
- [ ] Test 1: debería renderizar formulario con todos los campos
- [ ] Test 2: debería requerir campo fullName
- [ ] Test 3: debería validar minLength(3) en fullName
- [ ] Test 4: debería validar maxLength(100) en fullName
- [ ] Test 5: debería aceptar description opcional (max 500 chars)
- [ ] Test 6: debería validar teléfono con patrón regex
- [ ] Test 7: debería deshabilitar botón si form inválido
- [ ] Test 8: debería habilitar botón si form válido
- [ ] Test 9: debería mostrar preview de imagen en file upload
- [ ] Test 10: debería validar max 5MB en file upload
- [ ] Test 11: debería llamar profileService.updateProfile() on submit
- [ ] Test 12: debería mostrar loading spinner durante submit
- [ ] Test 13: debería mostrar toast success en actualización exitosa
- [ ] Test 14: debería mostrar toast error en fallo de API
- [ ] Test 15: debería preservar form data si hay error (para retry)

**Coverage Goal:** 80%+

---

#### AvailabilityConfiguratorComponent Tests [25 tests]

**Tareas:**

**Sección 1: Day Toggles & Time Inputs (8 tests)**
- [ ] Test 1: debería renderizar 7 toggles (Lun-Dom)
- [ ] Test 2: debería deshabilitar time inputs cuando toggle OFF
- [ ] Test 3: debería habilitar time inputs cuando toggle ON
- [ ] Test 4: debería validar que startTime < endTime
- [ ] Test 5: debería mostrar error si startTime > endTime
- [ ] Test 6: debería requerir al menos 1 día activo
- [ ] Test 7: debería desabilitar submit si no hay días
- [ ] Test 8: debería permitir submit con múltiples días configurados

**Sección 2: Breaks (6 tests)**
- [ ] Test 9: debería agregar nuevo break al click "+ Agregar"
- [ ] Test 10: debería eliminar break al click "Eliminar"
- [ ] Test 11: debería validar break dentro de horario working
- [ ] Test 12: debería mostrar error si break fuera de horario
- [ ] Test 13: debería validar sin traslape entre breaks
- [ ] Test 14: debería permitir múltiples breaks

**Sección 3: Duration & Buffer (3 tests)**
- [ ] Test 15: debería tener select duration con opciones [15,30,45,60]
- [ ] Test 16: debería tener select buffer con opciones [0,5,10,15]
- [ ] Test 17: debería usar valores default (30min, 0min)

**Sección 4: Preview Generation (5 tests)**
- [ ] Test 18: debería generar preview on form valueChanges
- [ ] Test 19: debería usar DisplaySlotPipe para formatear slots
- [ ] Test 20: debería mostrar slots para próximos 3 días
- [ ] Test 21: debería ocultar slots durante breaks
- [ ] Test 22: debería actualizar preview cuando form cambia

**Submit & Loading (3 tests)**
- [ ] Test 23: debería llamar availabilityService.configure() on submit
- [ ] Test 24: debería deshabilitar form durante submit
- [ ] Test 25: debería mostrar toast success/error según resultado

**Coverage Goal:** 85%+

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
BACKEND (Completado ✅)
├─ Fases 1-6: ✅ COMPLETADO (3 Enero 2026)
│  └─ 41 tests pasando + E2E Backend
│
FRONTEND (Ahora ⏳)
├─ Fases 7-8: [50 min] ProfileService + Models
├─ Fases 9-11: [3h 30min] Dashboard + ProfileEditor + AvailabilityConfigurator
├─ Fase 12: [1h 30min] Component Tests Vitest (40+ tests)
└─ Fase 13: [1h 30min] E2E Playwright (20+ tests) 🎖️

TOTAL: 7h 50min (aprox 8h)
```

---

## ✅ DEFINICIÓN DE COMPLETITUD SPRINT 2

**BACKEND:** ✅ COMPLETADO 3 Enero
- [x] Entidades, migraciones, servicios, controllers
- [x] 41 tests unitarios pasando
- [x] 15 tests de integración pasando
- [x] E2E Backend tests 100% pasando

**FRONTEND:** ⏳ EN PROGRESO
- [ ] Fase 7-8: Setup & Models
- [ ] Fase 9-11: Componentes (Dashboard, ProfileEditor, AvailabilityConfigurator)
- [ ] Fase 12: Component Tests Vitest (40+ tests, 80%+ coverage)
- [ ] Fase 13: E2E Playwright (20+ tests, todos pasando)

**SPRINT COMPLETADO CUANDO:**
- ✅ Backend: 100% (tests + E2E)
- ✅ Frontend: 100% (componentes + tests)
- ✅ Sin errores de compilación
- ✅ Manual verification en http://localhost:4200/dashboard
- ✅ Todos los tests pasando (unit + E2E)

---

## 🚀 NEXT STEP

**Esperar visto bueno para empezar Fase 12: Component Tests (Vitest Setup + Tests)**




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


