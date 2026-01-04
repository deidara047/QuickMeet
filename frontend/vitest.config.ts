import { defineConfig } from 'vitest/config';
import tsconfigPaths from 'vite-tsconfig-paths';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [
    angular(),
    tsconfigPaths()
  ],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test.ts'],
    include: ['src/**/*.spec.ts'],
    exclude: [
      'node_modules',
      'dist',
      '.idea',
      '.git',
      '.cache',
      'e2e/**' // Excluir tests E2E (Playwright los maneja)
    ],
    restoreMocks: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov', 'text-summary'],
      exclude: [
        'node_modules/',
        'src/test.ts',
        '**/*.spec.ts',
        'src/main.ts',
        'src/polyfills.ts',
        'src/environments/**',
        '**/*.d.ts',
        '**/*.config.ts'
      ],
      lines: 50,
      functions: 50,
      branches: 50,
      statements: 50
    }
  }
});
