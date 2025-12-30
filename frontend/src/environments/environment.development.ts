export const environment = {
  production: false,
  appName: process.env['APP_NAME'] || 'QuickMeet (Dev)',
  apiUrl: process.env['API_BASE_URL'] || 'http://localhost:3000/api',
  apiTimeout: parseInt(process.env['API_TIMEOUT'] || '30000', 10),
  logLevel: process.env['LOG_LEVEL'] || 'debug',
  enableAnalytics: process.env['ENABLE_ANALYTICS'] === 'true' || false,
  enableDebugPanel: process.env['ENABLE_DEBUG_PANEL'] === 'true' || true,
  sessionTimeoutMinutes: parseInt(process.env['SESSION_TIMEOUT_MINUTES'] || '60', 10),
  googleAnalyticsId: process.env['GOOGLE_ANALYTICS_ID'] || '',
  sentryDsn: process.env['SENTRY_DSN'] || '',
  enableBetaFeatures: process.env['ENABLE_BETA_FEATURES'] === 'true' || true,
};
