// Tests for TaskCommentSection — renders comments, submit adds comment, own comments show edit/delete
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { CommentResponse } from '../types';

// Mock the commentApi module to avoid real HTTP calls
const mockCreateComment = vi.fn();
const mockUpdateComment = vi.fn();
const mockDeleteComment = vi.fn();

vi.mock('../api/commentApi', () => ({
  createComment: (...args: unknown[]) => mockCreateComment(...args),
  updateComment: (...args: unknown[]) => mockUpdateComment(...args),
  deleteComment: (...args: unknown[]) => mockDeleteComment(...args),
}));

// Mock the queryClient invalidateQueries
const mockInvalidate = vi.fn();
vi.mock('@tanstack/react-query', async () => {
  const actual = await vi.importActual('@tanstack/react-query');
  return {
    ...actual,
    useQueryClient: () => ({ invalidateQueries: mockInvalidate }),
    useMutation: (opts: { mutationFn: unknown; onSuccess?: () => void }) => {
      const fn = opts.mutationFn as (...args: unknown[]) => Promise<unknown>;
      return {
        mutate: async (...args: unknown[]) => {
          await fn(...args);
          if (opts.onSuccess) opts.onSuccess();
        },
        mutateAsync: async (...args: unknown[]) => {
          await fn(...args);
          if (opts.onSuccess) opts.onSuccess();
        },
        isPending: false,
      };
    },
  };
});

import TaskCommentSection from '../components/TaskCommentSection';

const mockComments: CommentResponse[] = [
  {
    id: 'c1',
    taskId: 't1',
    authorId: 'user1',
    authorName: 'Current User',
    body: 'This is my comment',
    editedAt: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'c2',
    taskId: 't1',
    authorId: 'user2',
    authorName: 'Other User',
    body: 'This is someone else\'s comment',
    editedAt: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

function renderCommentSection(comments: CommentResponse[] = mockComments, currentUserId = 'user1') {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <TaskCommentSection taskId="t1" comments={comments} currentUserId={currentUserId} />
    </QueryClientProvider>,
  );
}

describe('TaskCommentSection', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders all comments', () => {
    renderCommentSection();

    expect(screen.getByText('This is my comment')).toBeInTheDocument();
    expect(screen.getByText('This is someone else\'s comment')).toBeInTheDocument();
  });

  it('shows edit and delete buttons only for own comments', () => {
    renderCommentSection();

    // Own comment should have edit/delete
    const myComment = screen.getByText('This is my comment').closest('.bg-white');
    expect(myComment?.querySelector('button')).toBeTruthy();

    // Other user's comment should NOT have edit/delete
    const otherComment = screen.getByText('This is someone else\'s comment').closest('.bg-white');
    const buttons = otherComment?.querySelectorAll('button');
    expect(buttons?.length).toBe(0);
  });

  it('submit adds a new comment', async () => {
    mockCreateComment.mockResolvedValueOnce({ id: 'c3', taskId: 't1', authorId: 'user1', authorName: 'Current User', body: 'New comment', editedAt: null, createdAt: '2024-01-03T00:00:00Z' });

    renderCommentSection();

    const user = userEvent.setup();
    const textarea = screen.getByPlaceholderText(/write a comment/i);
    await user.type(textarea, 'New comment');
    await user.click(screen.getByRole('button', { name: /add comment/i }));

    expect(mockCreateComment).toHaveBeenCalledWith('t1', { body: 'New comment' });
  });

  it('has a comment textarea for adding new comments', () => {
    renderCommentSection();

    expect(screen.getByPlaceholderText(/write a comment/i)).toBeInTheDocument();
  });
});