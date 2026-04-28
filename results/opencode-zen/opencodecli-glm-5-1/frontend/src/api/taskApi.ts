// API functions for task-related endpoints
import apiClient from './apiClient';
import type { TaskResponse, TaskDetailResponse, CreateTaskRequest, UpdateTaskRequest } from '../types';

export const fetchTasks = async (params?: Record<string, string>): Promise<TaskResponse[]> => {
  const { data } = await apiClient.get<TaskResponse[]>('/tasks', { params });
  return data;
};

export const fetchTaskById = async (id: string): Promise<TaskDetailResponse> => {
  const { data } = await apiClient.get<TaskDetailResponse>(`/tasks/${id}`);
  return data;
};

export const createTask = async (task: CreateTaskRequest): Promise<TaskResponse> => {
  const { data } = await apiClient.post<TaskResponse>('/tasks', task);
  return data;
};

export const updateTask = async (id: string, task: UpdateTaskRequest): Promise<TaskResponse> => {
  const { data } = await apiClient.put<TaskResponse>(`/tasks/${id}`, task);
  return data;
};

export const deleteTask = async (id: string): Promise<void> => {
  await apiClient.delete(`/tasks/${id}`);
};