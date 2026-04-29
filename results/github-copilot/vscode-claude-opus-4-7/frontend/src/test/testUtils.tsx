// testUtils.tsx — shared helpers to render components with the providers
// they expect (React Query + Router + AuthContext).
import { ReactElement, ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, type RenderOptions } from '@testing-library/react';
import { AuthProvider } from '../auth/AuthContext';
import { TOKEN_STORAGE_KEY } from '../api/apiClient';
import type { User } from '../types';

interface WithProvidersOptions extends RenderOptions {
  initialEntries?: string[];
  authenticatedAs?: User | null;
}

export function renderWithProviders(ui: ReactElement, options: WithProvidersOptions = {}) {
  const { initialEntries = ['/'], authenticatedAs = null, ...rest } = options;
  if (authenticatedAs) {
    localStorage.setItem(TOKEN_STORAGE_KEY, 'fake-test-token');
    localStorage.setItem('tm_user', JSON.stringify(authenticatedAs));
  }

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });

  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={qc}>
        <AuthProvider>
          <MemoryRouter initialEntries={initialEntries}>{children}</MemoryRouter>
        </AuthProvider>
      </QueryClientProvider>
    );
  }

  return render(ui, { wrapper: Wrapper, ...rest });
}
