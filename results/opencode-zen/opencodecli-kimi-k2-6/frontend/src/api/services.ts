import apiClient from './apiClient'
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  Task,
  TaskDetail,
  CreateTaskRequest,
  UpdateTaskRequest,
  Comment,
  CreateCommentRequest,
  UpdateCommentRequest,
  TaskFilter,
} from '../types'

/**
 * Authentication API operations.
 */
export const authApi = {
  login: (data: LoginRequest): Promise<AuthResponse> =>
    apiClient.post('/auth/login', data).then((res) => res.data),

  register: (data: RegisterRequest): Promise<AuthResponse> =>
    apiClient.post('/auth/register', data).then((res) => res.data),
}

/**
 * Task API operations.
 */
export const tasksApi = {
  getAll: (filter?: TaskFilter): Promise<Task[]> =>
    apiClient.get('/tasks', { params: filter }).then((res) => res.data),

  getById: (id: string): Promise<TaskDetail> =>
    apiClient.get(`/tasks/${id}`).then((res) => res.data),

  create: (data: CreateTaskRequest): Promise<Task> =>
    apiClient.post('/tasks', data).then((res) => res.data),

  update: (id: string, data: UpdateTaskRequest): Promise<Task> =>
    apiClient.put(`/tasks/${id}`, data).then((res) => res.data),

  delete: (id: string): Promise<void> =>
    apiClient.delete(`/tasks/${id}`).then((res) => res.data),
}

/**
 * Comment API operations.
 */
export const commentsApi = {
  create: (taskId: string, data: CreateCommentRequest): Promise<Comment> =>
    apiClient.post(`/tasks/${taskId}/comments`, data).then((res) => res.data),

  update: (taskId: string, commentId: string, data: UpdateCommentRequest): Promise<Comment> =>
    apiClient.put(`/tasks/${taskId}/comments/${commentId}`, data).then((res) => res.data),

  delete: (taskId: string, commentId: string): Promise<void> =>
    apiClient.delete(`/tasks/${taskId}/comments/${commentId}`).then((res) => res.data),
}
