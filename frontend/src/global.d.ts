/**
 * Global TypeScript declarations for test environment
 * 
 * Permite que TypeScript entienda las variables globales
 * que se usan en el setup de tests.
 */

declare global {
  /**
   * Flag para verificar si TestBed ha sido inicializado
   * Usado en src/vitest.setup.ts para inicializar una sola vez
   */
  var __ANGULAR_TESTBED_INITIALIZED__: boolean | undefined;
}

export {};
