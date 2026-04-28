// TypeScript type definitions for all API request and response shapes
export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  displayName: string;
  token: string;
}

export type TaskStatusEnum = 'Todo' | 'InProgress' | 'Done';
export type TaskPriorityEnum = 'Low' | 'Medium' | 'High';

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  status: TaskStatusEnum;
  priority: TaskPriorityEnum;
  dueDate: string | null;
  assignedToId: string | null;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string | null;
  status?: TaskStatusEnum;
  priority?: TaskPriorityEnum;
  dueDate?: string | null;
  assignedToId?: string | null;
}

export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priority: string;
  dueDate: string | null;
  createdById: string;
  createdByName: string;
  assignedToId: string | null;
  assignedToName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskDetailResponse extends TaskResponse {
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

export interface CreateCommentRequest {
  body: string;
}

export interface UpdateCommentRequest {
  body: string;
}