# QuickMeet - Guía de Desarrollo

Sistema de agendamiento de citas sin fricción. Desarrollo local optimizado + Docker ready para CI/CD.

## 🚀 Inicio Rápido

### Opción 1: Desarrollo LOCAL (Recomendado para desarrollo activo)

Este enfoque evita rebuilds frecuentes de Docker y es más rápido para iteración.

#### Prerequisitos
- .NET 8 SDK
- Node.js 20+
- SQL Server 2022 (o usar Docker solo para BD)
- npm o yarn

#### Setup Backend (.NET)

```bash
# Terminal 1: Iniciar solo SQL Server en Docker
docker-compose up sql-server

# Terminal 2: Backend
cd backend
dotnet restore
dotnet run --project src/QuickMeet.API

# API estará disponible en: http://localhost:5000
```

#### Setup Frontend (Angular)

```bash
# Terminal 3: Frontend
cd frontend
npm install
npm start  # o: ng serve

# Frontend estará disponible en: http://localhost:4200
```

### Opción 2: Desarrollo FULL con Docker (Validación antes de push)

Usar cuando quieras validar que todo funciona en contenedores (simula CI/CD):

```bash
# Descomentar los servicios 'api' y 'frontend' en docker-compose.yml

# Luego:
docker-compose up

# API: http://localhost:5000
# Frontend: http://localhost:4200
# SQL Server: localhost:1433
```

**Nota:** Si modificas archivos, necesitas rebuild:
```bash
docker-compose up --build
```

---

## 📁 Estructura del Proyecto

```
ProyectoCitas/
├── backend/
│   ├── src/
│   │   ├── QuickMeet.API/          # ASP.NET Core Web API
│   │   ├── QuickMeet.Core/         # Domain, Entities, Interfaces
│   │   └── QuickMeet.Infrastructure/  # Data, Repositories, Services
│   ├── tests/
│   │   ├── QuickMeet.UnitTests/    # xUnit tests
│   │   └── QuickMeet.IntegrationTests/
│   ├── QuickMeet.sln
│   ├── Dockerfile                  # Para producción
│   └── docker-compose.yml
├── frontend/
│   ├── src/
│   │   ├── app/                    # Components, Services, Guards
│   │   ├── assets/
│   │   └── environments/
│   ├── Dockerfile                  # Multi-stage para producción
│   ├── Dockerfile.dev             # Para desarrollo
│   ├── nginx.conf
│   └── angular.json
├── docker-compose.yml             # Orquestación principal
└── README.md
```

---

## 🔧 Desarrollo Backend

### Comandos Útiles

```bash
cd backend

# Restore dependencias
dotnet restore

# Build
dotnet build

# Ejecutar API
dotnet run --project src/QuickMeet.API

# Tests
dotnet test

# Unit tests con cobertura
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Crear migraciones (EF Core)
dotnet ef migrations add NombreMigracion --project src/QuickMeet.Infrastructure
```

### Variables de Entorno

Crear `backend/src/QuickMeet.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=QuickMeet;User Id=sa;Password=Develop123!@;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Secret": "your-development-secret-key-min-32-characters-needed-here",
    "Issuer": "quickmeet-api",
    "Audience": "quickmeet-client",
    "ExpiryMinutes": 60
  },
  "SendGrid": {
    "ApiKey": "SG.your-dev-key-here"
  }
}
```

---

## 🎨 Desarrollo Frontend

### Comandos Útiles

```bash
cd frontend

# Instalar dependencias
npm install

# Servidor de desarrollo (hot reload)
npm start
# o
ng serve --open

# Build producción
npm run build
# o
ng build --configuration production

# Tests
npm test

# E2E tests
npm run e2e

# Lint
npm run lint
```

### Estructura de Componentes

```
src/app/
├── core/                    # Servicios principales (auth, http)
│   ├── auth/
│   ├── interceptors/
│   └── guards/
├── shared/                  # Componentes reutilizables
│   ├── components/
│   └── pipes/
├── features/                # Feature modules
│   ├── auth/
│   ├── professional/
│   ├── booking/
│   └── dashboard/
└── app.module.ts
```

---

## 🐳 Docker & CI/CD

### Subir imagen a registrio (GitHub Packages)

```bash
# Build backend
cd backend
docker build -f src/QuickMeet.API/Dockerfile -t ghcr.io/yourusername/quickmeet-api:latest .

# Push
docker login ghcr.io
docker push ghcr.io/yourusername/quickmeet-api:latest

# Similar para frontend
cd ../frontend
docker build -f Dockerfile -t ghcr.io/yourusername/quickmeet-frontend:latest .
docker push ghcr.io/yourusername/quickmeet-frontend:latest
```

### GitHub Actions

El workflow en `.github/workflows/ci-cd.yml`:

1. **Build Backend** → Tests → Coverage
2. **Build Frontend** → Tests → Coverage
3. **Build Docker Images** (si push a main/develop)
4. **Push to GitHub Container Registry**

**Triggers:**
- Push a `main` o `develop`
- Pull requests

---

## 🗄️ Base de Datos

### Configurar SQL Server Localmente (Opción sin Docker)

```bash
# Windows (SQL Server Express)
# Descargar: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# macOS/Linux con Docker:
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Develop123!@" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Aplicar Migraciones

```bash
cd backend

# Crear migración (después de cambiar DbContext)
dotnet ef migrations add AddUsersTable --project src/QuickMeet.Infrastructure

# Aplicar a la BD
dotnet ef database update --project src/QuickMeet.Infrastructure
```

---

## ✅ Checklist Antes de Push

- [ ] `dotnet build` sin errores (backend)
- [ ] `dotnet test` todos pasan (backend)
- [ ] `npm run lint` sin errores (frontend)
- [ ] `npm test` todos pasan (frontend)
- [ ] SQL Server corriendo y migraciones aplicadas
- [ ] API responde en `http://localhost:5000`
- [ ] Frontend carga en `http://localhost:4200`

---

## 🐛 Troubleshooting

### "Cannot connect to SQL Server"
```bash
# Verificar que SQL Server esté corriendo
docker ps | grep sql

# Si no está:
docker-compose up sql-server
```

### "Port 1433 already in use"
```bash
# Cambiar en docker-compose.yml o matar proceso:
netstat -ano | findstr :1433
taskkill /PID <PID> /F
```

### "Module not found" (Frontend)
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### ".NET restore issues"
```bash
cd backend
dotnet restore --no-cache
```

---

## 📚 Documentación

- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [Angular Docs](https://angular.io/docs)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server on Docker](https://hub.docker.com/_/microsoft-mssql-server)

---

## 🚀 Deploy

Ver instrucciones específicas para cada plataforma en `DEPLOY.md` (próximo documento).

---

**Happy coding! 🎉**
