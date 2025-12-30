export const environment = {
  production: true,
  appName: process.env['APP_NAME'] || 'QuickMeet',
  apiUrl: process.env['API_BASE_URL'] || '/api',
  apiTimeout: parseInt(process.env['API_TIMEOUT'] || '30000', 10),
  logLevel: process.env['LOG_LEVEL'] || 'warn',
  enableAnalytics: process.env['ENABLE_ANALYTICS'] === 'true' || true,
  enableDebugPanel: process.env['ENABLE_DEBUG_PANEL'] === 'true' || false,
  sessionTimeoutMinutes: parseInt(process.env['SESSION_TIMEOUT_MINUTES'] || '60', 10),
  googleAnalyticsId: process.env['GOOGLE_ANALYTICS_ID'] || '',
  sentryDsn: process.env['SENTRY_DSN'] || '',
  enableBetaFeatures: process.env['ENABLE_BETA_FEATURES'] === 'true' || false,
};
