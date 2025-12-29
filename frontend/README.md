# Frontend - QuickMeet Angular

Stack moderno con Angular 21, PrimeNG, Tailwind CSS y Material Icons.

## Stack Tecnológico

- **Angular 21** - Framework web moderno
- **TypeScript 5.9** - Lenguaje tipado
- **PrimeNG** - Componentes UI profesionales
- **@primeuix/themes** - Sistema de temas moderno
- **Tailwind CSS** - Utilidades CSS
- **Material Icons** - Iconografía de Google
- **Vitest** - Testing rápido
- **RxJS 7.8** - Programación reactiva

## Estructura de Carpetas

```
src/
├── app/
│   ├── core/
│   │   ├── services/        # Servicios reutilizables
│   │   ├── interceptors/    # Interceptores HTTP
│   │   ├── guards/          # Guards de rutas
│   │   └── constants/       # Constantes de la app
│   ├── shared/
│   │   ├── components/      # Componentes reutilizables
│   │   ├── directives/      # Directivas personalizadas
│   │   └── pipes/           # Pipes personalizados
│   ├── features/
│   │   ├── auth/           # Módulo autenticación
│   │   ├── appointments/   # Módulo citas
│   │   └── dashboard/      # Módulo dashboard
│   ├── layouts/
│   │   ├── main-layout/    # Layout principal
│   │   └── auth-layout/    # Layout autenticación
│   ├── app.routes.ts       # Rutas de la app
│   ├── app.config.ts       # Configuración global
│   └── app.ts              # Componente raíz
├── styles.css              # Estilos globales
└── main.ts                 # Entrada de la app
```

## Instalación

```bash
# Instalar dependencias
npm install

# Servir la aplicación
npm start
```

## Desarrollo

```bash
# Servidor de desarrollo
npm start

# Compilar para producción
npm run build

# Ejecutar tests
npm test

# Linter
npm run lint
```

## Componentes PrimeNG Disponibles

- Botones (`pButton`)
- Inputs (`pInputText`)
- Selects (`p-dropdown`)
- Tablas (`p-table`)
- Diálogos/Modales (`p-dialog`)
- Formularios (`p-form`)
- Tarjetas (`p-card`)
- Navegación (`p-menu`, `p-tabView`)
- Toast/Notificaciones (`p-toast`)
- Y muchos más...

Ver: [PRIMENG_SETUP.md](./PRIMENG_SETUP.md)

## Iconos

### PrimeIcons (Recomendado)
```html
<i class="pi pi-home"></i>
<i class="pi pi-envelope"></i>
<i class="pi pi-cog"></i>
```

### Material Icons
```html
<span class="material-icons">home</span>
<span class="material-icons-outlined">settings</span>
```

Ver: [PRIMENG_SETUP.md](./PRIMENG_SETUP.md)

## Tailwind CSS

Configurado con:
- Colores personalizados
- Plugins: forms y typography
- Compatible con PrimeNG

Personalizar en: `tailwind.config.js`

## Temas PrimeNG

Cambiar tema en `styles.css`:
- Lara Light Blue (actual)
- Lara Light/Dark con varios colores
- Y muchos más...

## Testing

```bash
# Ejecutar tests
npm test

# Con coverage
npm test -- --coverage
```

## Build Producción

```bash
npm run build

# Output: dist/quickmeet-frontend/
```

## Variables de Entorno

Crear `.env` basado en `.env.example`:

```
NG_APP_API_URL=http://localhost:5000/api
NG_APP_API_TIMEOUT=30000
NG_APP_ENVIRONMENT=development
NG_APP_ENABLE_LOGGING=true
```

## Documentación

- [PrimeNG Docs](https://primeng.org/)
- [PrimeUX Docs](https://primeuix.org/)
- [Material Icons](https://fonts.google.com/icons)
- [Tailwind CSS](https://tailwindcss.com/)
- [Angular Docs](https://angular.dev/)

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
