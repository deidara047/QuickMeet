# QuickStart - Desarrollo Local

Guía rápida para iniciar QuickMeet en tu máquina.

## Prerrequisitos

- .NET 10 SDK
- Node.js 20+
- Docker Desktop
- npm

Verifica: `dotnet --version`, `node --version`, `npm --version`, `docker --version`

## Setup Inicial (Una sola vez)

```bash
# Desde la raíz del proyecto
cp backend/.env.example.dev backend/.env.local
cp frontend/.env.example.dev frontend/.env

# Restaurar dependencias
cd backend
dotnet restore
cd ../frontend
npm install

cd ..
```

## Levantar la aplicación

Abre 3 terminales en la raíz del proyecto:

### Terminal 1: SQL Server (Docker)

```bash
docker-compose up sql-server
```

Espera a que diga `Started`. Continúa cuando SQL Server esté listo (~10 segundos).

### Terminal 2: Backend (ASP.NET Core API)

```bash
cd backend
dotnet run --project src/QuickMeet.API
```

Espera a: `Application started. Press Ctrl+C to shut down.`

Disponible en: http://localhost:5173

### Terminal 3: Frontend (Angular)

```bash
cd frontend
npm start
```

Espera a: `NOTE: Raw file sizes do not reflect...`

Disponible en: http://localhost:4200

## Ya está listo

Accede a http://localhost:4200

Hot reload automático en ambos lados. Los cambios se reflejan inmediatamente.

## Base de datos

SQL Server corre en Docker con:
- Host: localhost
- Puerto: 1433
- Usuario: sa
- Contraseña: Develop123!@

La aplicación usa:
- Usuario: quickmeet_app
- Contraseña: App123!@#Secure$Pass2025

## Detener

En cada terminal: `Ctrl+C`

Detener SQL Server:
```bash
docker-compose down
```

## Comandos útiles

```bash
# Backend
cd backend
dotnet build              # Compilar
dotnet test               # Tests
dotnet ef database update # Migraciones

# Frontend
cd frontend
npm test                  # Tests
npm run build             # Build producción
npm run lint              # Linter
```

## Conexión a la BD

Desde SQL Server Management Studio o Azure Data Studio:
- Server: localhost,1433
- Login: sa
- Password: Develop123!@

## Problemas

### "Port 4200 already in use"
```bash
netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

### "Cannot connect to SQL Server"
```bash
docker ps  # Verificar si está corriendo
docker-compose logs sql-server  # Ver logs
```

### "Module not found" (Frontend)
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### ".NET build error"
```bash
cd backend
dotnet clean
dotnet restore
dotnet build
```

## CI/CD

Los cambios a `main` o `develop` triggean el workflow en `.github/workflows/ci-cd.yml`:
- Tests unitarios
- Build Docker
- Push a GitHub Container Registry

## Más info

- README.md: Descripción general
- DEVELOPMENT_SETUP.md: Setup detallado
- ENVIRONMENT_VARIABLES.md: Variables de entorno
- rules.md: Reglas del proyecto
