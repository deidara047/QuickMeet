// ============================================================================
// FILE: src/test-setup.ts (VERSIÓN FINAL - CORREGIDA)
// ============================================================================

/**
 * Vitest Test Setup using Projects
 * 
 * This file is executed via vitest.config.ts projects[].test.setupFiles
 * It initializes Angular TestBed ONCE and properly resets it between tests.
 */

// ✅ CRÍTICO 0: Import reflect-metadata PRIMERO (necesario para Angular DI metadata)
import 'reflect-metadata';

// ✅ CRÍTICO 1: Import compiler SEGUNDO (antes de zone.js)
import '@angular/compiler';

// ✅ CRÍTICO 2: Import zone.js TERCERO
import 'zone.js';
import 'zone.js/testing';

import { getTestBed } from '@angular/core/testing';
import { BrowserTestingModule } from '@angular/platform-browser/testing';
import { platformBrowserTesting } from '@angular/platform-browser/testing';

declare global {
  var __ANGULAR_TESTBED_INITIALIZED__: boolean | undefined;
}

console.log('🚀 Loading test-setup.ts via projects setupFiles');

// Initialize TestBed ONCE
if (!globalThis.__ANGULAR_TESTBED_INITIALIZED__) {
  try {
    console.log('🔧 Initializing Angular TestBed...');
    
    getTestBed().initTestEnvironment(
      BrowserTestingModule,
      platformBrowserTesting(),
      {
        teardown: { destroyAfterEach: true }
      }
    );
    
    globalThis.__ANGULAR_TESTBED_INITIALIZED__ = true;
    console.log('✅ TestBed initialized successfully with auto-teardown');
    
  } catch (error) {
    console.error('❌ Failed to initialize TestBed:', error);
    throw error;
  }
} else {
  console.log('ℹ️  TestBed already initialized (skipping)');
}