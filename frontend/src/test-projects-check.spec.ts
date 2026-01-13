// test-profile-simple.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProfileService } from './app/core/services/profile.service';

describe('ProfileService - Smoke Test with Reflection Debug', () => {
  
  beforeEach(() => {
    console.log('🧹 beforeEach: Configuring TestBed');
    
    // Debug: Check if reflect-metadata is available
    if (typeof Reflect !== 'undefined' && typeof Reflect.getMetadata === 'function') {
      console.log('✅ Reflect.getMetadata available');
      const metadata = Reflect.getMetadata('design:paramtypes', ProfileService);
      console.log('ProfileService constructor paramtypes:', metadata);
    } else {
      console.log('❌ Reflect.getMetadata NOT available - reflect-metadata may not be loaded');
    }
    
    TestBed.configureTestingModule({
      providers: [
        ProfileService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
  });

  afterEach(() => {
    console.log('🧹 afterEach: Resetting TestBed');
    TestBed.resetTestingModule();
  });

  it('Test 1: Should create ProfileService', () => {
    console.log('🔴 Test 1 START');
    const service = TestBed.inject(ProfileService);
    console.log('✅ Test 1: ProfileService created');
    expect(service).toBeDefined();
    expect(service).toBeInstanceOf(ProfileService);
  });

  it('Test 2: Should create ProfileService again (after reset)', () => {
    console.log('🔴 Test 2 START');
    const service = TestBed.inject(ProfileService);
    console.log('✅ Test 2: ProfileService created again');
    expect(service).toBeDefined();
  });

  it('Test 3: Should create ProfileService third time', () => {
    console.log('🔴 Test 3 START');
    const service = TestBed.inject(ProfileService);
    console.log('✅ Test 3: ProfileService created third time');
    expect(service).toBeDefined();
  });
});