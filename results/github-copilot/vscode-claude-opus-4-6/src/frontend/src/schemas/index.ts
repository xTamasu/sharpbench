// Zod schemas for form validation, mirroring backend constraints.
import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Invalid email format'),
  password: z.string().min(1, 'Password is required'),
});

export const registerSchema = z.object({
  email: z.string().email('Invalid email format'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  displayName: z.string().min(1, 'Display name is required').max(100, 'Display name must not exceed 100 characters'),
});

export const createTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title must not exceed 200 characters'),
  description: z.string().max(5000, 'Description must not exceed 5000 characters').optional().or(z.literal('')),
  status: z.number().min(0).max(2),
  priority: z.number().min(0).max(2),
  dueDate: z.string().optional().or(z.literal('')),
  assignedToId: z.string().optional().or(z.literal('')),
});

export const commentSchema = z.object({
  body: z.string().min(1, 'Comment body is required').max(2000, 'Comment must not exceed 2000 characters'),
});

export type LoginFormData = z.infer<typeof loginSchema>;
export type RegisterFormData = z.infer<typeof registerSchema>;
export type CreateTaskFormData = z.infer<typeof createTaskSchema>;
export type CommentFormData = z.infer<typeof commentSchema>;
