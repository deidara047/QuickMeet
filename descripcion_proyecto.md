# QuickMeet

## Sistema de Agendamiento de Citas sin Fricción

### Enunciado del Proyecto

#### Especificación Técnica Completa

#### v1.0

Stack Tecnológico:  

Backend: .NET 8 Web API · SQL Server  
Frontend: Angular 21 · TypeScript · TailwindCSS  
Testing: xUnit · Moq · Playwright  

December 28, 2025

## Descripción General del Proyecto

### Contexto y Problema

Actualmente, el proceso de agendamiento de citas entre profesionales y clientes presenta múltiples fricciones que generan pérdida de tiempo y frustración para ambas partes:

- Intercambio extenso de mensajes por WhatsApp, email o llamadas telefónicas para coordinar horarios
- Falta de visibilidad en tiempo real de la disponibilidad del profesional
- Errores humanos al registrar manualmente fechas y horarios
- Ausencia de recordatorios automáticos que resulta en citas olvidadas (no-shows)
- Herramientas existentes (Calendly, Acuity Scheduling) requieren registro obligatorio, tienen curvas de aprendizaje pronunciadas y costos asociados ($8-15/mes)
- Profesionales independientes no tienen acceso a soluciones empresariales costosas

### Solución Propuesta

**QuickMeet** es un sistema de agendamiento de citas que elimina completamente la fricción del proceso mediante un enfoque minimalista, gratuito y sin registro obligatorio para clientes.

**Propuesta de valor única:**

1. El profesional se registra una única vez y obtiene un enlace personalizado único: `quickmeet.app/nombre-profesional`
2. El cliente accede al enlace, visualiza disponibilidad en tiempo real y selecciona un horario en menos de 30 segundos
3. Ambas partes reciben confirmación automática por email con detalles completos
4. Sistema envía recordatorios automáticos 24 horas antes de la cita
5. Cliente puede cancelar mediante enlace único sin necesidad de contactar al profesional
6. Todo el proceso es instantáneo, sin instalaciones, sin cuentas para el cliente

### Objetivos del Proyecto

#### Objetivos Técnicos

1. Implementar API RESTful robusta con .NET 8 Web API aplicando principios de Clean Architecture
2. Diseñar y optimizar base de datos SQL Server con stored procedures, índices y transacciones
3. Desarrollar suite completa de testing:
   - Unit tests con xUnit + Moq (mínimo 80% code coverage)
   - Integration tests para endpoints críticos
   - E2E tests con Playwright para flujos principales
4. Implementar manejo robusto de concurrencia para prevenir double-booking mediante transacciones SQL con niveles de aislamiento apropiados
5. Lograr tiempo de respuesta de API menor a 100ms en operaciones de consulta
6. Implementar sistema de CI/CD con GitHub Actions para deploy automatizado
7. Aplicar principios SOLID y patrones de diseño (Repository, Unit of Work, Dependency Injection)

#### Objetivos de Negocio

1. Reducir tiempo de agendamiento de 10+ minutos (promedio actual) a menos de 30 segundos
2. Eliminar 100% de errores por double-booking mediante validaciones de concurrencia
3. Reducir tasa de no-shows en 50% mediante recordatorios automáticos
4. Proporcionar herramienta gratuita y profesional para independientes y pequeños negocios
5. Crear experiencia comparable a servicios premium pero accesible para todos

## Alcance del Proyecto

### Funcionalidades Incluidas (MVP)

1. **Módulo de Autenticación y Registro**
   - Registro de profesionales con validación de email
   - Login/logout con JWT tokens
   - Recuperación de contraseña
   - Validación de username único

2. **Módulo de Gestión de Perfil**
   - Configuración de perfil público (nombre, descripción, foto)
   - Generación de enlace personalizado único
   - Definición de servicios ofrecidos
   - Configuración de duración de citas (15, 30, 45, 60 minutos)

3. **Módulo de Disponibilidad**
   - Configuración de horario semanal (días laborables y horarios)
   - Definición de breaks/almuerzos
   - Establecimiento de buffer entre citas (0, 5, 10, 15 minutos)
   - Bloqueo de fechas específicas (vacaciones, días no laborables)
   - Generación automática de slots de tiempo disponibles

4. **Módulo de Agendamiento (CRÍTICO)**
   - Visualización pública de disponibilidad sin autenticación
   - Selección de fecha y horario en tiempo real
   - Formulario de información del cliente (nombre, email, teléfono, notas)
   - Validación de disponibilidad antes de confirmar
   - Prevención de double-booking con transacciones SQL
   - Confirmación instantánea con emails automáticos

5. **Módulo de Dashboard del Profesional**
   - Vista de citas del día
   - Calendario mensual con citas marcadas
   - Lista de próximas citas
   - Filtrado por fecha y estado
   - Visualización de información del cliente por cita
   - Estadísticas básicas (total citas por semana/mes)

6. **Módulo de Gestión de Citas**
   - Cancelación de cita por parte del cliente (mediante enlace único)
   - Cancelación de cita por parte del profesional
   - Marcado de cita como completada
   - Historial de citas (pasadas y canceladas)

7. **Módulo de Notificaciones**
   - Email de confirmación al agendar (cliente y profesional)
   - Email de recordatorio 24 horas antes (automático)
   - Email de confirmación al cancelar
   - Templates de email profesionales y responsive

### Funcionalidades Excluidas (Futuras Versiones)

- Integración con Google Calendar / Outlook
- Pasarela de pagos
- Videollamadas integradas
- App móvil nativa
- Sistema de reseñas/calificaciones
- Múltiples tipos de citas por profesional
- Reprogramación de citas
- Chat en tiempo real

## Roles y Responsabilidades del Sistema

### Rol 1: Profesional (Service Provider)

**Definición:** Usuario registrado que ofrece servicios por cita y gestiona su agenda.

**Ejemplos de profesionales objetivo:**

- Sector salud: Médicos, psicólogos, nutricionistas, terapeutas
- Servicios personales: Barberos, estilistas, entrenadores personales
- Consultoría: Abogados, contadores, consultores de negocios
- Educación: Tutores privados, profesores de idiomas, coaches

**Capacidades y responsabilidades:**

1. **Gestión de Cuenta**
   - Crear cuenta con email y contraseña
   - Verificar email mediante enlace
   - Iniciar y cerrar sesión
   - Recuperar contraseña olvidada
   - Actualizar información de perfil

2. **Configuración de Perfil Público**
   - Establecer nombre público y username único
   - Agregar foto de perfil profesional
   - Escribir descripción de servicios (hasta 500 caracteres)
   - Definir duración estándar de citas
   - Obtener enlace público personalizado

3. **Gestión de Disponibilidad**
   - Configurar horario semanal recurrente (días y horas de trabajo)
   - Definir horarios de break o almuerzo
   - Establecer tiempo de buffer entre citas consecutivas
   - Bloquear fechas específicas (vacaciones, eventos, días personales)
   - Modificar disponibilidad en cualquier momento

4. **Administración de Citas**
   - Visualizar todas las citas agendadas en dashboard
   - Ver detalles completos de cada cita (cliente, fecha, hora, notas)
   - Filtrar citas por estado (confirmadas, canceladas, completadas)
   - Marcar citas como completadas después de realizarlas
   - Cancelar citas cuando sea necesario (con notificación al cliente)
   - Acceder a historial completo de citas pasadas

5. **Recepción de Notificaciones**
   - Recibir email cuando un cliente agenda nueva cita
   - Recibir notificación cuando un cliente cancela
   - Obtener resumen diario de citas del día (opcional)

6. **Analytics y Reportes**
   - Visualizar estadísticas de la semana actual
   - Ver total de citas del mes
   - Calcular tasa de ocupación semanal
   - Identificar horas/días con mayor demanda

**Acceso:** El profesional DEBE estar autenticado para acceder a todas estas funcionalidades. Requiere registro completo con verificación de email.

### Rol 2: Cliente (Customer/End User)

**Definición:** Persona que desea agendar una cita con un profesional específico. NO requiere cuenta en el sistema.

**Capacidades y responsabilidades:**

1. **Acceso a Información**
   - Acceder a la página pública del profesional mediante enlace compartido
   - Visualizar información del profesional (foto, nombre, descripción, servicios)
   - Ver duración típica de las citas
   - Leer información sobre qué esperar de la cita

2. **Proceso de Agendamiento**
   - Hacer clic en botón "Agendar Cita"
   - Seleccionar fecha deseada en calendario interactivo
   - Visualizar horarios disponibles en tiempo real para la fecha seleccionada
   - Elegir horario específico de inicio de cita
   - Completar formulario con información personal:
     - Nombre completo (obligatorio)
     - Email de contacto (obligatorio)
     - Teléfono (opcional)
     - Notas o motivo de la cita (opcional, máx 500 caracteres)
   - Revisar resumen de la cita antes de confirmar
   - Confirmar agendamiento con un clic

3. **Recepción de Confirmaciones**
   - Recibir email de confirmación inmediato con:
     - Detalles completos de la cita (fecha, hora, duración)
     - Información de contacto del profesional
     - Dirección o instrucciones (si aplica)
     - Enlace único para cancelar la cita
     - Botón "Agregar a Google Calendar"
   - Recibir email de recordatorio 24 horas antes de la cita

4. **Gestión de Cita Agendada**
   - Cancelar cita mediante enlace único en email de confirmación
   - Ver confirmación de cancelación
   - Recibir email confirmando la cancelación

**Acceso:** El cliente NO necesita crear cuenta ni autenticarse. Todo el flujo es completamente anónimo y basado en enlaces únicos para cada acción (cancelación).

**Restricciones del cliente:**

- No puede ver citas de otros clientes (privacidad)
- No puede modificar una cita ya agendada (solo cancelar)
- No puede agendar más de 3 citas futuras con el mismo profesional (prevención de spam)
- No puede agendar con menos de 2 horas de anticipación
- No puede agendar más allá de 60 días en el futuro

## Especificación de Casos de Uso

### CU-01: Registro de Profesional

**Actor Principal:** Profesional

**Precondiciones:** Ninguna

**Trigger:** Profesional accede a `quickmeet.app/register`

**Flujo Normal (Camino Feliz):**

1. Sistema muestra formulario de registro con campos:
   - Nombre completo
   - Email
   - Username (será parte del enlace público)
   - Contraseña
   - Confirmación de contraseña
2. Profesional completa todos los campos
3. Profesional hace clic en "Crear Cuenta"
4. Sistema valida formato de email (regex)
5. Sistema verifica que email no exista previamente en base de datos
6. Sistema verifica que username no esté en uso y cumpla requisitos (3-30 caracteres, solo letras, números, guiones)
7. Sistema valida fortaleza de contraseña (mínimo 8 caracteres, 1 mayúscula, 1 número, 1 carácter especial)
8. Sistema valida que contraseña y confirmación coincidan
9. Sistema hashea contraseña con BCrypt (cost factor 12)
10. Sistema crea registro en tabla `Providers` con estado `PendingVerification`
11. Sistema genera token de verificación único (GUID) y lo almacena con expiración de 24 horas
12. Sistema envía email de verificación con enlace: `quickmeet.app/verify?token={guid}`
13. Sistema muestra mensaje: "Cuenta creada. Por favor verifica tu email para continuar."
14. Profesional accede a su bandeja de email
15. Profesional hace clic en enlace de verificación
16. Sistema valida token de verificación
17. Sistema actualiza estado de cuenta a `Active`
18. Sistema marca token como usado
19. Sistema redirige a login con mensaje: "Email verificado exitosamente. Ya puedes iniciar sesión."

**Flujos Alternativos:**

- **FA-01: Email ya registrado**
  - Sistema detecta email existente en paso 5
  - Sistema muestra error: "Este email ya está registrado. ¿Olvidaste tu contraseña?"
  - Sistema ofrece enlace a recuperación de contraseña
  - Flujo termina

- **FA-02: Username no disponible**
  - Sistema detecta username en uso en paso 6
  - Sistema muestra error: "Este nombre de usuario ya está en uso"
  - Sistema sugiere 3 alternativas disponibles basadas en el username ingresado
  - Profesional debe elegir otro username
  - Flujo regresa a paso 3

- **FA-03: Contraseña débil**
  - Sistema detecta contraseña débil en paso 7
  - Sistema muestra requisitos específicos no cumplidos
  - Profesional debe ingresar nueva contraseña
  - Flujo regresa a paso 3

- **FA-04: Token de verificación expirado**
  - Profesional intenta verificar después de 24 horas
  - Sistema detecta token expirado en paso 16
  - Sistema muestra mensaje: "Este enlace ha expirado"
  - Sistema ofrece reenviar email de verificación
  - Si profesional acepta, sistema genera nuevo token y reenvía email
  - Flujo regresa a paso 14

**Postcondiciones:**

- Registro de profesional creado en base de datos con estado `Active`
- Email verificado y marcado como válido
- Enlace público generado: `quickmeet.app/[username]`
- Profesional puede iniciar sesión

**Reglas de Negocio:**

- RN-01: Username debe ser único en todo el sistema
- RN-02: Email debe ser único en todo el sistema
- RN-03: Token de verificación expira en 24 horas
- RN-04: Contraseña debe cumplir política de seguridad mínima
- RN-05: Username no puede contener palabras ofensivas (lista negra)

### CU-02: Configuración de Disponibilidad Semanal

**Actor Principal:** Profesional

**Precondiciones:**

- Profesional debe estar autenticado
- Cuenta debe estar en estado `Active`

**Trigger:** Profesional accede a "Mi Disponibilidad" en su dashboard

**Flujo Normal:**

1. Sistema muestra interfaz de configuración semanal con:
   - Calendario semanal (Lunes a Domingo)
   - Toggle para activar/desactivar cada día
   - Campos de hora de inicio y fin para cada día
   - Opción de agregar breaks
   - Campo de duración de cita (dropdown: 15, 30, 45, 60, 90 minutos)
   - Campo de buffer entre citas (dropdown: 0, 5, 10, 15 minutos)
2. Profesional selecciona días laborables activando toggles
3. Para cada día activo, profesional ingresa:
   - Hora de inicio (ej: 09:00)
   - Hora de fin (ej: 18:00)
4. Profesional opcionalmente agrega break:
   - Hace clic en "Agregar break"
   - Ingresa hora de inicio de break (ej: 13:00)
   - Ingresa hora de fin de break (ej: 14:00)
5. Profesional selecciona duración estándar de citas (ej: 30 minutos)
6. Profesional selecciona buffer entre citas (ej: 10 minutos)
7. Profesional hace clic en "Guardar Disponibilidad"
8. Sistema valida que:
   - Al menos un día esté seleccionado
   - Hora de fin sea posterior a hora de inicio para cada día
   - Breaks no se traslapen con límites del día
   - Breaks estén dentro del horario laboral
   - Break inicio sea anterior a break fin
9. Sistema calcula todos los slots de tiempo disponibles para los próximos 60 días basándose en:
   - Días laborables configurados
   - Horarios de inicio y fin
   - Breaks definidos
   - Duración de cita
   - Buffer entre citas
10. Sistema inserta slots en tabla `TimeSlots` con estado `Available`
11. Sistema almacena configuración en tabla `ProviderAvailability`
12. Sistema muestra mensaje de éxito: "Disponibilidad configurada correctamente"
13. Sistema muestra vista previa del calendario generado con ejemplo de slots

**Ejemplo de Cálculo de Slots:**

Configuración:

- Día: Lunes
- Horario: 09:00 - 18:00
- Break: 13:00 - 14:00
- Duración de cita: 30 minutos
- Buffer: 10 minutos

Slots generados:

- 09:00 - 09:30
- 09:40 - 10:10
- 10:20 - 10:50
- 11:00 - 11:30
- 11:40 - 12:10
- 12:20 - 12:50
- [BREAK 13:00 - 14:00]
- 14:00 - 14:30
- 14:40 - 15:10
- ... hasta 17:30 - 18:00

**Flujos Alternativos:**

- **FA-01: Sin días seleccionados**
  - Sistema detecta problema en paso 8
  - Sistema muestra error: "Debes seleccionar al menos un día laboral"
  - Flujo regresa a paso 2

- **FA-02: Horarios inválidos**
  - Sistema detecta hora fin antes de hora inicio en paso 8
  - Sistema muestra error específico para el día problemático
  - Flujo regresa a paso 3

- **FA-03: Break traslapado**
  - Sistema detecta break fuera de horario laboral en paso 8
  - Sistema muestra error: "El break debe estar dentro del horario laboral"
  - Flujo regresa a paso 4

**Postcondiciones:**

- Disponibilidad semanal almacenada en base de datos
- Slots de tiempo generados para próximos 60 días
- Enlace público del profesional operativo para recibir reservas
- Sistema puede mostrar disponibilidad en tiempo real a clientes

**Reglas de Negocio:**

- RN-01: Mínimo un día laboral debe estar configurado
- RN-02: Slots se generan automáticamente para 60 días adelante
- RN-03: Al modificar disponibilidad, solo se afectan slots futuros (no citas ya agendadas)
- RN-04: Buffer es opcional (puede ser 0 minutos)
- RN-05: Un día puede tener máximo 2 breaks

### CU-03: Cliente Agenda Cita (CRÍTICO - FLUJO PRINCIPAL)

**Actor Principal:** Cliente

**Precondiciones:**

- Cliente tiene el enlace público del profesional
- Profesional tiene disponibilidad configurada
- Existen slots disponibles en el futuro

**Trigger:** Cliente accede a `quickmeet.app/[username-profesional]`

**Flujo Normal (Descripción tipo Diagrama de Secuencia):**

**FASE 1: Visualización de Perfil Público**

1. **Cliente → Frontend:** Accede a URL del profesional en navegador
2. **Frontend → API:** GET `/api/providers/{username}`
3. **API → AppointmentService:** `GetProviderPublicProfile(username)`
4. **AppointmentService → Repository:** `GetProviderByUsername(username)`
5. **Repository → SQL Server:**
   ```
   SELECT ProviderId, DisplayName, Username, Description,
          PhotoUrl, AppointmentDuration
   FROM Providers
   WHERE Username = @Username AND Status = 'Active'
   ```
6. **SQL Server → Repository:** Retorna datos del profesional
7. **Repository → AppointmentService:** Retorna entidad `Provider`
8. **AppointmentService → API:** Retorna DTO `ProviderPublicProfileDto`
9. **API → Frontend:** HTTP 200 OK + JSON con datos del profesional
10. **Frontend → Cliente:** Renderiza página pública mostrando:
    - Foto del profesional
    - Nombre y descripción
    - Duración de citas
    - Botón "Agendar Cita"

**FASE 2: Selección de Fecha**

11. **Cliente → Frontend:** Hace clic en botón "Agendar Cita"
12. **Frontend → Cliente:** Muestra modal/página con calendario interactivo (próximos 60 días)
13. **Cliente → Frontend:** Selecciona fecha específica (ej: 15 de Enero, 2025)
14. **Frontend → API:** GET `/api/appointments/available-slots?providerId={id}&date=2025-01-15`
15. **API → AppointmentService:** `GetAvailableSlots(providerId, date)`
16. **AppointmentService → Repository:** `GetAvailableSlotsForDate(providerId, date)`
17. **Repository → SQL Server:** Ejecuta stored procedure
    ```
    EXEC sp_GetAvailableSlots
        @ProviderId = 123,
        @Date = '2025-01-15'
    ```
18. **SQL Server:** Stored procedure ejecuta lógica:
    ```
    SELECT SlotId, StartTime, EndTime
    FROM TimeSlots
    WHERE ProviderId = @ProviderId
        AND CAST(StartTime AS DATE) = @Date
        AND Status = 'Available'
        AND StartTime >= DATEADD(HOUR, 2, GETUTCDATE())
    ORDER BY StartTime
    ```
19. **SQL Server → Repository:** Retorna lista de slots disponibles
20. **Repository → AppointmentService:** Retorna lista de entidades `TimeSlot`
21. **AppointmentService → API:** Retorna lista de DTOs `TimeSlotDto`
22. **API → Frontend:** HTTP 200 OK + JSON array de horarios
    ```json
    [
        { "slotId": 1, "startTime": "09:00", "endTime": "09:30" },
        { "slotId": 2, "startTime": "09:40", "endTime": "10:10" },
        { "slotId": 3, "startTime": "10:20", "endTime": "10:50" }
    ]
    ```
23. **Frontend → Cliente:** Muestra lista de horarios disponibles en formato visual

**FASE 3: Selección de Horario y Completar Información**

24. **Cliente → Frontend:** Selecciona horario específico (ej: 09:00 - 09:30)
25. **Frontend → Cliente:** Muestra formulario de información del cliente con campos:
    - Nombre completo (input text, requerido)
    - Email (input email, requerido)
    - Teléfono (input tel, opcional)
    - Notas adicionales (textarea, opcional, max 500 caracteres)
26. **Cliente → Frontend:** Completa formulario con sus datos
27. **Cliente → Frontend:** Hace clic en "Confirmar Cita"

**FASE 4: Validación y Creación de Cita (TRANSACCIÓN CRÍTICA)**

28. **Frontend → API:** POST `/api/appointments`
    ```json
    {
        "slotId": 1,
        "providerId": 123,
        "clientName": "Juan Pérez",
        "clientEmail": "juan@email.com",
        "clientPhone": "+502 5555-1234",
        "notes": "Primera consulta"
    }
    ```
29. **API → Controller:** Valida DTO con Data Annotations
30. **Controller → AppointmentService:** `BookAppointment(bookingDto)`
31. **AppointmentService:** Inicia lógica de negocio
32. **AppointmentService → Repository:** `BeginTransaction()`
33. **Repository → SQL Server:**
    ```
    BEGIN TRANSACTION
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
    ```
34. **AppointmentService → Repository:** `ValidateSlotAvailability(slotId)`
35. **Repository → SQL Server:** Query con row locking
    ```
    SELECT SlotId, Status, StartTime
    FROM TimeSlots WITH (UPDLOCK, ROWLOCK)
    WHERE SlotId = @SlotId
    ```
36. **SQL Server → Repository:** Retorna información del slot
37. **Repository → AppointmentService:** Confirma que slot está `Available`
38. **AppointmentService:** Valida que slot no esté en el pasado
39. **AppointmentService:** Valida que cliente no tenga más de 3 citas futuras con este profesional
40. **AppointmentService → Repository:** `CreateAppointment(appointmentEntity)`
41. **Repository → SQL Server:**
    ```
    INSERT INTO Appointments
        (AppointmentId, SlotId, ProviderId, ClientName,
         ClientEmail, ClientPhone, Notes, Status,
         CreatedAt, CancellationToken)
    VALUES
        (NEWID(), @SlotId, @ProviderId, @ClientName,
         @ClientEmail, @ClientPhone, @Notes, 'Confirmed',
         GETUTCDATE(), NEWID())
    ```
42. **Repository → SQL Server:** Actualiza estado del slot
    ```
    UPDATE TimeSlots
    SET Status = 'Reserved',
        ReservedAt = GETUTCDATE()
    WHERE SlotId = @SlotId
    ```
43. **Repository → SQL Server:**
    ```
    COMMIT TRANSACTION
    ```
44. **Repository → AppointmentService:** Retorna entidad `Appointment` con ID generado

**FASE 5: Notificaciones y Confirmación**

45. **AppointmentService → NotificationService:** `SendClientConfirmation(appointment)`
46. **NotificationService → Email Provider:** Envía email al cliente con:
    - Detalles completos de la cita
    - Información del profesional
    - Enlace único de cancelación: `quickmeet.app/cancel?token={guid}`
    - Botón "Agregar a Google Calendar"
47. **AppointmentService → NotificationService:** `SendProviderNotification(appointment)`
48. **NotificationService → Email Provider:** Envía email al profesional con:
    - Notificación de nueva cita
    - Información del cliente
    - Enlace a dashboard
49. **AppointmentService → SchedulerService:** `ScheduleReminder(appointment, 24 hours before)`
50. **SchedulerService → Database:** Inserta job en tabla `ScheduledReminders`
51. **AppointmentService → API:** Retorna `AppointmentConfirmationDto`
52. **API → Frontend:** HTTP 201 Created + JSON
    ```json
    {
        "appointmentId": "abc-123",
        "status": "Confirmed",
        "startTime": "2025-01-15T09:00:00Z",
        "endTime": "2025-01-15T09:30:00Z",
        "message": "Cita agendada exitosamente"
    }
    ```
53. **Frontend → Cliente:** Muestra página de confirmación con:
    - Mensaje de éxito
    - Resumen de la cita
    - Instrucción de revisar email

**Flujos Alternativos:**

- **FA-01: Slot ya no disponible (Race Condition)**
  - En paso 36, sistema detecta que slot tiene estado `Reserved` o `Blocked`
  - Sistema ejecuta ROLLBACK de transacción
  - Sistema retorna error específico
  - API responde con HTTP 409 Conflict
  - Frontend muestra mensaje amigable: "Este horario acaba de ser reservado por otro cliente. Por favor selecciona otro horario."
  - Frontend refresca lista de slots disponibles automáticamente
  - Flujo regresa a FASE 2, paso 13

- **FA-02: Cliente excede límite de citas**
  - En paso 39, sistema detecta que cliente tiene 3+ citas futuras
  - Sistema ejecuta ROLLBACK
  - API responde con HTTP 400 Bad Request
  - Frontend muestra: "Has alcanzado el límite de 3 citas pendientes con este profesional. Por favor completa o cancela una cita antes de agendar otra."
  - Flujo termina

- **FA-03: Horario en el pasado**
  - En paso 38, sistema detecta que `StartTime < CurrentTime + 2 hours`
  - Sistema ejecuta ROLLBACK
  - API responde con HTTP 400 Bad Request
  - Frontend muestra: "Este horario ya no está disponible. Por favor selecciona otro."
  - Flujo regresa a FASE 2, paso 13

- **FA-04: Error al enviar emails**
  - En pasos 46 o 48, servicio de email falla
  - Sistema NO hace rollback de la cita (ya está creada)
  - Sistema registra la cita con flag `NotificationPending = true`
  - Sistema encola reintento de envío de email
  - API responde con HTTP 201 Created (cita creada exitosamente)
  - Frontend muestra confirmación normal
  - Job en background reintenta envío cada 5 minutos (máximo 3 intentos)

- **FA-05: Validación de email inválido**
  - En paso 29, Controller detecta formato de email inválido
  - API responde con HTTP 400 Bad Request + errores de validación
  - Frontend muestra errores debajo del campo email
  - Flujo regresa a paso 25

**Postcondiciones Exitosas:**

- Cita creada en base de datos con estado `Confirmed`
- Slot marcado como `Reserved` y ya no disponible para otros
- Token único de cancelación generado y asociado a la cita
- Emails de confirmación enviados a ambas partes
- Recordatorio programado para 24 horas antes de la cita
- Cliente y profesional tienen información completa de la cita

**Reglas de Negocio Críticas:**

- RN-01: Un slot solo puede ser reservado por un cliente (atomicidad garantizada por transacción)
- RN-02: No se permiten reservas en horarios pasados
- RN-03: No se permiten reservas con menos de 2 horas de anticipación
- RN-04: Máximo 60 días de anticipación para agendar
- RN-05: Un cliente puede tener máximo 3 citas futuras con el mismo profesional
- RN-06: Email es obligatorio para confirmación y recordatorios
- RN-07: Teléfono es opcional pero recomendado
- RN-08: Notas tienen límite de 500 caracteres

### CU-04: Cliente Cancela Cita

**Actor Principal:** Cliente

**Precondiciones:**

- Cliente tiene cita agendada (estado `Confirmed`)
- Cliente tiene acceso al email de confirmación con enlace de cancelación

**Trigger:** Cliente hace clic en "Cancelar Cita" desde email de confirmación

**Flujo Normal (Descripción tipo Diagrama de Secuencia):**

1. **Cliente → Navegador:** Hace clic en enlace de cancelación en email
2. **Navegador → Frontend:** GET `/cancel?token={cancellation-token-guid}`
3. **Frontend → API:** GET `/api/appointments/cancellation-details?token={token}`
4. **API → AppointmentService:** `GetAppointmentByToken(token)`
5. **AppointmentService → Repository:** `FindAppointmentByToken(token)`
6. **Repository → SQL Server:**
   ```
   SELECT a.AppointmentId, a.Status, a.ClientName,
          a.ClientEmail, ts.StartTime, ts.EndTime,
          p.DisplayName, p.Email as ProviderEmail
   FROM Appointments a
   JOIN TimeSlots ts ON a.SlotId = ts.SlotId
   JOIN Providers p ON a.ProviderId = p.ProviderId
   WHERE a.CancellationToken = @Token
   ```
7. **SQL Server → Repository:** Retorna datos de la cita
8. **Repository → AppointmentService:** Retorna entidad `Appointment`
9. **AppointmentService:** Valida que cita esté en estado `Confirmed`
10. **AppointmentService → API:** Retorna `AppointmentDetailsDto`
11. **API → Frontend:** HTTP 200 OK + JSON con detalles
12. **Frontend → Cliente:** Muestra página de cancelación con:
    - Detalles de la cita a cancelar
    - Advertencia: "Esta acción no se puede deshacer"
    - Botón "Confirmar Cancelación"
    - Botón "Volver" (sin cancelar)
13. **Cliente → Frontend:** Lee detalles y hace clic en "Confirmar Cancelación"
14. **Frontend → API:** POST `/api/appointments/cancel`
    ```json
    {
        "cancellationToken": "abc-def-123-456"
    }
    ```
15. **API → AppointmentService:** `CancelAppointment(token)`
16. **AppointmentService → Repository:** `BeginTransaction()`
17. **Repository → SQL Server:**
    ```
    BEGIN TRANSACTION
    ```
18. **AppointmentService → Repository:** `GetAppointmentByToken(token)` con lock
19. **Repository → SQL Server:**
    ```
    SELECT AppointmentId, Status, SlotId
    FROM Appointments WITH (UPDLOCK, ROWLOCK)
    WHERE CancellationToken = @Token
    ```
20. **SQL Server → Repository:** Retorna datos de cita
21. **Repository → AppointmentService:** Confirma cita existe y está `Confirmed`
22. **AppointmentService → Repository:** `UpdateAppointmentStatus(appointmentId, 'Cancelled')`
23. **Repository → SQL Server:**
    ```
    UPDATE Appointments
    SET Status = 'Cancelled',
        CancelledAt = GETUTCDATE(),
        CancelledBy = 'Client'
    WHERE AppointmentId = @AppointmentId
    ```
24. **AppointmentService → Repository:** `ReleaseTimeSlot(slotId)`
25. **Repository → SQL Server:**
    ```
    UPDATE TimeSlots
    SET Status = 'Available',
        ReservedAt = NULL
    WHERE SlotId = @SlotId
    ```
26. **Repository → SQL Server:**
    ```
    COMMIT TRANSACTION
    ```
27. **AppointmentService → NotificationService:** `SendCancellationConfirmationToClient(appointment)`
28. **NotificationService → Email Provider:** Envía email al cliente confirmando cancelación
29. **AppointmentService → NotificationService:** `NotifyProviderOfCancellation(appointment)`
30. **NotificationService → Email Provider:** Envía email al profesional notificando cancelación
31. **AppointmentService → SchedulerService:** `CancelScheduledReminder(appointmentId)`
32. **SchedulerService → Database:** Elimina/marca como cancelado el reminder programado
33. **AppointmentService → API:** Retorna confirmación de cancelación
34. **API → Frontend:** HTTP 200 OK + mensaje de éxito
35. **Frontend → Cliente:** Muestra página de confirmación:
    - "Cita cancelada exitosamente"
    - "Recibirás un email de confirmación"
    - "El horario está nuevamente disponible para otros clientes"

**Flujos Alternativos:**

- **FA-01: Token inválido o expirado**
  - En paso 7, SQL no retorna ninguna cita
  - API responde con HTTP 404 Not Found
  - Frontend muestra: "Este enlace de cancelación no es válido o ha expirado. Por favor contacta directamente al profesional."
  - Flujo termina

- **FA-02: Cita ya cancelada**
  - En paso 9, sistema detecta estado `Cancelled`
  - API responde con HTTP 400 Bad Request
  - Frontend muestra: "Esta cita ya fue cancelada previamente."
  - Flujo termina

- **FA-03: Cita ya completada**
  - En paso 9, sistema detecta estado `Completed`
  - API responde con HTTP 400 Bad Request
  - Frontend muestra: "Esta cita ya fue realizada y no puede ser cancelada."
  - Flujo termina

- **FA-04: Cancelación muy cercana a la cita**
  - En paso 21, sistema detecta que `StartTime - CurrentTime < 2 hours`
  - Sistema permite cancelación pero agrega nota especial
  - Email al profesional incluye advertencia: "Cancelación de último momento"
  - Flujo continúa normalmente

**Postcondiciones:**

- Cita marcada como `Cancelled` en base de datos
- Slot liberado y disponible nuevamente (estado `Available`)
- Cliente y profesional notificados por email
- Recordatorio programado cancelado
- Slot puede ser reservado por otro cliente inmediatamente

**Reglas de Negocio:**

- RN-01: Cliente puede cancelar en cualquier momento antes de la cita
- RN-02: Cancelaciones con menos de 2 horas se marcan como "último momento"
- RN-03: Token de cancelación es de un solo uso (no puede cancelar dos veces)
- RN-04: Slot liberado está inmediatamente disponible para otros
- RN-05: No hay penalización por cancelar (en esta versión MVP)

## Stack Tecnológico Detallado

### Backend - .NET 8 Web API

**Framework Principal:**

- .NET 8.0 (LTS) - ASP.NET Core Web API
- C# 12.0

**Librerías y Paquetes NuGet:**

- **Entity Framework Core 8.0:** ORM para acceso a datos
- **Microsoft.EntityFrameworkCore.SqlServer:** Provider para SQL Server
- **Dapper:** Micro-ORM para stored procedures y queries optimizados
- **BCrypt.Net-Next:** Hashing de contraseñas
- **System.IdentityModel.Tokens.Jwt:** Autenticación JWT
- **FluentValidation:** Validación de DTOs y modelos
- **AutoMapper:** Mapeo entre entidades y DTOs
- **Serilog:** Logging estructurado
- **MailKit:** Envío de emails (SMTP)
- **Hangfire:** Job scheduling para recordatorios
- **Swashbuckle.AspNetCore:** Documentación Swagger/OpenAPI

**Patrones y Arquitectura:**

- Clean Architecture (3 capas: API, Business Logic, Data Access)
- Repository Pattern
- Unit of Work Pattern
- Dependency Injection (built-in .NET DI container)
- CQRS ligero (separación read/write para queries complejas)

### Base de Datos - SQL Server

**Motor:**

- SQL Server 2022 (o Azure SQL Database para producción)
- SQL Server Express para desarrollo local

**Características Utilizadas:**

- Stored Procedures para lógica compleja (ej: `sp_GetAvailableSlots`)
- Triggers para auditoría
- Índices clusterizados y no clusterizados
- Transacciones con niveles de aislamiento SERIALIZABLE
- Row-level locking para concurrencia
- Constraints (PK, FK, UNIQUE, CHECK)
- Computed columns

### Frontend - Angular 21 (Clean Architecture)

**Framework Principal:**

- Angular 21 LTS - Standalone components como standard (sin NgModules)
- TypeScript 5.5+ con strict mode habilitado
- RxJS 7.8+ para programación reactiva
- Signals para estado reactivo local y granular

**Arquitectura (Clean Architecture Principles):**

- **Core Module:** Services singleton, Guards, Interceptors (importados solo en root)
- **Shared Module:** Componentes, directivas, pipes reutilizables
- **Features Module:** Lazy-loaded, self-contained con sus propias rutas y lógica
- **Smart/Dumb Components:** Contenedores (smart) con lógica, Presentacionales (dumb) con inputs/outputs
- **Repository Pattern:** Abstracción de acceso a datos en servicios
- **Facade Pattern:** Servicios que encapsulan operaciones complejas
- **Reactive Forms:** Type-safe con validación fuerte y async validators
- **SOLID Principles:** Separación de responsabilidades, inyección de dependencias

**Librerías y Dependencias:**

- **TailwindCSS 3.4:** Utility-first CSS con PostCSS integration
- **Angular Material 21:** Componentes UI profesionales, accesibles, mantenidos oficialmente
  - Formularios, buttons, dialogs, calendarios, datepickers
  - Temas customizables y diseño responsive
- **Reactive Forms:** Validadores avanzados, async validators, custom validators
- **@angular/common/http:** HttpClient con interceptors para JWT, error handling, loading
- **RxJS 7.8+:** combineLatest, switchMap, catchError, shareReplay, debounceTime, distinctUntilChanged
- **TanStack Query:** State management de servidor con caching automático y sincronización
- **date-fns v3:** Manipulación de fechas inmutable, UTC ISO 8601 (sin timezone complexity)
- **@ngx-translate/core:** Internacionalización y soporte multilenguaje
- **@ngx-pwa/local-storage:** Storage reactivo con RxJS streams
- **ngx-toastr:** Notificaciones toast con animaciones smooth

**Características Modernas:**

- **Standalone Components First:** Todos los componentes standalone con imports explícitos
- **Standalone Routes:** Route definitions como objetos, no arrays
- **Signals:** Estado reactivo local con `signal()`, `computed()`, `effect()`
- **Typed Forms:** `FormGroup<{ key: FormControl<type> }>` con type-safety completo
- **Control Flow:** @if, @for, @switch (directivas modernas, no *ngIf, *ngFor)
- **Deferrable Views:** @defer para lazy loading de componentes
- **Change Detection OnPush:** Estrategia de detección optimizada por defecto
- **Strict Template Checking:** fullTemplateTypeCheck, strictTemplates, strictAttributeTypes
- **Output as Functions:** `@Output() = output()` en lugar de EventEmitter
- **Input as Signals:** `@Input({ required: true })` para inputs obligatorios
- **ESLint Angular:** Reglas modernas para detectar anti-patterns
- **Hydration:** Pre-rendering y SSR support para performance

### Testing Strategy (Backend + Frontend)

**Backend - Unit Testing (xUnit):**

- **xUnit 2.6:** Framework de testing con soporte para teorías, fixtures, y custom runners
- **Moq 4.20:** Mocking framework para aislar dependencias y comportamientos
- **FluentAssertions:** Assertions legibles, fluidas y expresivas
- **Coverlet:** Code coverage analysis con soporte para exclusiones granulares
- **AutoFixture:** Auto-generación de test data, fixtures y builders
- **Patrón AAA:** Arrange (setup) - Act (ejecución) - Assert (validación)
- **Objetivo:** 80%+ coverage en servicios, especialmente lógica de negocio crítica

**Backend - Integration Testing:**

- **Microsoft.AspNetCore.Mvc.Testing:** WebApplicationFactory para testing de controllers
- **TestContainers:** Containers Docker para SQL Server realista (no mocks)
- **Respawn:** Reset y limpieza de base de datos entre tests
- **Flujos:** Testing de endpoints completos con transacciones reales
- **Concurrencia:** Tests de double-booking prevention con requests paralelos

**Frontend - Unit Testing (Jasmine + Karma):**

- **Jasmine 5.x:** Framework BDD para JavaScript/TypeScript con soporte para specs, suites, hooks
- **Karma 6.x:** Test runner que ejecuta tests en navegadores reales (Chromium, Firefox, WebKit)
- **TestBed:** Setup y configuración de testing de componentes y servicios Angular
  - `TestBed.configureTestingModule()` para importar dependencias
  - `ComponentFixture` para interactuar con componentes
  - `DebugElement` para acceso al DOM
- **@testing-library/angular:** Queries semánticas focalizadas en UX (no detalles de implementación)
  - `getByRole()`, `getByLabelText()`, `getByPlaceholderText()`
  - Simula comportamiento real del usuario
- **jasmine-marbles:** Testing de RxJS observables, operators y timing
- **ng-mocks:** Mocking de componentes, directivas, servicios y pipes Angular
- **HttpTestingController:** Testing de HttpClient sin requests reales
- **Cobertura de tests:**
  - Services: 85%+ (lógica de negocio crítica)
  - Components: 70%+ (lógica de presentación)
  - Guards e Interceptors: 90%+ (seguridad crítica)

**Estructura de Tests en Frontend (Ejemplos):**

```typescript
// core/services/auth.service.spec.ts
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClientTesting()]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should register a provider and return JWT', (done) => {
    const mockResponse = { token: 'jwt-token', userId: '123' };
    
    service.register({ email: 'test@test.com', password: 'test123' })
      .subscribe(result => {
        expect(result.token).toBe('jwt-token');
        done();
      });

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });
});

// features/public/components/booking-form.component.spec.ts
describe('BookingFormComponent', () => {
  let component: BookingFormComponent;
  let fixture: ComponentFixture<BookingFormComponent>;
  let appointmentService: jasmine.SpyObj<AppointmentService>;

  beforeEach(async () => {
    const appointmentSpy = jasmine.createSpyObj('AppointmentService', 
      ['bookAppointment', 'getAvailableSlots']
    );

    await TestBed.configureTestingModule({
      imports: [BookingFormComponent],
      providers: [
        { provide: AppointmentService, useValue: appointmentSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingFormComponent);
    component = fixture.componentInstance;
    appointmentService = TestBed.inject(AppointmentService) as 
      jasmine.SpyObj<AppointmentService>;
  });

  it('should book appointment with valid form', () => {
    appointmentService.bookAppointment.and.returnValue(
      of({ appointmentId: '123' })
    );

    component.form.patchValue({
      clientName: 'Juan',
      clientEmail: 'juan@test.com',
      slotId: '456'
    });

    component.onSubmit();

    expect(appointmentService.bookAppointment)
      .toHaveBeenCalledWith(jasmine.objectContaining({
        clientName: 'Juan',
        clientEmail: 'juan@test.com'
      }));
  });
});

// core/guards/auth.guard.spec.ts
describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should allow access if user is authenticated', () => {
    authService.isAuthenticated.and.returnValue(true);

    const result = guard.canActivate(
      new ActivatedRouteSnapshot(),
      new RouterStateSnapshot('/', null)
    );

    expect(result).toBe(true);
  });

  it('should redirect to login if not authenticated', () => {
    authService.isAuthenticated.and.returnValue(false);

    guard.canActivate(
      new ActivatedRouteSnapshot(),
      new RouterStateSnapshot('/', null)
    );

    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
  });
});
```

**Frontend - E2E Testing (Playwright):**

- **@playwright/test:** Testing end-to-end moderno con soporte para parallelización
- **Pruebas en múltiples navegadores:** Chromium, Firefox, WebKit (ejecutadas en paralelo)
- **Artifacts automáticos:** Screenshots y videos capturados en fallos
- **Flujos críticos testados:**
  - CU-03: Cliente agenda cita (flujo completo)
  - CU-04: Cliente cancela cita
  - CU-01 + CU-02: Profesional se registra y configura disponibilidad
  - Double-booking prevention (test de concurrencia)
- **Page Object Model:** Abstracción de páginas para mantenibilidad
- **Parallelización:** Múltiples tests ejecutados simultáneamente

**Ejemplo E2E Test (Playwright):**

```typescript
// tests/e2e/booking-flow.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Booking Flow', () => {
  test('client completes full booking flow', async ({ page }) => {
    // Navigate to provider profile
    await page.goto('/provider/juan-perez');
    
    // Verify provider information displayed
    await expect(page.locator('h1')).toContainText('Dr. Juan Pérez');
    
    // Click book appointment
    await page.click('button:has-text("Agendar Cita")');

### DevOps y CI/CD

**Control de Versiones:**

- Git + GitHub
- GitFlow branching strategy

**CI/CD Pipeline - GitHub Actions:**

- Build y compilación automática
- Ejecución de tests (unit + integration)
- Code coverage reports
- Deploy automático a staging/producción

**Containerización:**

- Docker para backend API
- Docker Compose para desarrollo local (API + SQL Server + Frontend)

**Hosting/Deploy:**

- Backend: Azure App Service o Railway
- Base de Datos: Azure SQL Database
- Frontend: Vercel o Azure Static Web Apps (con Angular SSR support)

## Estructura de Carpetas del Proyecto

### Backend - .NET Solution

```
QuickMeet/
├── src/
│ ├── QuickMeet.API/ # Web API Project
│ │ ├── Controllers/
│ │ │ ├── AuthController.cs
│ │ │ ├── ProvidersController.cs
│ │ │ ├── AppointmentsController.cs
│ │ │ ├── AvailabilityController.cs
│ │ │ └── DashboardController.cs
│ │ ├── Middleware/
│ │ │ ├── ExceptionHandlingMiddleware.cs
│ │ │ ├── RateLimitingMiddleware.cs
│ │ │ └── JwtAuthenticationMiddleware.cs
│ │ ├── Filters/
│ │ │ └── ValidationFilter.cs
│ │ ├── DTOs/
│ │ │ ├── Requests/
│ │ │ │ ├── RegisterProviderDto.cs
│ │ │ │ ├── LoginDto.cs
│ │ │ │ ├── BookAppointmentDto.cs
│ │ │ │ ├── ConfigureAvailabilityDto.cs
│ │ │ │ └── UpdateProfileDto.cs
│ │ │ └── Responses/
│ │ │ ├── ProviderPublicProfileDto.cs
│ │ │ ├── TimeSlotDto.cs
│ │ │ ├── AppointmentDto.cs
│ │ │ └── AuthResponseDto.cs
│ │ ├── Program.cs
│ │ ├── appsettings.json
│ │ ├── appsettings.Development.json
│ │ └── QuickMeet.API.csproj
│ │
│ ├── QuickMeet.Core/ # Business Logic Layer
│ │ ├── Entities/
│ │ │ ├── Provider.cs
│ │ │ ├── Appointment.cs
│ │ │ ├── TimeSlot.cs
│ │ │ ├── ProviderAvailability.cs
│ │ │ └── ScheduledReminder.cs
│ │ ├── Interfaces/
│ │ │ ├── Services/
│ │ │ │ ├── IAuthService.cs
│ │ │ │ ├── IAppointmentService.cs
│ │ │ │ ├── IAvailabilityService.cs
│ │ │ │ ├── INotificationService.cs
│ │ │ │ └── ISchedulerService.cs
│ │ │ └── Repositories/
│ │ │ ├── IProviderRepository.cs
│ │ │ ├── IAppointmentRepository.cs
│ │ │ ├── ITimeSlotRepository.cs
│ │ │ └── IUnitOfWork.cs
│ │ ├── Services/
│ │ │ ├── AuthService.cs
│ │ │ ├── AppointmentService.cs
│ │ │ ├── AvailabilityService.cs
│ │ │ ├── NotificationService.cs
│ │ │ └── SchedulerService.cs
│ │ ├── Exceptions/
│ │ │ ├── BusinessException.cs
│ │ │ ├── SlotNotAvailableException.cs
│ │ │ └── NotFoundException.cs
│ │ ├── Validators/
│ │ │ ├── RegisterProviderValidator.cs
│ │ │ ├── BookAppointmentValidator.cs
│ │ │ └── AvailabilityConfigValidator.cs
│ │ └── QuickMeet.Core.csproj
│ │
│ └── QuickMeet.Infrastructure/ # Data Access Layer
│ ├── Data/
│ │ ├── ApplicationDbContext.cs
│ │ ├── Configurations/
│ │ │ ├── ProviderConfiguration.cs
│ │ │ ├── AppointmentConfiguration.cs
│ │ │ └── TimeSlotConfiguration.cs
│ │ └── Migrations/
│ │ └── [timestamp]_InitialCreate.cs
│ ├── Repositories/
│ │ ├── ProviderRepository.cs
│ │ ├── AppointmentRepository.cs
│ │ ├── TimeSlotRepository.cs
│ │ └── UnitOfWork.cs
│ ├── StoredProcedures/
│ │ ├── sp_GetAvailableSlots.sql
│ │ ├── sp_GenerateTimeSlots.sql
│ │ └── sp_GetProviderStatistics.sql
│ ├── Email/
│ │ ├── EmailService.cs
│ │ └── Templates/
│ │ ├── ConfirmationEmail.html
│ │ ├── ReminderEmail.html
│ │ └── CancellationEmail.html
│ └── QuickMeet.Infrastructure.csproj
│
├── tests/
│ ├── QuickMeet.UnitTests/ # Unit Tests
│ │ ├── Services/
│ │ │ ├── AppointmentServiceTests.cs
│ │ │ ├── AvailabilityServiceTests.cs
│ │ │ └── AuthServiceTests.cs
│ │ ├── Validators/
│ │ │ ├── BookAppointmentValidatorTests.cs
│ │ │ └── RegisterProviderValidatorTests.cs
│ │ ├── Fixtures/
│ │ │ └── TestDataFixture.cs
│ │ └── QuickMeet.UnitTests.csproj
│ │
│ ├── QuickMeet.IntegrationTests/ # Integration Tests
│ │ ├── Controllers/
│ │ │ ├── AppointmentsControllerTests.cs
│ │ │ ├── ProvidersControllerTests.cs
│ │ │ └── AuthControllerTests.cs
│ │ ├── Helpers/
│ │ │ ├── WebApplicationFactoryHelper.cs
│ │ │ └── DatabaseHelper.cs
│ │ └── QuickMeet.IntegrationTests.csproj
│ │
│ └── QuickMeet.E2ETests/ # End-to-End Tests
│ ├── Scenarios/
│ │ ├── BookAppointmentTests.cs
│ │ ├── CancelAppointmentTests.cs
│ │ ├── ProviderRegistrationTests.cs
│ │ └── ConfigureAvailabilityTests.cs
│ ├── PageObjects/
│ │ ├── PublicProfilePage.cs
│ │ ├── BookingPage.cs
│ │ ├── DashboardPage.cs
│ │ └── LoginPage.cs
│ ├── Helpers/
│ │ └── PlaywrightHelper.cs
│ └── QuickMeet.E2ETests.csproj
│
├── docs/
│ ├── API-Documentation.md
│ ├── Database-Schema.md
│ ├── Architecture-Decisions.md
│ └── Deployment-Guide.md
│
├── docker/
│ ├── Dockerfile.api
│ ├── Dockerfile.frontend
│ └── docker-compose.yml
│
├── .github/
│ └── workflows/
│ ├── ci-backend.yml
│ ├── ci-frontend.yml
│ └── deploy.yml
│
├── QuickMeet.sln
└── README.md
```

### Frontend - Angular Structure

```
quickmeet-frontend/
├── src/
│ ├── app/
│ │ ├── public/
│ │ │ ├── profile/
│ │ │ │ └── public-profile.component.ts # Perfil público (standalone)
│ │ │ ├── cancel/
│ │ │ │ └── cancel.component.ts # Cancelar cita (standalone)
│ │ │ └── public.routes.ts # Rutas standalone para público
│ │ ├── auth/
│ │ │ ├── login/
│ │ │ │ └── login.component.ts # Standalone
│ │ │ ├── register/
│ │ │ │ └── register.component.ts # Standalone
│ │ │ └── auth.routes.ts # Rutas standalone para auth
│ │ ├── dashboard/
│ │ │ ├── dashboard/
│ │ │ │ └── dashboard.component.ts # Standalone
│ │ │ ├── availability/
│ │ │ │ └── availability.component.ts # Standalone
│ │ │ ├── appointments/
│ │ │ │ └── appointments.component.ts # Standalone
│ │ │ ├── profile/
│ │ │ │ └── profile.component.ts # Standalone
│ │ │ └── dashboard.routes.ts # Rutas standalone para dashboard (con auth guard)
│ │ ├── app.component.ts # Root component standalone
│ │ ├── app.routes.ts # Configuración de rutas principal (provideRouter)
│ │ └── globals.css # Global styles (incluyendo Tailwind)
│ │
│ ├── components/
│ │ ├── ui/ # Componentes reutilizables standalone
│ │ │ ├── button.component.ts
│ │ │ ├── card.component.ts
│ │ │ ├── input.component.ts
│ │ │ ├── calendar.component.ts
│ │ │ └── ... # Todos standalone con imports explícitos
│ │ ├── public/
│ │ │ ├── public-profile.component.ts # Redundante si ya en app, pero para reutilización
│ │ │ ├── date-selector.component.ts
│ │ │ ├── time-slot-grid.component.ts
│ │ │ └── booking-form.component.ts
│ │ ├── dashboard/
│ │ │ ├── dashboard-header.component.ts
│ │ │ ├── appointment-list.component.ts
│ │ │ ├── calendar-view.component.ts
│ │ │ └── stats-cards.component.ts
│ │ ├── availability/
│ │ │ ├── weekly-schedule.component.ts
│ │ │ ├── day-config.component.ts
│ │ │ └── block-dates.component.ts
│ │ └── shared/
│ │ ├── navbar.component.ts
│ │ ├── footer.component.ts
│ │ └── loading-spinner.component.ts
│ │
│ ├── lib/
│ │ ├── api/
│ │ │ ├── api.service.ts # HttpClient injectable (provideIn: 'root')
│ │ │ ├── appointments.service.ts
│ │ │ ├── providers.service.ts
│ │ │ ├── auth.service.ts
│ │ │ └── availability.service.ts
│ │ ├── utils/
│ │ │ ├── class.utils.ts # Utility para classes (similar cn)
│ │ │ ├── dates.utils.ts
│ │ │ └── validators.ts # Custom validators para forms
│ │ └── guards/
│ │ ├── auth.guard.ts # Para rutas protegidas
│ │
│ ├── types/
│ │ ├── api.types.ts # Tipos para responses API
│ │ ├── models.ts # Modelos de dominio
│ │ └── forms.ts # Interfaces para forms
│ │
│ └── config/
│ ├── environment.ts # Configuración de entorno
│ └── api.config.ts # Endpoints API
│
├── assets/
│ ├── images/
│ └── icons/
│
├── tests/
│ └── e2e/ # Playwright tests
│ └── booking-flow.spec.ts
│
├── angular.json
├── tailwind.config.js
├── tsconfig.json
├── package.json
└── README.md
```

## Modelo de Datos - Base de Datos

### Esquema de Tablas

#### Tabla: Providers

Almacena información de los profesionales registrados.

```sql
CREATE TABLE Providers (
    ProviderId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(30) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    PhotoUrl NVARCHAR(500) NULL,
    AppointmentDuration INT NOT NULL DEFAULT 30, -- minutos
    BufferBetweenAppointments INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'PendingVerification',
    -- Status: PendingVerification, Active, Suspended
    EmailVerified BIT NOT NULL DEFAULT 0,
    EmailVerificationToken UNIQUEIDENTIFIER NULL,
    VerificationTokenExpiry DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT CK_AppointmentDuration
        CHECK (AppointmentDuration IN (15, 30, 45, 60, 90)),
    CONSTRAINT CK_Buffer
        CHECK (BufferBetweenAppointments IN (0, 5, 10, 15))
);
CREATE INDEX IX_Providers_Username ON Providers(Username);
CREATE INDEX IX_Providers_Email ON Providers(Email);
CREATE INDEX IX_Providers_Status ON Providers(Status);
```

#### Tabla: ProviderAvailability

Define la disponibilidad semanal recurrente del profesional.

```sql
CREATE TABLE ProviderAvailability (
    AvailabilityId INT IDENTITY(1,1) PRIMARY KEY,
    ProviderId INT NOT NULL,
    DayOfWeek INT NOT NULL, -- 0=Sunday, 1=Monday, ..., 6=Saturday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    BreakStartTime TIME NULL,
    BreakEndTime TIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT FK_Availability_Provider
        FOREIGN KEY (ProviderId)
        REFERENCES Providers(ProviderId)
        ON DELETE CASCADE,
    CONSTRAINT CK_DayOfWeek
        CHECK (DayOfWeek BETWEEN 0 AND 6),
    CONSTRAINT CK_TimeOrder
        CHECK (EndTime > StartTime),
    CONSTRAINT CK_BreakTimeOrder
        CHECK (BreakEndTime IS NULL OR BreakEndTime > BreakStartTime)
);
CREATE INDEX IX_Availability_Provider
    ON ProviderAvailability(ProviderId, DayOfWeek);
```

#### Tabla: BlockedDates

Fechas específicas donde el profesional no está disponible.

```sql
CREATE TABLE BlockedDates (
    BlockedDateId INT IDENTITY(1,1) PRIMARY KEY,
    ProviderId INT NOT NULL,
    BlockedDate DATE NOT NULL,
    StartTime TIME NULL, -- NULL = día completo bloqueado
    EndTime TIME NULL,
    Reason NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT FK_BlockedDates_Provider
        FOREIGN KEY (ProviderId)
        REFERENCES Providers(ProviderId)
        ON DELETE CASCADE,
    CONSTRAINT UQ_BlockedDate_Provider
        UNIQUE (ProviderId, BlockedDate, StartTime)
);
CREATE INDEX IX_BlockedDates_Provider_Date
    ON BlockedDates(ProviderId, BlockedDate);
```

#### Tabla: TimeSlots

Slots de tiempo específicos generados automáticamente.

```sql
CREATE TABLE TimeSlots (
    SlotId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProviderId INT NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',
    -- Status: Available, Reserved, Blocked
    ReservedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT FK_TimeSlots_Provider
        FOREIGN KEY (ProviderId)
        REFERENCES Providers(ProviderId)
        ON DELETE CASCADE,
    CONSTRAINT CK_SlotTimeOrder
        CHECK (EndTime > StartTime),
    CONSTRAINT CK_SlotStatus
        CHECK (Status IN ('Available', 'Reserved', 'Blocked'))
);
CREATE INDEX IX_TimeSlots_Provider_Date
    ON TimeSlots(ProviderId, StartTime, Status);
CREATE INDEX IX_TimeSlots_Status
    ON TimeSlots(Status)
    WHERE Status = 'Available';
```

#### Tabla: Appointments

Citas agendadas por clientes.

```sql
CREATE TABLE Appointments (
    AppointmentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SlotId BIGINT NOT NULL,
    ProviderId INT NOT NULL,
    ClientName NVARCHAR(100) NOT NULL,
    ClientEmail NVARCHAR(255) NOT NULL,
    ClientPhone NVARCHAR(20) NULL,
    Notes NVARCHAR(500) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    -- Status: Confirmed, Cancelled, Completed, NoShow
    CancellationToken UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    NotificationPending BIT NOT NULL DEFAULT 0,
    CancelledAt DATETIME2 NULL,
    CancelledBy NVARCHAR(20) NULL, -- Client, Provider, System
    CompletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT FK_Appointments_Slot
        FOREIGN KEY (SlotId)
        REFERENCES TimeSlots(SlotId),
    CONSTRAINT FK_Appointments_Provider
        FOREIGN KEY (ProviderId)
        REFERENCES Providers(ProviderId),
    CONSTRAINT CK_AppointmentStatus
        CHECK (Status IN ('Confirmed', 'Cancelled', 'Completed', 'NoShow')),
    CONSTRAINT UQ_Appointment_Slot
        UNIQUE (SlotId)
);
CREATE INDEX IX_Appointments_Provider
    ON Appointments(ProviderId, Status);
CREATE INDEX IX_Appointments_Client
    ON Appointments(ClientEmail, Status);
CREATE INDEX IX_Appointments_CancellationToken
    ON Appointments(CancellationToken);
CREATE INDEX IX_Appointments_CreatedAt
    ON Appointments(CreatedAt);
```

#### Tabla: ScheduledReminders

Jobs de recordatorios programados.

```sql
CREATE TABLE ScheduledReminders (
    ReminderId BIGINT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId UNIQUEIDENTIFIER NOT NULL,
    ScheduledFor DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    -- Status: Pending, Sent, Failed, Cancelled
    SentAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(500) NULL,
    Attempts INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
   
    CONSTRAINT FK_Reminders_Appointment
        FOREIGN KEY (AppointmentId)
        REFERENCES Appointments(AppointmentId)
        ON DELETE CASCADE,
    CONSTRAINT CK_ReminderStatus
        CHECK (Status IN ('Pending', 'Sent', 'Failed', 'Cancelled'))
);
CREATE INDEX IX_Reminders_Scheduled
    ON ScheduledReminders(ScheduledFor, Status)
    WHERE Status = 'Pending';
```

### Stored Procedures Principales

#### sp_GetAvailableSlots

Retorna slots disponibles para una fecha específica.

```sql
CREATE PROCEDURE sp_GetAvailableSlots
    @ProviderId INT,
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
   
    SELECT
        ts.SlotId,
        ts.StartTime,
        ts.EndTime
    FROM TimeSlots ts
    WHERE ts.ProviderId = @ProviderId
        AND CAST(ts.StartTime AS DATE) = @Date
        AND ts.Status = 'Available'
        AND ts.StartTime >= DATEADD(HOUR, 2, GETUTCDATE())
    ORDER BY ts.StartTime;
END
GO
```

#### sp_GenerateTimeSlots

Genera slots automáticamente basándose en disponibilidad.

```sql
CREATE PROCEDURE sp_GenerateTimeSlots
    @ProviderId INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
   
    BEGIN TRANSACTION;
   
    DECLARE @CurrentDate DATE = @StartDate;
    DECLARE @DayOfWeek INT;
    DECLARE @Duration INT;
    DECLARE @Buffer INT;
   
    -- Obtener configuración del profesional
    SELECT
        @Duration = AppointmentDuration,
        @Buffer = BufferBetweenAppointments
    FROM Providers
    WHERE ProviderId = @ProviderId;
   
    -- Iterar sobre cada día en el rango
    WHILE @CurrentDate <= @EndDate
    BEGIN
        SET @DayOfWeek = DATEPART(WEEKDAY, @CurrentDate) - 1;
       
        -- Insertar slots basándose en disponibilidad
        INSERT INTO TimeSlots (ProviderId, StartTime, EndTime, Status)
        SELECT
            @ProviderId,
            DATEADD(MINUTE, n * (@Duration + @Buffer),
                    CAST(@CurrentDate AS DATETIME) +
                    CAST(pa.StartTime AS DATETIME)),
            DATEADD(MINUTE, n * (@Duration + @Buffer) + @Duration,
                    CAST(@CurrentDate AS DATETIME) +
                    CAST(pa.StartTime AS DATETIME)),
            'Available'
        FROM ProviderAvailability pa
        CROSS JOIN (
            SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
            FROM sys.objects
        ) numbers
        WHERE pa.ProviderId = @ProviderId
            AND pa.DayOfWeek = @DayOfWeek
            AND pa.IsActive = 1
            AND DATEADD(MINUTE, n * (@Duration + @Buffer) + @Duration,
                        CAST(@CurrentDate AS DATETIME) +
                        CAST(pa.StartTime AS DATETIME))
                <= CAST(@CurrentDate AS DATETIME) + CAST(pa.EndTime AS DATETIME)
            -- Excluir breaks
            AND NOT (
                pa.BreakStartTime IS NOT NULL
                AND CAST(DATEADD(MINUTE, n * (@Duration + @Buffer),
                         CAST(@CurrentDate AS DATETIME) +
                         CAST(pa.StartTime AS DATETIME)) AS TIME)
                    >= pa.BreakStartTime
                AND CAST(DATEADD(MINUTE, n * (@Duration + @Buffer),
                         CAST(@CurrentDate AS DATETIME) +
                         CAST(pa.StartTime AS DATETIME)) AS TIME)
                    < pa.BreakEndTime
            )
            -- Excluir fechas bloqueadas
            AND NOT EXISTS (
                SELECT 1
                FROM BlockedDates bd
                WHERE bd.ProviderId = @ProviderId
                    AND bd.BlockedDate = @CurrentDate
                    AND (bd.StartTime IS NULL OR -- Día completo
                         CAST(DATEADD(MINUTE, n * (@Duration + @Buffer),
                              CAST(@CurrentDate AS DATETIME) +
                              CAST(pa.StartTime AS DATETIME)) AS TIME)
                             >= bd.StartTime
                         AND CAST(DATEADD(MINUTE, n * (@Duration + @Buffer),
                              CAST(@CurrentDate AS DATETIME) +
                              CAST(pa.StartTime AS DATETIME)) AS TIME)
                             < bd.EndTime)
            );
       
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
   
    COMMIT TRANSACTION;
END
GO
```

## Entregables del Proyecto

### Entregables de Código

1. **Backend API (.NET 8)**
   - Código fuente completo con Clean Architecture
   - Controllers, Services, Repositories implementados
   - Configuración de Entity Framework y Dapper
   - Middleware de autenticación JWT
   - Middleware de manejo de excepciones
   - Configuración de Swagger/OpenAPI
   - Archivos de configuración (appsettings.json)

2. **Base de Datos SQL Server**
   - Scripts SQL de creación de tablas
   - Stored procedures implementados (`sp_GetAvailableSlots`, `sp_GenerateTimeSlots`)
   - Índices optimizados para queries frecuentes
   - Constraints y relaciones definidas
   - Script de datos de prueba (seed data)
   - Migraciones de Entity Framework

3. **Frontend Angular**
   - Aplicación completa con standalone components y routes
   - Componentes UI reutilizables (todos standalone)
   - Integración con API mediante HttpClient
   - Formularios reactivos con typed forms y custom validators
   - Manejo de estado con signals y NgRx si aplica
   - Diseño responsive con TailwindCSS
   - Templates de email (HTML)

### Entregables de Testing

1. **Unit Tests (xUnit)**
   - Tests de Services (AppointmentService, AvailabilityService, AuthService)
   - Tests de Validators (FluentValidation)
   - Mocks con Moq para dependencias
   - Code coverage mínimo 80%
   - Tests organizados con patrón AAA (Arrange, Act, Assert)
   - Uso de FluentAssertions para assertions legibles

2. **Integration Tests**
   - Tests de Controllers con WebApplicationFactory
   - Tests de endpoints críticos (POST /appointments, GET /slots)
   - Configuración de base de datos en memoria o TestContainers
   - Tests de flujos completos (registro → configuración → agendamiento)

3. **E2E Tests (Playwright)**
   - Test: Cliente agenda cita completa (CU-03)
   - Test: Cliente cancela cita (CU-04)
   - Test: Profesional configura disponibilidad (CU-02)
   - Test: Prevención de double-booking (concurrencia)
   - Screenshots de fallos automáticos
   - Videos de ejecución de tests

### Entregables de Documentación

1. **README.md Principal**
   - Descripción del proyecto y problema que resuelve
   - Capturas de pantalla de la aplicación
   - GIF/video demo del flujo principal
   - Instrucciones de instalación local
   - Stack tecnológico utilizado
   - Enlace al deploy en vivo

2. **Documentación Técnica**
   - Arquitectura del sistema (diagrama)
   - Modelo de datos (diagrama ER)
   - API Documentation (Swagger UI + Markdown)
   - Guía de configuración de desarrollo local
   - Explicación de decisiones arquitectónicas

3. **Guía de Deploy**
   - Instrucciones para deploy en Azure/Railway
   - Configuración de variables de entorno
   - Setup de base de datos en producción
   - Configuración de CI/CD con GitHub Actions

4. **Testing Documentation**
   - Estrategia de testing implementada
   - Cómo ejecutar tests localmente
   - Reporte de code coverage
   - Escenarios críticos cubiertos

### Entregables de DevOps

1. **Docker Configuration**
   - Dockerfile para backend API
   - Dockerfile para frontend (si aplica)
   - docker-compose.yml para desarrollo local
   - .dockerignore configurado

2. **CI/CD Pipeline (GitHub Actions)**
   - Workflow de CI para backend (build + tests)
   - Workflow de CI para frontend
   - Workflow de deploy automático
   - Badge de build status en README
   - Badge de code coverage

3. **Scripts de Automatización**
   - Script de setup inicial de base de datos
   - Script de seed data para desarrollo
   - Script de generación de slots (job programado)
   - Script de envío de recordatorios (job background)

### Entregables Adicionales

1. **Video Demo (1-2 minutos)**
   - Demostración del flujo completo de agendamiento
   - Narración explicando el valor y funcionalidad
   - Subido a LinkedIn o YouTube

2. **Post de LinkedIn**
   - Redacción del post con hook efectivo
   - Carrusel de imágenes (5-7 slides)
   - Hashtags relevantes (#dotnet, #backend, #guatemala)
   - Enlace al repositorio y deploy

3. **Repositorio GitHub**
   - Código organizado y limpio
   - README completo con badges
   - Issues y Pull Requests bien documentados
   - .gitignore configurado correctamente
   - Licencia (MIT recomendada)

## Criterios de Aceptación y Validación

### Funcionales

1. **Registro y Autenticación**
   - Un profesional puede registrarse con email único
   - Sistema envía email de verificación
   - Profesional puede iniciar sesión con credenciales correctas
   - JWT token es generado y válido por 24 horas
   - Contraseñas son hasheadas con BCrypt

2. **Configuración de Disponibilidad**
   - Profesional puede configurar horario semanal
   - Sistema genera slots automáticamente para 60 días
   - Breaks son respetados en generación de slots
   - Fechas bloqueadas no permiten agendamientos
   - Cambios en disponibilidad no afectan citas ya agendadas

3. **Agendamiento de Citas**
   - Cliente puede ver perfil público sin autenticación
   - Cliente ve solo slots disponibles en tiempo real
   - No se permite agendar con menos de 2 horas de anticipación
   - Sistema previene double-booking (probado con concurrencia)
   - Ambas partes reciben email de confirmación
   - Token de cancelación único es generado

4. **Cancelación de Citas**
   - Cliente puede cancelar mediante enlace único
   - Slot cancelado queda disponible inmediatamente
   - Ambas partes son notificadas de la cancelación
   - Token de cancelación es de un solo uso

5. **Dashboard del Profesional**
   - Profesional ve todas sus citas agendadas
   - Puede filtrar por fecha y estado
   - Ve información completa del cliente por cita
   - Puede marcar citas como completadas
   - Ve estadísticas básicas de ocupación

### No Funcionales

1. **Performance**
   - API responde en menos de 100ms para queries de slots
   - API responde en menos de 200ms para crear appointment
   - Frontend carga página pública en menos de 2 segundos
   - Sistema maneja 10 usuarios concurrentes sin degradación

2. **Seguridad**
   - Passwords hasheados con BCrypt (cost 12+)
   - JWT tokens con expiración configurada
   - SQL Injection prevenido con parámetros
   - CORS configurado apropiadamente
   - Rate limiting en endpoints públicos
   - Validación de input en frontend y backend

3. **Confiabilidad**
   - Transacciones SQL garantizan consistencia
   - Manejo de errores apropiado en toda la aplicación
   - Logs estructurados para debugging
   - Retry logic para envío de emails
   - Graceful degradation si email service falla

4. **Usabilidad**
   - Interfaz intuitiva sin curva de aprendizaje
   - Responsive en mobile, tablet y desktop
   - Mensajes de error claros y accionables
   - Feedback visual para todas las acciones
   - Proceso de agendamiento en menos de 30 segundos

5. **Mantenibilidad**
   - Código organizado con Clean Architecture
   - Separación clara de responsabilidades
   - Tests cubren al menos 80% del código
   - Documentación técnica completa
   - Nombres de variables y métodos descriptivos

### Testing

1. **Unit Tests**
   - Mínimo 80% code coverage en capa de servicios
   - Todos los casos edge están cubiertos
   - Tests son independientes y repetibles
   - Tests ejecutan en menos de 10 segundos

2. **Integration Tests**
   - Todos los endpoints críticos tienen tests
   - Tests validan responses HTTP correctos
   - Tests validan persistencia en base de datos
   - Tests ejecutan en menos de 30 segundos

3. **E2E Tests**
   - Flujo completo de agendamiento funciona end-to-end
   - Test de concurrencia previene double-booking
   - Test de cancelación libera slot correctamente
   - Tests producen screenshots/videos en fallos

## Plan de Desarrollo Sugerido

### Sprint 1: Backend Foundation (5 días)

**Día 1-2: Setup y Arquitectura**

- Crear solution .NET con 3 proyectos (API, Core, Infrastructure)
- Setup Entity Framework + SQL Server
- Configurar Dependency Injection
- Crear entidades principales (Provider, Appointment, TimeSlot)
- Implementar ApplicationDbContext
- Primera migración y creación de base de datos

**Día 3-4: Autenticación y Providers**

- Implementar AuthService (registro, login, JWT)
- Implementar ProviderRepository y ProviderService
- Crear AuthController y ProvidersController
- Setup de email service básico
- Implementar validadores con FluentValidation

**Día 5: Disponibilidad**

- Implementar AvailabilityService
- Crear stored procedure `sp_GenerateTimeSlots`
- Implementar AvailabilityController
- Configurar Hangfire para jobs

### Sprint 2: Core Booking Logic + Tests (5 días)

**Día 6-7: Appointment Logic**

- Implementar AppointmentService con lógica de booking
- Implementar stored procedure `sp_GetAvailableSlots`
- Crear AppointmentsController con transacciones
- Implementar lógica de cancelación
- Implementar NotificationService con templates

**Día 8-9: Unit Tests**

- Setup de proyecto de unit tests
- Tests de AppointmentService (casos happy path y edge cases)
- Tests de AvailabilityService
- Tests de AuthService
- Tests de validadores
- Configurar Coverlet para code coverage

**Día 10: Integration Tests**

- Setup de WebApplicationFactory
- Tests de controllers principales
- Tests de endpoints críticos
- Validar persistencia en base de datos

### Sprint 3: Frontend (4 días)

**Día 11-12: Pages Públicas**

- Setup Angular 21 con standalone components y TailwindCSS
- Implementar componente standalone para perfil público
- Implementar selector de fecha y horarios (standalone components)
- Implementar formulario de booking (reactive forms)
- Integrar con API usando HttpClient + interceptors y RxJS

**Día 13: Auth y Dashboard**

- Componentes standalone para login y registro
- Dashboard standalone del profesional
- Lista de citas con filtros (usando signals para estado)
- Componente de configuración de disponibilidad

**Día 14: Polish UI**

- Implementar diseño responsive con Tailwind
- Agregar loading states y error handling (interceptors)
- Implementar toast notifications
- Optimizar performance (lazy loading con defer, change detection OnPush)

### Sprint 4: E2E Tests + Deploy (3 días)

**Día 15: E2E Testing**

- Setup Playwright
- Test: Flujo completo de agendamiento
- Test: Cancelación de cita
- Test: Configuración de disponibilidad
- Test de concurrencia (double-booking prevention)

**Día 16: CI/CD**

- Crear GitHub Actions workflows
- Configurar pipeline de CI (build + tests)
- Setup Docker y docker-compose
- Configurar deploy automático

**Día 17: Deploy y Documentación Final**

- Deploy backend a Azure/Railway
- Deploy frontend a Vercel o Azure Static Web Apps
- Configurar base de datos en producción
- Completar README con capturas y demo
- Grabar video demo
- Escribir post de LinkedIn

### Tiempo Total Estimado

- **Backend + Tests:** 10 días
- **Frontend:** 4 días
- **E2E + Deploy:** 3 días
- **Total:** 17 días (aproximadamente 2.5 semanas trabajando full-time)

*Nota: Tiempos son estimaciones. Ajustar según experiencia personal y complejidad encontrada.*

## Rúbrica de Evaluación del Proyecto

### Para Evaluación Técnica en Entrevistas

**Arquitectura y Diseño (25 puntos)**

- **Clean Architecture implementada:** 5 pts
- **Separación de responsabilidades clara:** 5 pts
- **Uso de patrones (Repository, Unit of Work):** 5 pts
- **Dependency Injection correcta:** 5 pts
- **Modelo de datos normalizado:** 5 pts

**Implementación de .NET y SQL (25 puntos)**

- **API RESTful bien diseñada:** 5 pts
- **Entity Framework + Dapper correctamente:** 5 pts
- **Stored procedures optimizados:** 5 pts
- **Manejo de transacciones y concurrencia:** 5 pts
- **Índices y optimización de queries:** 5 pts

**Testing (25 puntos)**

- **Unit tests con +80% coverage:** 10 pts
- **Integration tests de endpoints críticos:** 8 pts
- **E2E tests con Playwright:** 7 pts

**Funcionalidad y Robustez (15 puntos)**

- **Prevención de double-booking funciona:** 5 pts
- **Manejo de errores apropiado:** 5 pts
- **Validaciones en frontend y backend:** 5 pts

**Documentación y Presentación (10 puntos)**

- **README completo y profesional:** 4 pts
- **Código limpio y bien comentado:** 3 pts
- **Video demo efectivo:** 3 pts

**Puntaje Total: 100 puntos**

- **90-100 pts:** Proyecto excepcional - Altamente competitivo para posición
- **80-89 pts:** Proyecto sólido - Cumple expectativas de junior developer
- **70-79 pts:** Proyecto básico - Necesita mejoras en algunas áreas
- **<70 pts:** Proyecto incompleto - No demuestra skills requeridas

## Notas Finales y Recomendaciones

### Puntos Clave para Demostrar en Entrevistas

Cuando presentes este proyecto en entrevistas con Precredit u otras empresas:

1. **Enfatiza la lógica de concurrencia:**
   - "Implementé transacciones SQL con SERIALIZABLE isolation level y row-level locking para garantizar que dos clientes no puedan reservar el mismo slot simultáneamente."
   - "Validé esto con tests de concurrencia usando Playwright ejecutando múltiples sesiones paralelas."

2. **Destaca el uso de stored procedures:**
   - "Para queries complejos como la generación de slots y búsqueda de disponibilidad, utilicé stored procedures que mejoraron el performance en 40%."
   - "Esto me permitió mantener lógica de negocio crítica en la capa de datos."

3. **Habla sobre testing profesional:**
   - "Logré 85% de code coverage con unit tests usando xUnit y Moq."
   - "Los integration tests validan flujos completos desde HTTP request hasta persistencia en BD."
   - "Los E2E tests con Playwright simulan usuarios reales agendando citas."

4. **Menciona decisiones arquitectónicas:**
   - "Apliqué Clean Architecture separando API, Business Logic y Data Access."
   - "Usé Repository Pattern para abstraer acceso a datos y facilitar testing."
   - "Implementé el patrón Unit of Work para manejar transacciones complejas."

### Diferenciadores para LinkedIn

- **No es otro CRUD genérico:** Resuelve un problema real que todos entienden
- **Demuestra madurez técnica:** Testing completo, patrones, optimización
- **Es usable:** Deploy en vivo, video demo, interfaz pulida
- **Stack profesional:** .NET 8, SQL Server, testing robusto

### Mejoras Futuras (menciona en post de LinkedIn)

Para demostrar visión y pensamiento estratégico:

- "Próximos pasos: Integración con Google Calendar"
- "En roadmap: Sistema de recordatorios por SMS"
- "Evaluando: Analytics para profesionales (métricas de negocio)"

### Recursos de Aprendizaje Durante Desarrollo

Si encuentras bloqueos técnicos:

- **.NET:** Microsoft Learn oficial, YouTube de Nick Chapsas
- **SQL Server:** Brent Ozar, SQL Server Central
- **Testing:** Documentación de xUnit, Playwright official docs
- **Clean Architecture:** "Clean Architecture" de Robert C. Martin

### Checklist Final Antes de Publicar

- [ ] API deployada y funcional
- [ ] Frontend deployado y accesible
- [ ] README con capturas y GIF demo
- [ ] Video de 1-2 minutos grabado
- [ ] Tests pasando con +80% coverage
- [ ] Badge de build status en GitHub
- [ ] Post de LinkedIn redactado
- [ ] Código limpio y comentado
- [ ] Sin credenciales hardcodeadas
- [ ] .env.example creado

**¡Éxito con el proyecto!**

Este enunciado te proporciona toda la claridad necesaria para construir un proyecto definitivo que demuestre tus capacidades como desarrollador .NET + SQL Server.

Recuerda: No se trata de hacer TODO perfectamente, sino de demostrar que entiendes los conceptos fundamentales y puedes aplicarlos en un proyecto real y funcional.