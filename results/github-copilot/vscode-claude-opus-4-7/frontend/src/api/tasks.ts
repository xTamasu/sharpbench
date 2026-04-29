// tasks.ts — typed wrappers for /api/tasks/* endpoints.
import { apiClient } from './apiClient';
import type { TaskItem, TaskDetail, TaskPriority, TaskStatus } from '../types';

export interface TaskFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToId?: string;
}

export interface TaskInput {
  title: string;
  description?: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string | null;
  assignedToId?: string | null;
}

export async function listTasks(filters: TaskFilters = {}): Promise<TaskItem[]> {
  const { data } = await apiClient.get<TaskItem[]>('/tasks', { params: filters });
  return data;
}

export async function getTask(id: string): Promise<TaskDetail> {
  const { data } = await apiClient.get<TaskDetail>(`/tasks/${id}`);
  return data;
}

export async function createTask(input: TaskInput): Promise<TaskItem> {
  const { data } = await apiClient.post<TaskItem>('/tasks', input);
  return data;
}

export async function updateTask(id: string, input: TaskInput): Promise<TaskItem> {
  const { data } = await apiClient.put<TaskItem>(`/tasks/${id}`, input);
  return data;
}

export async function deleteTask(id: string): Promise<void> {
  await apiClient.delete(`/tasks/${id}`);
}
