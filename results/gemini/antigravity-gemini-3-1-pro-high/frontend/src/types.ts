export type Priority = 0 | 1 | 2; // Low, Medium, High
export type Status = 0 | 1 | 2; // Todo, InProgress, Done

export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  status: Status;
  priority: Priority;
  dueDate?: string;
  assignedToId?: string;
  createdById: string;
  createdAt: string;
  updatedAt: string;
}

export interface Comment {
  id: string;
  taskId: string;
  authorId: string;
  body: string;
  editedAt?: string;
  createdAt: string;
}

export interface TaskDetail extends Task {
  comments: Comment[];
}
