// TypeScript type definitions for the Task Manager application.

export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  id: string;
  email: string;
  displayName: string;
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export type Priority = 'Low' | 'Medium' | 'High';

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: Priority;
  dueDate: string | null;
  assignedTo: User | null;
  createdBy: User;
  createdAt: string;
  updatedAt: string;
}

export interface TaskDetail extends Task {
  comments: Comment[];
}

export interface Comment {
  id: string;
  taskId: string;
  author: User;
  body: string;
  editedAt: string | null;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
  assignedToId?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: Priority;
  dueDate?: string;
  assignedToId?: string;
}

export interface CreateCommentRequest {
  body: string;
}

export interface UpdateCommentRequest {
  body: string;
}
