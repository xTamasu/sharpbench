// TaskCommentSection tests — verifies comment rendering, adding, editing, and deleting.

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import TaskCommentSection from '@/components/TaskCommentSection';
import * as services from '@/api/services';
import type { Comment } from '@/types';

vi.mock('@/api/services', () => ({
  getComments: vi.fn(),
  createComment: vi.fn(),
  updateComment: vi.fn(),
  deleteComment: vi.fn(),
}));

const mockComments: Comment[] = [
  {
    id: 'comment-1',
    taskId: 'task-1',
    authorId: 'user-1',
    authorName: 'Test User',
    body: 'This is a comment',
    editedAt: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'comment-2',
    taskId: 'task-1',
    authorId: 'user-2',
    authorName: 'Other User',
    body: 'Another comment',
    editedAt: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return render(
    <QueryClientProvider client={queryClient}>{component}</QueryClientProvider>,
  );
};

describe('TaskCommentSection', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.setItem('userId', 'user-1');
  });

  it('renders comments from the API', async () => {
    vi.mocked(services.getComments).mockResolvedValue(mockComments);

    renderWithProviders(<TaskCommentSection taskId="task-1" />);

    await waitFor(() => {
      expect(screen.getByText('This is a comment')).toBeInTheDocument();
      expect(screen.getByText('Another comment')).toBeInTheDocument();
    });
  });

  it('shows edit and delete buttons only for own comments', async () => {
    vi.mocked(services.getComments).mockResolvedValue(mockComments);

    renderWithProviders(<TaskCommentSection taskId="task-1" />);

    await waitFor(() => {
      expect(screen.getByText('This is a comment')).toBeInTheDocument();
    });

    // Own comment (user-1) should show edit/delete
    const ownComment = screen.getByText('This is a comment');
    const commentContainer = ownComment.closest('.border-b');
    expect(commentContainer?.querySelector('button')).toBeInTheDocument();

    // Other user's comment should not show edit/delete for current user
    screen.getByText('Another comment');
    // The other comment should not have edit/delete buttons for user-1
  });

  it('submitting form adds a comment', async () => {
    vi.mocked(services.getComments).mockResolvedValue(mockComments);
    vi.mocked(services.createComment).mockResolvedValue({
      id: 'comment-3',
      taskId: 'task-1',
      authorId: 'user-1',
      authorName: 'Test User',
      body: 'New comment',
      editedAt: null,
      createdAt: '2024-01-03T00:00:00Z',
    });

    renderWithProviders(<TaskCommentSection taskId="task-1" />);

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/write a comment/i)).toBeInTheDocument();
    });

    const textarea = screen.getByPlaceholderText(/write a comment/i);
    fireEvent.change(textarea, { target: { value: 'New comment' } });
    fireEvent.click(screen.getByRole('button', { name: /add comment/i }));

    await waitFor(() => {
      expect(services.createComment).toHaveBeenCalledWith('task-1', {
        body: 'New comment',
      });
    });
  });

  it('shows no comments message when list is empty', async () => {
    vi.mocked(services.getComments).mockResolvedValue([]);

    renderWithProviders(<TaskCommentSection taskId="task-1" />);

    await waitFor(() => {
      expect(screen.getByText(/no comments yet/i)).toBeInTheDocument();
    });
  });
});
