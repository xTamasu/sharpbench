// API functions for auth-related endpoints
import apiClient from './apiClient';
import type { AuthResponse, RegisterRequest, LoginRequest } from '../types';

export const register = async (request: RegisterRequest): Promise<AuthResponse> => {
  const { data } = await apiClient.post<AuthResponse>('/auth/register', request);
  return data;
};

export const login = async (request: LoginRequest): Promise<AuthResponse> => {
  const { data } = await apiClient.post<AuthResponse>('/auth/login', request);
  return data;
};