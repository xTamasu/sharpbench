// comments.ts — typed wrappers for /api/tasks/{id}/comments endpoints.
import { apiClient } from './apiClient';
import type { Comment } from '../types';

export async function addComment(taskId: string, body: string): Promise<Comment> {
  const { data } = await apiClient.post<Comment>(`/tasks/${taskId}/comments`, { body });
  return data;
}

export async function updateComment(taskId: string, commentId: string, body: string): Promise<Comment> {
  const { data } = await apiClient.put<Comment>(`/tasks/${taskId}/comments/${commentId}`, { body });
  return data;
}

export async function deleteComment(taskId: string, commentId: string): Promise<void> {
  await apiClient.delete(`/tasks/${taskId}/comments/${commentId}`);
}
