// apiClient.ts
// Centralized Axios instance. Attaches the JWT from localStorage to every request
// and converts non-2xx responses into Error objects whose message comes from the
// backend's ProblemDetails payload.
import axios, { AxiosError } from 'axios';
import type { ProblemDetails } from '../types';

export const TOKEN_STORAGE_KEY = 'tm_token';

export const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor: attach Bearer token if present.
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: surface backend error details and auto-logout on 401.
apiClient.interceptors.response.use(
  (r) => r,
  (error: AxiosError<ProblemDetails>) => {
    const data = error.response?.data;
    const message = data?.detail ?? data?.title ?? error.message ?? 'Request failed';

    // 401 from the API means the token is missing/expired -> wipe it so the
    // next ProtectedRoute redirect bounces the user back to /login.
    if (error.response?.status === 401) {
      localStorage.removeItem(TOKEN_STORAGE_KEY);
    }

    return Promise.reject(new Error(message));
  },
);
