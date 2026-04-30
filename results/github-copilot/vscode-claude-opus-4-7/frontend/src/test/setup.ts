// setup.ts — Vitest global setup. Adds @testing-library/jest-dom matchers
// and ensures localStorage is fresh between tests.
import '@testing-library/jest-dom/vitest';
import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

afterEach(() => {
  cleanup();
  localStorage.clear();
});
