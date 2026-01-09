import { Injectable } from '@angular/core';

/**
 * Abstract storage service for implementing Dependency Inversion principle.
 * Allows mocking in tests and switching implementations (localStorage, sessionStorage, IndexedDB)
 */
export abstract class StorageService {
  abstract getItem(key: string): string | null;
  abstract setItem(key: string, value: string): void;
  abstract removeItem(key: string): void;
  abstract clear(): void;
}

/**
 * LocalStorage implementation of StorageService
 * Provides persistent storage in the browser
 */
@Injectable({
  providedIn: 'root'
})
export class LocalStorageService implements StorageService {
  getItem(key: string): string | null {
    return localStorage.getItem(key);
  }

  setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }
}
