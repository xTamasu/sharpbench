import '@testing-library/jest-dom'
import { vi } from 'vitest'

/**
 * Vitest setup file for frontend tests.
 * Configures jest-dom matchers and mocks browser APIs.
 */

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
})

// Mock window.location
Object.defineProperty(window, 'location', {
  value: { href: '' },
  writable: true,
})
