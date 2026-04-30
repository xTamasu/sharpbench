// API functions for auth, tasks, and comments. Used by React Query hooks.
import apiClient from './apiClient';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  TaskResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  CommentResponse,
  CreateCommentRequest,
  UpdateCommentRequest,
  TaskItemStatus,
  TaskPriority,
} from '../types';

// Auth
export const login = async (data: LoginRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/login', data);
  return response.data;
};

export const register = async (data: RegisterRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/register', data);
  return response.data;
};

// Tasks
export const getTasks = async (params?: {
  status?: TaskItemStatus;
  priority?: TaskPriority;
  assignedToId?: string;
}): Promise<TaskResponse[]> => {
  const response = await apiClient.get<TaskResponse[]>('/tasks', { params });
  return response.data;
};

export const getTask = async (id: string): Promise<TaskResponse> => {
  const response = await apiClient.get<TaskResponse>(`/tasks/${id}`);
  return response.data;
};

export const createTask = async (data: CreateTaskRequest): Promise<TaskResponse> => {
  const response = await apiClient.post<TaskResponse>('/tasks', data);
  return response.data;
};

export const updateTask = async (id: string, data: UpdateTaskRequest): Promise<TaskResponse> => {
  const response = await apiClient.put<TaskResponse>(`/tasks/${id}`, data);
  return response.data;
};

export const deleteTask = async (id: string): Promise<void> => {
  await apiClient.delete(`/tasks/${id}`);
};

// Comments
export const createComment = async (taskId: string, data: CreateCommentRequest): Promise<CommentResponse> => {
  const response = await apiClient.post<CommentResponse>(`/tasks/${taskId}/comments`, data);
  return response.data;
};

export const updateComment = async (
  taskId: string,
  commentId: string,
  data: UpdateCommentRequest
): Promise<CommentResponse> => {
  const response = await apiClient.put<CommentResponse>(`/tasks/${taskId}/comments/${commentId}`, data);
  return response.data;
};

export const deleteComment = async (taskId: string, commentId: string): Promise<void> => {
  await apiClient.delete(`/tasks/${taskId}/comments/${commentId}`);
};
