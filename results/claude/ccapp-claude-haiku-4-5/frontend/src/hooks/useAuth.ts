import { useMutation } from '@tanstack/react-query'
import api from '../services/apiClient'
import { LoginRequest, RegisterRequest, LoginResponse, UserDto } from '../types'

export const useAuth = () => {
  const user = JSON.parse(localStorage.getItem('user') || 'null') as UserDto | null
  const accessToken = localStorage.getItem('accessToken')

  const loginMutation = useMutation({
    mutationFn: async (request: LoginRequest) => {
      const response = await api.post<LoginResponse>('/auth/login', request)
      localStorage.setItem('accessToken', response.data.accessToken)
      localStorage.setItem('user', JSON.stringify(response.data.user))
      return response.data
    },
  })

  const registerMutation = useMutation({
    mutationFn: async (request: RegisterRequest) => {
      const response = await api.post<LoginResponse>('/auth/register', request)
      localStorage.setItem('accessToken', response.data.accessToken)
      localStorage.setItem('user', JSON.stringify(response.data.user))
      return response.data
    },
  })

  const logout = () => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('user')
    window.location.href = '/login'
  }

  return {
    user,
    isAuthenticated: !!accessToken,
    loginMutation,
    registerMutation,
    logout,
  }
}
