// Authentication types
export interface LoginResponse {
  accessToken: string
  user: UserDto
}

export interface UserDto {
  id: string
  email: string
  displayName: string
}

export interface RegisterRequest {
  email: string
  password: string
  displayName: string
}

export interface LoginRequest {
  email: string
  password: string
}

// Task types
export interface TaskDto {
  id: string
  title: string
  description?: string
  status: number
  priority: number
  dueDate?: string
  assignedToId?: string
  assignedToDisplayName?: string
  createdById: string
  createdByDisplayName: string
  createdAt: string
  updatedAt: string
}

export interface CreateTaskRequest {
  title: string
  description?: string
  priority: number
  dueDate?: string
  assignedToId?: string
}

export interface UpdateTaskRequest {
  title?: string
  description?: string
  status?: number
  priority?: number
  dueDate?: string
  assignedToId?: string
}

// Comment types
export interface CommentDto {
  id: string
  taskId: string
  authorId: string
  authorDisplayName: string
  body: string
  editedAt?: string
  createdAt: string
}

export interface CreateCommentRequest {
  body: string
}

export interface UpdateCommentRequest {
  body: string
}

// Enum values
export const TaskStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
} as const

export const TaskPriority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const

export type TaskStatusType = typeof TaskStatus[keyof typeof TaskStatus]
export type TaskPriorityType = typeof TaskPriority[keyof typeof TaskPriority]
