// TypeScript type definitions — mirror backend DTOs for type safety.

export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  displayName: string;
  id: string;
}

export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  assignedToId: string | null;
  assignedToName: string | null;
  createdById: string;
  createdByName: string | null;
  createdAt: string;
  updatedAt: string;
  comments: Comment[];
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  assignedToId?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  assignedToId?: string;
}

export interface Comment {
  id: string;
  taskId: string;
  authorId: string;
  authorName: string;
  body: string;
  editedAt: string | null;
  createdAt: string;
}

export interface CreateCommentRequest {
  body: string;
}

export interface UpdateCommentRequest {
  body: string;
}

export interface TaskListFilter {
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToId?: string;
}
