import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import TaskCommentSection from './TaskCommentSection';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { apiClient } from '../apiClient';
import type { Comment } from '../types';

vi.mock('../apiClient');

describe('TaskCommentSection', () => {
  const queryClient = new QueryClient();

  beforeEach(() => {
    Object.defineProperty(window, 'localStorage', {
      value: {
        getItem: vi.fn(() => 'user1'),
        setItem: vi.fn(),
        removeItem: vi.fn(),
      },
      writable: true
    });
  });

  const renderComponent = (comments: Comment[] = []) => render(
    <QueryClientProvider client={queryClient}>
      <TaskCommentSection taskId="task1" comments={comments} />
    </QueryClientProvider>
  );

  it('renders comments', () => {
    const comments = [
      { id: '1', taskId: 'task1', authorId: 'user2', body: 'Comment 1', createdAt: new Date().toISOString() },
    ];
    renderComponent(comments);

    expect(screen.getByText('Comment 1')).toBeInTheDocument();
  });

  it('submit adds a comment', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: {} });
    renderComponent();

    fireEvent.change(screen.getByTestId('new-comment-input'), { target: { value: 'New comment' } });
    fireEvent.click(screen.getByRole('button', { name: 'Post' }));

    await waitFor(() => {
      expect(apiClient.post).toHaveBeenCalledWith('/tasks/task1/comments', { body: 'New comment' });
    });
  });

  it('own comments show edit and delete', () => {
    const comments = [
      { id: '1', taskId: 'task1', authorId: 'user1', body: 'My Comment', createdAt: new Date().toISOString() },
    ];
    renderComponent(comments);

    expect(screen.getByText('Edit')).toBeInTheDocument();
    expect(screen.getByText('Delete')).toBeInTheDocument();
  });
});
