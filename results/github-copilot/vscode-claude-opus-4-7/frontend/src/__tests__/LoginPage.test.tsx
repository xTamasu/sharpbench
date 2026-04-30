// LoginPage.test.tsx
// Verifies the form renders and that a failed login surfaces an error message.
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import LoginPage from '../pages/LoginPage';
import { renderWithProviders } from '../test/testUtils';

vi.mock('../api/auth', () => ({
  login: vi.fn(),
  register: vi.fn(),
}));

import { login } from '../api/auth';
const loginMock = vi.mocked(login);

describe('LoginPage', () => {
  beforeEach(() => loginMock.mockReset());

  it('renders the login form', () => {
    renderWithProviders(<LoginPage />);
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('shows an error when login fails', async () => {
    loginMock.mockRejectedValueOnce(new Error('Invalid email or password.'));
    const user = userEvent.setup();

    renderWithProviders(<LoginPage />);
    await user.type(screen.getByLabelText(/email/i), 'bad@example.com');
    await user.type(screen.getByLabelText(/password/i), 'wrongpw');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() =>
      expect(screen.getByRole('alert')).toHaveTextContent('Invalid email or password.'),
    );
    expect(loginMock).toHaveBeenCalledWith({ email: 'bad@example.com', password: 'wrongpw' });
  });
});
