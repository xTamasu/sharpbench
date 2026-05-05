// API functions for task CRUD operations.

import apiClient from './apiClient';
import type {
  Task,
  TaskDetail,
  CreateTaskRequest,
  UpdateTaskRequest,
  TaskStatus,
  Priority,
} from '../types';

interface TaskFilters {
  status?: TaskStatus;
  priority?: Priority;
  assignedToId?: string;
}

export const tasksApi = {
  getTasks: async (filters?: TaskFilters): Promise<Task[]> => {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.priority) params.append('priority', filters.priority);
    if (filters?.assignedToId) params.append('assignedToId', filters.assignedToId);
    
    const response = await apiClient.get<Task[]>('/tasks', { params });
    return response.data;
  },

  getTask: async (id: string): Promise<TaskDetail> => {
    const response = await apiClient.get<TaskDetail>(`/tasks/${id}`);
    return response.data;
  },

  createTask: async (data: CreateTaskRequest): Promise<Task> => {
    const response = await apiClient.post<Task>('/tasks', data);
    return response.data;
  },

  updateTask: async (id: string, data: UpdateTaskRequest): Promise<Task> => {
    const response = await apiClient.put<Task>(`/tasks/${id}`, data);
    return response.data;
  },

  deleteTask: async (id: string): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },
};
