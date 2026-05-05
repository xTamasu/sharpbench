// Zod validation schemas — enforce the same constraints as the backend DTOs.

import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
});

export const registerSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  displayName: z.string().min(1, 'Display name is required').max(100, 'Display name must be at most 100 characters'),
});

export const createTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title must be at most 200 characters'),
  description: z.string().max(5000, 'Description must be at most 5000 characters').optional(),
  status: z.enum(['Todo', 'InProgress', 'Done']),
  priority: z.enum(['Low', 'Medium', 'High']),
  dueDate: z.string().optional(),
  assignedToId: z.string().optional(),
});

export const commentSchema = z.object({
  body: z.string().min(1, 'Comment body is required').max(2000, 'Comment body must be at most 2000 characters'),
});
