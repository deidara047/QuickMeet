# QuickStart Guide - Ambientes de Desarrollo

Guía completa para levantar el proyecto en **Desarrollo (Dev)**, **Testing (Test)** y **Producción (Prod)**.

---

## Estructura de Bases de Datos

SQL Server en Windows local. BD separadas por ambiente:
- **QuickMeet_Dev**: Desarrollo local
- **QuickMeet_Test**: Testing E2E + Unit Tests
- **QuickMeet_Prod**: Producción

---

# Backend Setup

## 1. Development Environment (Default)

**Propósito**: Desarrollo local con todas las features habilitadas.

### Pasos
```bash
cd backend

# Establecer ambiente
$env:ASPNETCORE_ENVIRONMENT="Development"

# Ejecutar
dotnet run --project src/QuickMeet.API
```

### Configuración
- **Base de datos**: QuickMeet_Dev
- **API URL**: http://localhost:5173
- **TestController**: NO disponible (AllowDangerousOperations=false)
- **Logs**: Information level
- **Config file**: appsettings.Development.json

### Verificar que funciona
```bash
# En otra terminal
curl http://localhost:5173/api/test/ping
# Respuesta esperada: 404 Not Found (TestController no disponible)

# Verificar DB
sqlcmd -Q "SELECT name FROM sys.databases WHERE name = 'QuickMeet_Dev';"
```

---

## 2. Test Environment (E2E Testing + Unit Tests)

**Propósito**: Ambiente aislado para testing. Endpoints de testing habilitados para seeding/cleanup de datos.

### Pasos
```bash
cd backend

# Establecer ambiente
$env:ASPNETCORE_ENVIRONMENT="Test"

# Ejecutar
dotnet run --project src/QuickMeet.API
```

### Configuración
- **Base de datos**: QuickMeet_Test (separada, segura)
- **API URL**: http://localhost:5173
- **TestController**: ACTIVO (AllowDangerousOperations=true)
- **Logs**: Debug level (máxima visibilidad)
- **Config file**: appsettings.Test.json

### Endpoints de Testing Disponibles
- `POST /api/test/seed-user` - Crear usuario de test
- `DELETE /api/test/cleanup-user/{email}` - Eliminar usuario de test
- `POST /api/test/reset-database` - Reiniciar BD (destructivo)
- `GET /api/test/ping` - Health check

### Verificar que funciona
```bash
# Verificar TestController está activo
curl http://localhost:5173/api/test/ping
# Respuesta esperada: 200 OK
# {"message":"TestController is active","environment":"Test","timestamp":"..."}

# Verificar DB de test
sqlcmd -Q "SELECT name FROM sys.databases WHERE name = 'QuickMeet_Test';"

# Ejecutar unit tests
cd backend
$env:ASPNETCORE_ENVIRONMENT="Test"
dotnet test
```

---

## 3. Production Environment

**Propósito**: Simular entorno de producción. TestController NO compilado.

### Pasos
```bash
cd backend

# Establecer ambiente
$env:ASPNETCORE_ENVIRONMENT="Production"

# Ejecutar
dotnet run --project src/QuickMeet.API
```

### Configuración
- **Base de datos**: QuickMeet_Prod
- **API URL**: http://localhost:5173
- **TestController**: NO COMPILADO (compilación condicional #if DEBUG)
- **Logs**: Warning level
- **Config file**: appsettings.Production.json
- **Seguridad**: Imposible acceder a endpoints de testing (no existen en binario)

### Verificar que funciona
```bash
# Intentar acceder a TestController (debe retornar 404)
curl http://localhost:5173/api/test/ping
# Respuesta esperada: 404 Not Found
```

---

# Frontend Setup

## 1. Development Environment

**Propósito**: Desarrollo local con hot reload y debug.

### Pasos
```bash
cd frontend

# Instalar dependencias (si es primera vez)
npm install

# Ejecutar dev server
npm run start
```

### Configuración
- **Dev Server**: http://localhost:4200
- **Archivo .env**: Copia de .env.example (git ignored)
- **API Backend**: http://localhost:5173/api
- **Config**: environment.ts
- **Logs**: Debug level

### Estructura de archivos .env
```bash
# Crear .env (copia de .env.example)
cp .env.example .env

# Contenido de .env
NG_APP_ENVIRONMENT=development
NG_APP_API_URL=http://localhost:5173/api
NG_APP_API_TIMEOUT=30000
NG_APP_LOG_LEVEL=debug
NG_APP_ENABLE_DEBUG_PANEL=true
```

---

## 2. Test Environment (E2E Testing)

**Propósito**: Ejecutar tests E2E con Playwright.

### Prerequisites
Backend debe estar corriendo en **Test environment**:
```bash
cd backend
$env:ASPNETCORE_ENVIRONMENT="Test"
dotnet run --project src/QuickMeet.API
# Esperar: "Now listening on: http://localhost:5173"
```

### Frontend - Ejecutar E2E Tests
```bash
cd frontend

# Instalar dependencias si falta
npm install

# Ejecutar tests Playwright
npm run e2e

# O con UI interactivo
npm run e2e:ui

# O en modo debug
npm run e2e:debug
```

### Configuración
- **Dev Server**: http://localhost:4200 (se levanta automático)
- **Archivo .env.test**: git tracked (configuración específica para testing)
- **Backend**: Debe estar en Test environment (con TestController activo)
- **Logs**: Debug level

### Estructura de .env.test
```bash
# Archivo: frontend/.env.test
NG_APP_ENVIRONMENT=test
NG_APP_API_URL=http://localhost:5173/api
NG_APP_API_TIMEOUT=30000
NG_APP_LOG_LEVEL=debug
NG_APP_ENABLE_DEBUG_PANEL=true
```

### Verificar Setup E2E
```bash
# 1. Backend en Test environment está corriendo
curl http://localhost:5173/api/test/ping
# → 200 OK con {"message":"TestController is active",...}

# 2. Frontend dev server está corriendo
curl http://localhost:4200
# → 200 OK

# 3. Ejecutar tests
cd frontend
npm run e2e
```

---

## 3. Production Build

**Propósito**: Build optimizado para producción.

### Pasos
```bash
cd frontend

# Build optimizado
npm run build
# Output: dist/frontend/

# Para servir localmente (opcional)
npx http-server dist/frontend -p 8080
# Acceder a: http://localhost:8080
```

### Configuración
- **Build output**: dist/frontend/
- **Optimizaciones**: Bundling, minification, tree-shaking
- **API URL**: Debe apuntar a backend en Prod
- **Logs**: Warning level
- **Config**: environment.prod.ts

---

# Testing

## Unit Tests (Backend)

```bash
cd backend

# Establecer ambiente Test
$env:ASPNETCORE_ENVIRONMENT="Test"

# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true

# Archivo de test: backend/tests/QuickMeet.UnitTests/
```

---

## E2E Tests (Frontend)

### Prerequisites
1. Backend corriendo en Test environment
2. Verificar TestController disponible

```bash
# Backend en Terminal 1
cd backend
$env:ASPNETCORE_ENVIRONMENT="Test"
dotnet run --project src/QuickMeet.API
# Esperar: "Now listening on: http://localhost:5173"

# Frontend en Terminal 2
cd frontend
npm run e2e

# O UI interactivo
npm run e2e:ui

# O debug mode
npm run e2e:debug
```

### Estructura E2E
```
frontend/e2e/
├── fixtures/                    # Datos y configuración
│   ├── page.fixture.ts          # Page fixture base
│   └── users.json
├── helpers/                     # Utilidades reutilizables
│   ├── ui.helper.ts             # Selectors, assertions UI
│   ├── test-api.helper.ts       # Llamadas a endpoints /api/test/*
│   └── test-data.helper.ts      # Generación de datos de test
├── tests/
│   ├── auth/
│   │   ├── login.spec.ts        # Tests de login con E2E flow
│   │   └── register.spec.ts
│   └── shared/
│       └── (otros tests)
└── playwright.config.ts
```

### Flujo E2E Típico
```typescript
// 1. Reset de BD (asegura estado limpio)
await resetDatabase(page);

// 2. Generar usuario único
const testUser = generateUniqueUser('my-test');

// 3. Seed usuario en BD
const seededUser = await seedUser(page, testUser);

// 4. Test de UI (login, navigation, etc)
await page.safeGoto('/login');
await page.fill(selectors.loginEmail, testUser.email);
// ... más assertions

// 5. Cleanup (eliminar usuario)
await cleanupUser(page, testUser.email);
```

---

# Tabla Resumen - Ambientes

| Aspecto | Development | Test | Production |
|---------|-------------|------|-----------|
| **Base de Datos** | QuickMeet_Dev | QuickMeet_Test | QuickMeet_Prod |
| **TestController** | NO (404) | SÍ (200) | NO COMPILADO |
| **AllowDangerousOperations** | false | true | (ausente) |
| **Logs Level** | Information | Debug | Warning |
| **Propósito** | Local dev | Testing E2E | Production |
| **Frontend Dev Server** | :4200 | :4200 | N/A |
| **Backend Port** | :5173 | :5173 | :5173 |
| **Usar** | `npm start` | `npm run e2e` | `npm run build` |

---

# Troubleshooting

### Backend no inicia en Test environment
```bash
# Verificar que BD QuickMeet_Test existe
sqlcmd -Q "SELECT name FROM sys.databases WHERE name = 'QuickMeet_Test';"

# Si no existe, crearla
sqlcmd -Q "CREATE DATABASE QuickMeet_Test;"

# Aplicar migraciones
cd backend
$env:ASPNETCORE_ENVIRONMENT="Test"
dotnet ef database update --project src/QuickMeet.Infrastructure --startup-project src/QuickMeet.API
```

### E2E tests falla (TestController no disponible)
```bash
# Verificar que backend está en Test environment
curl http://localhost:5173/api/test/ping
# Si retorna 404: backend está en Development o Production

# Verificar appsettings.Test.json existe
# backend/src/QuickMeet.API/appsettings.Test.json

# Verificar que AllowDangerousOperations=true
cat backend/src/QuickMeet.API/appsettings.Test.json | grep -i dangerous
```

### Frontend no puede conectar a backend
```bash
# Verificar que backend está corriendo
curl http://localhost:5173/api/test/ping

# Verificar CORS en appsettings.{Environment}.json
# Debe incluir "http://localhost:4200" en AllowedOrigins

# Verificar NG_APP_API_URL en .env o .env.test
cat frontend/.env | grep API_URL
```