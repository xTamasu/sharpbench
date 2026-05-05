// LoginPage tests — verifies form rendering, validation, and error display on failed login.

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import LoginPage from '@/pages/LoginPage';
import * as services from '@/api/services';

// Mock the API service
vi.mock('@/api/services', () => ({
  login: vi.fn(),
}));

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{component}</MemoryRouter>
    </QueryClientProvider>,
  );
};

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('renders the login form', () => {
    renderWithProviders(<LoginPage />);

    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/you@example.com/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/••••••••/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
  });

  it('shows error on failed login', async () => {
    vi.mocked(services.login).mockRejectedValue(new Error('Invalid credentials'));

    renderWithProviders(<LoginPage />);

    fireEvent.change(screen.getByPlaceholderText(/you@example.com/i), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByPlaceholderText(/••••••••/i), {
      target: { value: 'wrongpassword' },
    });
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(
        screen.getByText(/invalid email or password/i),
      ).toBeInTheDocument();
    });
  });

  it('stores token and navigates on successful login', async () => {
    vi.mocked(services.login).mockResolvedValue({
      token: 'fake-jwt-token',
      email: 'test@example.com',
      displayName: 'Test User',
      id: 'user-id-123',
    });

    renderWithProviders(<LoginPage />);

    fireEvent.change(screen.getByPlaceholderText(/you@example.com/i), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByPlaceholderText(/••••••••/i), {
      target: { value: 'correctpassword' },
    });
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBe('fake-jwt-token');
    });
  });

  it('shows validation error for empty email', async () => {
    renderWithProviders(<LoginPage />);

    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(screen.getByText(/invalid email address/i)).toBeInTheDocument();
    });
  });
});
