// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.

export const environment = {
  production: false,
  appName: 'QuickMeet',
  apiUrl: 'http://localhost:5173/api',
  apiTimeout: 30000,
  logLevel: 'debug',
  enableAnalytics: false,
  enableDebugPanel: true,
  sessionTimeoutMinutes: 60,
  googleAnalyticsId: '',
  sentryDsn: '',
  enableBetaFeatures: true,
};
