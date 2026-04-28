import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClientProvider, QueryClient } from '@tanstack/react-query'
import { LoginPage } from '../pages/LoginPage'
import api from '../services/apiClient'

vi.mock('../services/apiClient')

describe('LoginPage', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient()
    localStorage.clear()
  })

  const renderLoginPage = () => {
    return render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <LoginPage />
        </QueryClientProvider>
      </BrowserRouter>
    )
  }

  it('renders login form', () => {
    renderLoginPage()
    expect(screen.getByText('Sign in to your account')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Email address')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Password')).toBeInTheDocument()
  })

  it('shows validation errors on invalid input', async () => {
    renderLoginPage()
    const submitButton = screen.getByText('Sign in')

    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/Invalid email address/)).toBeInTheDocument()
    })
  })

  it('displays error message on failed login', async () => {
    vi.mocked(api.post).mockRejectedValueOnce(new Error('Invalid credentials'))

    renderLoginPage()
    const emailInput = screen.getByPlaceholderText('Email address')
    const passwordInput = screen.getByPlaceholderText('Password')
    const submitButton = screen.getByText('Sign in')

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.change(passwordInput, { target: { value: 'password' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/Invalid email or password/)).toBeInTheDocument()
    })
  })

  it('shows register link', () => {
    renderLoginPage()
    expect(screen.getByText("Don't have an account?")).toBeInTheDocument()
    const registerLink = screen.getByText('Register')
    expect(registerLink).toHaveAttribute('href', '/register')
  })
})
