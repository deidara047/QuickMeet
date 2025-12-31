¡Excelente! Te voy a mostrar la arquitectura **profesional completa** de un proyecto Angular + Express con testing y CI/CD.

my-fullstack-app/
│
├── .github/                              # CI/CD con GitHub Actions
│   └── workflows/
│       ├── ci.yml                        # Pipeline CI
│       └── cd.yml                        # Pipeline CD
│
├── frontend/                             # Angular App
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── login/
│   │   │   │   │   ├── login.component.ts
│   │   │   │   │   ├── login.component.spec.ts  ← Unit tests (Jasmine/Karma)
│   │   │   │   │   ├── login.component.html
│   │   │   │   │   └── login.component.css
│   │   │   │   ├── register/
│   │   │   │   │   ├── register.component.ts
│   │   │   │   │   └── register.component.spec.ts
│   │   │   │   └── dashboard/
│   │   │   │       ├── dashboard.component.ts
│   │   │   │       └── dashboard.component.spec.ts
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.service.spec.ts
│   │   │   │   ├── user.service.ts
│   │   │   │   └── user.service.spec.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── auth.guard.spec.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── auth.interceptor.spec.ts
│   │   │   └── app.config.ts
│   │   ├── environments/
│   │   │   ├── environment.ts           # Development
│   │   │   ├── environment.staging.ts   # Staging
│   │   │   └── environment.prod.ts      # Production
│   │   ├── assets/
│   │   ├── index.html
│   │   └── main.ts
│   ├── angular.json
│   ├── tsconfig.json
│   ├── karma.conf.js                     # Configuración unit tests
│   ├── package.json
│   ├── Dockerfile                        # Para producción
│   └── nginx.conf                        # Configuración nginx para producción
│
├── backend/                              # Express API
│   ├── src/
│   │   ├── config/
│   │   │   ├── database.js
│   │   │   ├── environment.js           # Manejo de entornos
│   │   │   └── logger.js
│   │   ├── controllers/
│   │   │   ├── auth.controller.js
│   │   │   ├── auth.controller.spec.js  ← Unit tests (Mocha + Chai)
│   │   │   ├── user.controller.js
│   │   │   └── user.controller.spec.js
│   │   ├── models/
│   │   │   ├── user.model.js
│   │   │   └── user.model.spec.js
│   │   ├── routes/
│   │   │   ├── auth.routes.js
│   │   │   ├── user.routes.js
│   │   │   └── test.routes.js           # Solo en NODE_ENV=test
│   │   ├── middleware/
│   │   │   ├── auth.middleware.js
│   │   │   ├── auth.middleware.spec.js
│   │   │   ├── error.middleware.js
│   │   │   └── validation.middleware.js
│   │   ├── services/
│   │   │   ├── auth.service.js
│   │   │   ├── auth.service.spec.js
│   │   │   ├── email.service.js
│   │   │   └── token.service.js
│   │   ├── utils/
│   │   │   ├── validators.js
│   │   │   └── helpers.js
│   │   ├── app.js                       # Express app setup
│   │   └── server.js                    # Server entry point
│   ├── test/
│   │   ├── setup.js                     # Setup para Mocha
│   │   ├── helpers/
│   │   │   └── test-helpers.js
│   │   └── integration/                 # Integration tests (opcional)
│   │       ├── auth.integration.spec.js
│   │       └── user.integration.spec.js
│   ├── .env.development
│   ├── .env.test
│   ├── .env.staging
│   ├── .env.production
│   ├── .mocharc.json                    # Configuración Mocha
│   ├── package.json
│   ├── Dockerfile                       # Para producción
│   └── nodemon.json                     # Para desarrollo
│
├── e2e/                                  # E2E Tests (Playwright)
│   ├── fixtures/
│   │   ├── users.json
│   │   ├── products.json
│   │   └── custom-fixtures.js
│   ├── helpers/
│   │   ├── auth.helper.js
│   │   ├── data.helper.js
│   │   └── api.helper.js
│   ├── pages/
│   │   ├── login.page.js
│   │   ├── register.page.js
│   │   ├── dashboard.page.js
│   │   └── base.page.js
│   ├── specs/
│   │   ├── auth/
│   │   │   ├── login.spec.js
│   │   │   └── register.spec.js
│   │   ├── user/
│   │   │   ├── profile.spec.js
│   │   │   └── settings.spec.js
│   │   └── flows/
│   │       └── complete-user-journey.spec.js
│   ├── global-setup.js                  # Setup global (limpiar DB)
│   ├── global-teardown.js
│   └── playwright.config.js
│
├── docker/                               # Docker configs
│   ├── development/
│   │   └── docker-compose.yml
│   ├── staging/
│   │   └── docker-compose.yml
│   └── production/
│       └── docker-compose.yml
│
├── scripts/                              # Scripts de utilidad
│   ├── seed-db.js                       # Seed inicial de datos
│   ├── migrate-db.js                    # Migraciones
│   └── reset-test-db.js                 # Resetear DB de testing
│
├── docs/                                 # Documentación
│   ├── API.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── .gitignore
├── .dockerignore
├── docker-compose.yml                   # Docker compose principal
├── package.json                         # Scripts root
└── README.md


# ============================================
# ESTRUCTURA DE ENTORNOS
# ============================================

Entornos definidos:
├── Development (local)
│   ├── Frontend: http://localhost:4200
│   ├── Backend: http://localhost:3000
│   └── DB: PostgreSQL local
│
├── Test (CI/CD y local)
│   ├── Frontend: http://localhost:4201
│   ├── Backend: http://localhost:3001
│   └── DB: PostgreSQL test (separada)
│
├── Staging (pre-producción)
│   ├── Frontend: https://staging.myapp.com
│   ├── Backend: https://api-staging.myapp.com
│   └── DB: PostgreSQL staging (cloud)
│
└── Production
    ├── Frontend: https://myapp.com
    ├── Backend: https://api.myapp.com
    └── DB: PostgreSQL production (cloud)


# ============================================
# FLUJO DE TESTING POR ENTORNO
# ============================================

Development:
├── Unit Tests (Angular): ng test --watch
├── Unit Tests (Backend): npm run test:unit --watch
└── E2E Tests: npm run test:e2e (contra localhost)

Test (CI):
├── Unit Tests (Angular): ng test --no-watch --code-coverage
├── Unit Tests (Backend): npm run test:unit --coverage
├── Build: npm run build
└── E2E Tests: npm run test:e2e (contra build)

Staging:
├── Deploy automático desde main branch
├── E2E Tests: npm run test:e2e:staging
└── Smoke tests

Production:
├── Deploy manual o automático con aprobación
├── Smoke tests
└── Monitoreo continuo


# ============================================
# COMANDOS PRINCIPALES
# ============================================

# Development
npm run dev                    # Levanta frontend + backend + DB
npm run dev:frontend           # Solo Angular (ng serve)
npm run dev:backend            # Solo Express (nodemon)
npm run test:unit              # Unit tests (Angular + Backend)
npm run test:e2e               # E2E tests
npm run test:all               # Todos los tests

# Build
npm run build                  # Build frontend + backend
npm run build:frontend         # Solo Angular
npm run build:backend          # Solo Express (si aplica)

# Docker
npm run docker:dev             # Levantar en Docker (development)
npm run docker:test            # Levantar para testing
npm run docker:prod            # Build producción

# CI/CD
npm run ci:test                # Pipeline de testing completo
npm run ci:build               # Build para CI
npm run ci:deploy:staging      # Deploy a staging
npm run ci:deploy:prod         # Deploy a producción

Ahora te explico los archivos clave de configuración:

## 1. **Configuración de Entornos**

### Frontend - environments/environment.ts
```typescript
// environment.ts (Development)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:3000/api',
  environment: 'development'
};

// environment.staging.ts
export const environment = {
  production: false,
  apiUrl: 'https://api-staging.myapp.com/api',
  environment: 'staging'
};

// environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://api.myapp.com/api',
  environment: 'production'
};
```

### Backend - config/environment.js
```javascript
const dotenv = require('dotenv');

// Cargar .env según NODE_ENV
const envFile = `.env.${process.env.NODE_ENV || 'development'}`;
dotenv.config({ path: envFile });

module.exports = {
  NODE_ENV: process.env.NODE_ENV || 'development',
  PORT: process.env.PORT || 3000,
  DATABASE_URL: process.env.DATABASE_URL,
  JWT_SECRET: process.env.JWT_SECRET,
  FRONTEND_URL: process.env.FRONTEND_URL,
  
  // Flags por entorno
  isProduction: process.env.NODE_ENV === 'production',
  isTest: process.env.NODE_ENV === 'test',
  isDevelopment: process.env.NODE_ENV === 'development',
};
```

### Backend - .env files
```bash
# .env.development
NODE_ENV=development
PORT=3000
DATABASE_URL=postgresql://localhost:5432/myapp_dev
JWT_SECRET=dev-secret-change-in-prod
FRONTEND_URL=http://localhost:4200

# .env.test
NODE_ENV=test
PORT=3001
DATABASE_URL=postgresql://localhost:5432/myapp_test
JWT_SECRET=test-secret
FRONTEND_URL=http://localhost:4201

# .env.production (en servidor/secrets)
NODE_ENV=production
PORT=3000
DATABASE_URL=postgresql://user:pass@prod-db:5432/myapp
JWT_SECRET=super-secure-secret-from-vault
FRONTEND_URL=https://myapp.com
```

## 2. **Configuración de Testing**

### Backend - .mocharc.json
```json
{
  "require": ["test/setup.js"],
  "spec": "src/**/*.spec.js",
  "timeout": 5000,
  "exit": true,
  "color": true,
  "reporter": "spec"
}
```

### Backend - test/setup.js
```javascript
const chai = require('chai');
const chaiHttp = require('chai-http');

// Setup global
global.expect = chai.expect;
chai.use(chaiHttp);

// Configurar entorno de test
process.env.NODE_ENV = 'test';
```

### E2E - playwright.config.js
```javascript
export default {
  testDir: './e2e/specs',
  timeout: 30000,
  
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  
  projects: [
    {
      name: 'chromium',
      use: { browserName: 'chromium' },
    },
  ],
  
  webServer: {
    command: 'npm run start:test-env',
    port: 4200,
    reuseExistingServer: !process.env.CI,
  },
  
  globalSetup: './e2e/global-setup.js',
};
```

## 3. **Package.json Root (Monorepo style)**

```json
{
  "name": "my-fullstack-app",
  "scripts": {
    "dev": "concurrently \"npm run dev:backend\" \"npm run dev:frontend\"",
    "dev:frontend": "cd frontend && ng serve",
    "dev:backend": "cd backend && NODE_ENV=development nodemon src/server.js",
    
    "test:unit": "npm run test:unit:frontend && npm run test:unit:backend",
    "test:unit:frontend": "cd frontend && ng test --no-watch --code-coverage",
    "test:unit:backend": "cd backend && NODE_ENV=test mocha",
    "test:e2e": "playwright test",
    "test:all": "npm run test:unit && npm run test:e2e",
    
    "build": "npm run build:frontend && npm run build:backend",
    "build:frontend": "cd frontend && ng build --configuration production",
    "build:backend": "cd backend && echo 'Backend no require build'",
    
    "docker:dev": "docker-compose -f docker/development/docker-compose.yml up",
    "docker:test": "docker-compose -f docker/development/docker-compose.yml -f docker-compose.test.yml up",
    "docker:prod": "docker-compose -f docker/production/docker-compose.yml up",
    
    "start:test-env": "concurrently \"npm run start:backend:test\" \"npm run start:frontend:test\"",
    "start:backend:test": "cd backend && NODE_ENV=test node src/server.js",
    "start:frontend:test": "cd frontend && ng serve --port 4201"
  },
  "devDependencies": {
    "concurrently": "^8.0.0",
    "@playwright/test": "^1.40.0"
  }
}
```

## 4. **Docker Configuration**

### Frontend Dockerfile
```dockerfile
# frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Backend Dockerfile
```dockerfile
# backend/Dockerfile
FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
EXPOSE 3000
CMD ["node", "src/server.js"]
```

### docker-compose.yml (Production)
```yaml
version: '3.8'

services:
  frontend:
    build: ./frontend
    ports:
      - "80:80"
    environment:
      - NODE_ENV=production
    depends_on:
      - backend

  backend:
    build: ./backend
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
      - DATABASE_URL=postgresql://postgres:password@db:5432/myapp
    env_file:
      - backend/.env.production
    depends_on:
      - db

  db:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=myapp
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=password
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 5. **CI/CD Pipeline (.github/workflows/ci.yml)**

```yaml
name: CI Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: myapp_test
          POSTGRES_PASSWORD: test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: |
          cd frontend && npm ci
          cd ../backend && npm ci
          npm ci
      
      - name: Run unit tests (Frontend)
        run: cd frontend && npm run test -- --no-watch --code-coverage
      
      - name: Run unit tests (Backend)
        run: cd backend && NODE_ENV=test npm test
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/myapp_test
      
      - name: Build
        run: npm run build
      
      - name: Install Playwright
        run: npx playwright install --with-deps
      
      - name: Run E2E tests
        run: npm run test:e2e
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/myapp_test
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: |
            frontend/coverage/
            backend/coverage/
            playwright-report/
```

## **Resumen de la Arquitectura**

| Aspecto | Solución |
|---------|----------|
| **Entornos** | development, test, staging, production |
| **Frontend Tests** | Jasmine/Karma (unit) |
| **Backend Tests** | Mocha + Chai (unit) |
| **E2E Tests** | Playwright |
| **Containerización** | Docker + docker-compose |
| **CI/CD** | GitHub Actions |
| **Databases** | Separadas por entorno |
| **Secrets** | .env files + CI secrets |

Esta es la arquitectura **estándar de la industria** que verás en empresas como Google, Netflix, Airbnb, etc.

¡Excelente pregunta de seguridad! 🔒 Te explico **exactamente dónde y cómo** se protege esto:

## 🎯 La protección ocurre en **3 capas**

### **1. En el código del backend (primera línea de defensa)**

```javascript
// backend/src/server.js o app.js

const express = require('express');
const app = express();

// ✅ PROTECCIÓN PRINCIPAL: Cargar rutas de testing SOLO en entorno test
if (process.env.NODE_ENV === 'test' || process.env.NODE_ENV === 'development') {
  const testRoutes = require('./routes/test.routes');
  app.use('/api/test', testRoutes);
  console.log('⚠️  Test routes ENABLED (test/dev environment)');
}

// Rutas normales siempre disponibles
app.use('/api/auth', require('./routes/auth.routes'));
app.use('/api/users', require('./routes/user.routes'));

// En producción: /api/test/* simplemente NO EXISTE (404)
```

### **2. En las rutas de testing (segunda línea de defensa)**

```javascript
// backend/src/routes/test.routes.js

// ✅ PROTECCIÓN SECUNDARIA: Verificación extra al inicio del archivo
if (process.env.NODE_ENV === 'production') {
  throw new Error('❌ Test routes are NOT available in production');
}

const express = require('express');
const router = express.Router();

// Endpoints "agresivos" sin restricciones
router.delete('/cleanup-user/:email', async (req, res) => {
  await User.deleteOne({ email: req.params.email });
  res.json({ message: 'User deleted' });
});

router.post('/reset-db', async (req, res) => {
  await resetTestDatabase();
  res.json({ message: 'Database reset' });
});

router.post('/seed-users', async (req, res) => {
  await seedTestData();
  res.json({ message: 'Data seeded' });
});

module.exports = router;
```

### **3. En las variables de entorno (tercera línea)**

```bash
# .env.development (tu laptop)
NODE_ENV=development
DATABASE_URL=postgresql://localhost/myapp_dev

# .env.test (CI/CD y local)
NODE_ENV=test
DATABASE_URL=postgresql://localhost/myapp_test

# .env.production (servidor real) ← ¡AQUÍ ESTÁ LA CLAVE!
NODE_ENV=production  # ← Esto desactiva las rutas de testing
DATABASE_URL=postgresql://prod-server/myapp_prod
```

## 🔐 ¿Cómo funciona en cada entorno?

### **Desarrollo (tu laptop)**
```bash
npm run dev
# Levanta con: NODE_ENV=development
# ✅ Rutas de testing disponibles: http://localhost:3000/api/test/*
```

### **Testing (CI/CD)**
```bash
npm run test:e2e
# Levanta con: NODE_ENV=test
# ✅ Rutas de testing disponibles: http://localhost:3001/api/test/*
```

### **Producción (servidor real)**
```bash
npm start
# Levanta con: NODE_ENV=production (desde .env.production)
# ❌ Rutas de testing NO EXISTEN (nunca se cargan)
# Si alguien intenta: https://myapp.com/api/test/reset-db
# Respuesta: 404 Not Found (porque esa ruta no fue registrada)
```

## 📋 Verificación práctica

Puedes probarlo tú mismo:

```javascript
// backend/src/server.js
app.use((req, res, next) => {
  console.log(`📍 ${req.method} ${req.path} [ENV: ${process.env.NODE_ENV}]`);
  next();
});

// En desarrollo verás:
// 📍 DELETE /api/test/cleanup-user/test@test.com [ENV: development] ✅

// En producción:
// 📍 DELETE /api/test/cleanup-user/test@test.com [ENV: production] → 404
```

## 🛡️ Protección adicional recomendada

```javascript
// backend/src/middleware/test-only.middleware.js

function testOnlyMiddleware(req, res, next) {
  if (process.env.NODE_ENV === 'production') {
    return res.status(403).json({
      error: 'Test endpoints are not available in production'
    });
  }
  next();
}

// Usar en las rutas de testing
router.use(testOnlyMiddleware);
router.delete('/cleanup-user/:email', deleteUser);
```

## 🚀 Configuración en deployment (Render/Vercel/Railway)

```yaml
# render.yaml (ejemplo)
services:
  - type: web
    name: myapp-backend
    env: node
    envVars:
      - key: NODE_ENV
        value: production  # ← Configurado en el servicio
      - key: DATABASE_URL
        fromDatabase:
          name: myapp-db
          property: connectionString
```

## 📊 Resumen visual

```
┌─────────────────────────────────────────────────────┐
│ HACKER intenta: DELETE /api/test/reset-db           │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
          ┌──────────────────────────┐
          │ Servidor en producción   │
          │ NODE_ENV=production      │
          └──────────────────────────┘
                         │
                         ▼
          ┌──────────────────────────┐
          │ server.js verifica:      │
          │ if (NODE_ENV === 'test') │
          │   NO CARGA test.routes   │
          └──────────────────────────┘
                         │
                         ▼
              ❌ 404 Not Found
         (La ruta nunca existió)
```

## ✅ Checklist de seguridad

Antes de deployar a producción, verifica:

```bash
# 1. Variable de entorno configurada
echo $NODE_ENV  # Debe ser "production"

# 2. Rutas de testing NO disponibles
curl https://tu-app.com/api/test/reset-db
# Debe responder: 404 Not Found

# 3. Rutas normales SÍ funcionan
curl https://tu-app.com/api/auth/login
# Debe responder: 200 OK (o lo que corresponda)
```

## 🎓 Para tu aprendizaje

Cuando practiques localmente:

```bash
# Probar modo desarrollo
NODE_ENV=development node server.js
# Visitar: http://localhost:3000/api/test/reset-db ✅ Funciona

# Probar modo producción
NODE_ENV=production node server.js
# Visitar: http://localhost:3000/api/test/reset-db ❌ 404
```

**Respuesta directa**: La protección está en el `if (process.env.NODE_ENV === 'test')` del archivo `server.js`. Si un hacker intenta acceder a `/api/test/*` en producción, obtiene un 404 porque **esas rutas nunca fueron registradas** en Express cuando `NODE_ENV=production`.