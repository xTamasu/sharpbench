// auth.ts — typed wrappers for /api/auth/* endpoints.
import { apiClient } from './apiClient';
import type { AuthResponse } from '../types';

export interface RegisterPayload { email: string; password: string; displayName: string; }
export interface LoginPayload    { email: string; password: string; }

export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/register', payload);
  return data;
}

export async function login(payload: LoginPayload): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/login', payload);
  return data;
}
