// Test setup — configures Vitest globals and jest-dom matchers.

import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; },
  };
})();
Object.defineProperty(window, 'localStorage', { value: localStorageMock });

// Clean up after each test
afterEach(() => {
  cleanup();
  localStorageMock.clear();
});

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});
