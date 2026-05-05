/**
 * Type definitions for the Task Manager frontend.
 */

export interface User {
  id: string
  email: string
  displayName: string
}

export type TaskStatus = 'Todo' | 'InProgress' | 'Done'
export type Priority = 'Low' | 'Medium' | 'High'

export interface Task {
  id: string
  title: string
  description: string | null
  status: TaskStatus
  priority: Priority
  dueDate: string | null
  assignedToId: string | null
  assignedToName: string | null
  createdById: string
  createdByName: string
  createdAt: string
  updatedAt: string
}

export interface TaskDetail extends Task {
  comments: Comment[]
}

export interface Comment {
  id: string
  taskId: string
  authorId: string
  authorName: string
  body: string
  editedAt: string | null
  createdAt: string
}

export interface AuthResponse {
  token: string
  userId: string
  email: string
  displayName: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  displayName: string
}

export interface CreateTaskRequest {
  title: string
  description: string | null
  status: TaskStatus
  priority: Priority
  dueDate: string | null
  assignedToId: string | null
}

export interface UpdateTaskRequest {
  title: string
  description: string | null
  status: TaskStatus
  priority: Priority
  dueDate: string | null
  assignedToId: string | null
}

export interface CreateCommentRequest {
  body: string
}

export interface UpdateCommentRequest {
  body: string
}

export interface TaskFilter {
  status?: TaskStatus
  priority?: Priority
  assignedToId?: string
}
