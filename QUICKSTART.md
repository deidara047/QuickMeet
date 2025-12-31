# Database Setup
SQL Server en Windows local. BD separadas por ambiente: QuickMeet_Dev, QuickMeet_Test, QuickMeet_Prod

# Backend Setup

## 1. Development Environment (Default)
```bash
cd backend
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/QuickMeet.API
```
- Base de datos: QuickMeet_Dev
- API: http://localhost:5173
- TestController: NO disponible (AllowDangerousOperations=false)
- Logs: Information level

## 2. Test Environment (E2E Testing + Unit Tests)
```bash
cd backend
$env:ASPNETCORE_ENVIRONMENT="Test"
dotnet run --project src/QuickMeet.API
```
- Base de datos: QuickMeet_Test (separada, segura)
- API: http://localhost:5173
- TestController: ACTIVO (AllowDangerousOperations=true)
- Logs: Debug level (máxima visibilidad)
- Endpoints testing: /api/test/seed-user, /api/test/cleanup-user/{email}, /api/test/ping

## 3. Production Environment
```bash
cd backend
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project src/QuickMeet.API
```
- Base de datos: QuickMeet_Prod
- API: http://localhost:5173
- TestController: NO COMPILADO (compilación condicional #if DEBUG)
- Logs: Warning level
- Imposible acceder a endpoints de testing (no existen en binario)

# Frontend Setup

## Development
cd frontend
npm run start
# Angular dev server: http://localhost:4200
# Usa .env (copia de .env.example)

## Testing E2E
cd frontend
npm run start
# Angular dev server: http://localhost:4200
# Usa .env.test para configuración
# Ejecutar tests: npx playwright test

## Production
cd frontend
npm run build
# Build optimizado en dist/frontend/

# Testing

## Unit Tests (Backend)
cd backend
set ASPNETCORE_ENVIRONMENT=Test
dotnet test
# Ejecuta todos los tests en ambiente Test
# BD: QuickMeet_Test (no toca Dev ni Prod)

## E2E Tests (Frontend)
cd frontend
npx playwright test
# Ejecuta tests de Playwright
# Backend debe estar corriendo en ambiente Test

## Validar Fase 0 (Ambientes)
# 1. Verificar que BD de test existe
sqlcmd -Q "SELECT name FROM sys.databases WHERE name = 'QuickMeet_Test';"

# 2. Verificar migraciones en BD de test
sqlcmd -Q "USE QuickMeet_Test; SELECT name FROM sysobjects WHERE type = 'U' ORDER BY name;"

# 3. Verificar appsettings.Test.json cargado correctamente
cd backend
set ASPNETCORE_ENVIRONMENT=Test
dotnet run --project src/QuickMeet.API
# En logs debe aparecer: "Logging" y "AllowDangerousOperations": true