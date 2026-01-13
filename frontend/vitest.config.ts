import { defineConfig } from 'vitest/config';
import tsconfigPaths from 'vite-tsconfig-paths';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [
    angular(),
    tsconfigPaths()
  ],
  resolve: {
    alias: {
      '@env': new URL('./src/environments', import.meta.url).pathname
    }
  },
  test: {
    // ✅ USAR PROJECTS para garantizar setupFiles se ejecute correctamente
    projects: [
      {
        name: 'unit-tests',
        test: {
          globals: true,
          environment: 'jsdom',
          
          // ✅ setupFiles dentro del project
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
          
          restoreMocks: true,
          coverage: {
            provider: 'v8',
            reporter: ['text', 'html', 'lcov', 'text-summary'],
            exclude: [
              'node_modules/',
              'src/test-setup.ts',
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
      }
    ]
  }
});
