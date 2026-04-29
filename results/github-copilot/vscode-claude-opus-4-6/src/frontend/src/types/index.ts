// TypeScript types matching backend DTOs.

export enum TaskItemStatus {
  Todo = 0,
  InProgress = 1,
  Done = 2,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
}

export interface AuthResponse {
  token: string;
  email: string;
  displayName: string;
  userId: string;
}

export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate: string | null;
  assignedToId: string | null;
  assignedToName: string | null;
  createdById: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
  comments: CommentResponse[];
}

export interface CommentResponse {
  id: string;
  taskId: string;
  authorId: string;
  authorName: string;
  body: string;
  editedAt: string | null;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  assignedToId?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  assignedToId?: string;
}

export interface CreateCommentRequest {
  body: string;
}

export interface UpdateCommentRequest {
  body: string;
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
