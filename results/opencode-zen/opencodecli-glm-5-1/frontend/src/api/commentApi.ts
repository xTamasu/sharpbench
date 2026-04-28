// API functions for comment-related endpoints
import apiClient from './apiClient';
import type { CommentResponse, CreateCommentRequest, UpdateCommentRequest } from '../types';

export const createComment = async (taskId: string, request: CreateCommentRequest): Promise<CommentResponse> => {
  const { data } = await apiClient.post<CommentResponse>(`/tasks/${taskId}/comments`, request);
  return data;
};

export const updateComment = async (taskId: string, commentId: string, request: UpdateCommentRequest): Promise<CommentResponse> => {
  const { data } = await apiClient.put<CommentResponse>(`/tasks/${taskId}/comments/${commentId}`, request);
  return data;
};

export const deleteComment = async (taskId: string, commentId: string): Promise<void> => {
  await apiClient.delete(`/tasks/${taskId}/comments/${commentId}`);
};