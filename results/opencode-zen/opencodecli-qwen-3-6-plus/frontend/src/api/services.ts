// API service functions — typed wrappers around apiClient for auth, tasks, and comments.

import apiClient from '@/api/apiClient';
import type {
  AuthResponse,
  Task,
  Comment,
  CreateTaskRequest,
  UpdateTaskRequest,
  CreateCommentRequest,
  UpdateCommentRequest,
  TaskListFilter,
} from '@/types';

// Auth
export const register = async (
  email: string,
  password: string,
  displayName: string,
): Promise<AuthResponse> => {
  const { data } = await apiClient.post<AuthResponse>('/auth/register', {
    email,
    password,
    displayName,
  });
  return data;
};

export const login = async (
  email: string,
  password: string,
): Promise<AuthResponse> => {
  const { data } = await apiClient.post<AuthResponse>('/auth/login', {
    email,
    password,
  });
  return data;
};

// Tasks
export const getTasks = async (filter?: TaskListFilter): Promise<Task[]> => {
  const params: Record<string, string> = {};
  if (filter?.status) params.status = filter.status;
  if (filter?.priority) params.priority = filter.priority;
  if (filter?.assignedToId) params.assignedToId = filter.assignedToId;

  const { data } = await apiClient.get<Task[]>('/tasks', { params });
  return data;
};

export const getTask = async (id: string): Promise<Task> => {
  const { data } = await apiClient.get<Task>(`/tasks/${id}`);
  return data;
};

export const createTask = async (request: CreateTaskRequest): Promise<Task> => {
  const { data } = await apiClient.post<Task>('/tasks', request);
  return data;
};

export const updateTask = async (
  id: string,
  request: UpdateTaskRequest,
): Promise<Task> => {
  const { data } = await apiClient.put<Task>(`/tasks/${id}`, request);
  return data;
};

export const deleteTask = async (id: string): Promise<void> => {
  await apiClient.delete(`/tasks/${id}`);
};

// Comments
export const getComments = async (taskId: string): Promise<Comment[]> => {
  const { data } = await apiClient.get<Comment[]>(`/tasks/${taskId}/comments`);
  return data;
};

export const createComment = async (
  taskId: string,
  request: CreateCommentRequest,
): Promise<Comment> => {
  const { data } = await apiClient.post<Comment>(
    `/tasks/${taskId}/comments`,
    request,
  );
  return data;
};

export const updateComment = async (
  taskId: string,
  commentId: string,
  request: UpdateCommentRequest,
): Promise<Comment> => {
  const { data } = await apiClient.put<Comment>(
    `/tasks/${taskId}/comments/${commentId}`,
    request,
  );
  return data;
};

export const deleteComment = async (
  taskId: string,
  commentId: string,
): Promise<void> => {
  await apiClient.delete(`/tasks/${taskId}/comments/${commentId}`);
};
