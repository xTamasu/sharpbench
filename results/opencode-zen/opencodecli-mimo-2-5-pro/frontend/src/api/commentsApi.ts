// API functions for task comment operations.

import apiClient from './apiClient';
import type { Comment, CreateCommentRequest, UpdateCommentRequest } from '../types';

export const commentsApi = {
  addComment: async (taskId: string, data: CreateCommentRequest): Promise<Comment> => {
    const response = await apiClient.post<Comment>(`/tasks/${taskId}/comments`, data);
    return response.data;
  },

  updateComment: async (
    taskId: string,
    commentId: string,
    data: UpdateCommentRequest
  ): Promise<Comment> => {
    const response = await apiClient.put<Comment>(
      `/tasks/${taskId}/comments/${commentId}`,
      data
    );
    return response.data;
  },

  deleteComment: async (taskId: string, commentId: string): Promise<void> => {
    await apiClient.delete(`/tasks/${taskId}/comments/${commentId}`);
  },
};
