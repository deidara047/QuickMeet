# Playwright E2E Tests - QuickMeet Frontend

## Setup Inicial

Playwright ya está instalado. Configuración en:
- `playwright.config.ts` - Configuración de Playwright
- `e2e/` - Carpeta con tests e2e

## Ejecutar Tests

### 1. **Ejecutar todos los tests**
```bash
npm run e2e
```

### 2. **Ejecutar en modo UI (recomendado para desarrollo)**
```bash
npm run e2e:ui
```
Este modo abre una interfaz visual donde puedes:
- Ver los tests mientras se ejecutan
- Pausar/reanudar ejecución
- Inspeccionar elementos
- Hacer debug paso a paso

### 3. **Ejecutar en modo debug**
```bash
npm run e2e:debug
```
Abre el Playwright Inspector para debug avanzado

### 4. **Ejecutar un test específico**
```bash
npx playwright test e2e/example.spec.ts
```

## Estructura de Tests

```
e2e/
├── example.spec.ts          # Test básico (tu primer test)
└── [próximos tests aquí]
```

## Configuración

### Baseado en:
- **Base URL**: `http://localhost:4200` (Angular dev server)
- **Browsers**: Chromium, Firefox, Safari
- **Reporters**: HTML + Lista en consola
- **Auto-start**: El servidor Angular se inicia automáticamente

## Próximas Pruebas E2E

Ideas para la próxima sesión:
1. ✅ Load homepage
2. Navegar a login
3. Hacer login exitoso
4. Hacer login fallido
5. Navegar a dashboard
6. Crear cita
7. Cancelar cita
8. Logout

## Troubleshooting

### "Port 4200 already in use"
```bash
# Kill el proceso Angular anterior
lsof -ti:4200 | xargs kill -9
```

### Tests no encuentran elementos
1. Verificar que Angular está corriendo
2. Usar `--ui` mode para ver qué está pasando
3. Agregar `await page.waitForLoadState('networkidle');`

## Archivo Clave: playwright.config.ts

```typescript
// Automáticamente inicia 'npm run start'
webServer: {
  command: 'npm run start',
  url: 'http://localhost:4200',
  reuseExistingServer: !process.env.CI,
}
```

Esto significa que puedes correr `npm run e2e` sin iniciar manualmente el servidor.
