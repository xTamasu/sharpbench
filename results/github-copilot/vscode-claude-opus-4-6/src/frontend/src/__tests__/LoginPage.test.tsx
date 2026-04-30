// Tests for LoginPage: renders form, shows error on failed login.
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import LoginPage from '../pages/LoginPage';

// Mock the API module
vi.mock('../api', () => ({
  login: vi.fn(),
}));

import { login } from '../api';

const mockedLogin = vi.mocked(login);

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{ui}</BrowserRouter>
    </QueryClientProvider>
  );
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('renders the login form with email and password fields', () => {
    renderWithProviders(<LoginPage />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('shows validation errors for empty fields', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LoginPage />);

    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByText(/invalid email format/i)).toBeInTheDocument();
    });
  });

  it('shows error message on failed login', async () => {
    const user = userEvent.setup();
    mockedLogin.mockRejectedValueOnce(new Error('Invalid credentials'));

    renderWithProviders(<LoginPage />);

    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'wrongpassword');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(/invalid email or password/i);
    });
  });

  it('stores token and navigates on successful login', async () => {
    const user = userEvent.setup();
    mockedLogin.mockResolvedValueOnce({
      token: 'test-token',
      email: 'test@example.com',
      displayName: 'Test User',
      userId: '123',
    });

    renderWithProviders(<LoginPage />);

    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBe('test-token');
    });
  });
});
