import { useMutation } from '@tanstack/react-query'
import { authApi } from '../api/services'

/**
 * Hook for user login mutation.
 * On success, stores the JWT token and user info in localStorage.
 */
export function useLogin() {
  return useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify({
        id: data.userId,
        email: data.email,
        displayName: data.displayName,
      }))
    },
  })
}

/**
 * Hook for user registration mutation.
 * On success, stores the JWT token and user info in localStorage.
 */
export function useRegister() {
  return useMutation({
    mutationFn: authApi.register,
    onSuccess: (data) => {
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify({
        id: data.userId,
        email: data.email,
        displayName: data.displayName,
      }))
    },
  })
}

/**
 * Gets the current authenticated user from localStorage.
 */
export function getCurrentUser() {
  const userJson = localStorage.getItem('user')
  if (!userJson) return null
  try {
    return JSON.parse(userJson) as { id: string; email: string; displayName: string }
  } catch {
    return null
  }
}

/**
 * Checks if the user is authenticated.
 */
export function isAuthenticated(): boolean {
  return !!localStorage.getItem('token')
}

/**
 * Logs out the current user by clearing localStorage.
 */
export function logout(): void {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
}
