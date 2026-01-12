import { defineConfig } from 'vitest/config';
import tsconfigPaths from 'vite-tsconfig-paths';
import angular from '@analogjs/vite-plugin-angular';
import { fileURLToPath } from 'node:url';

export default defineConfig({
  plugins: [
    angular(),
    tsconfigPaths() // Este plugin debería leer automáticamente tu tsconfig
  ],
  resolve: {
    alias: {
      '@env': fileURLToPath(new URL('./src/environments', import.meta.url))
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    include: ['src/**/*.spec.ts'],
    exclude: [
      'node_modules',
      'dist',
      '.idea',
      '.git',
      '.cache',
      'e2e/**'
    ],
    restoreMocks: true
  }
});