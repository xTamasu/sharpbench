// Test mocks — provides mock implementations for apiClient to avoid real HTTP calls in tests.

import { vi } from 'vitest';

export const mockApiClient = {
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  delete: vi.fn(),
  interceptors: {
    request: {
      use: vi.fn(),
    },
    response: {
      use: vi.fn(),
    },
  },
};

vi.mock('@/api/apiClient', () => ({
  default: mockApiClient,
}));
