// Tests for TaskCommentSection: renders comments, submit adds a comment, own comments show edit/delete.
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import TaskCommentSection from '../components/TaskCommentSection';
import type { CommentResponse } from '../types';

// Mock the API module
vi.mock('../api', () => ({
  createComment: vi.fn(),
  updateComment: vi.fn(),
  deleteComment: vi.fn(),
}));

import { createComment, deleteComment } from '../api';

const mockedCreateComment = vi.mocked(createComment);
const mockedDeleteComment = vi.mocked(deleteComment);

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>
  );
}

const mockComments: CommentResponse[] = [
  {
    id: 'comment-1',
    taskId: 'task-1',
    authorId: 'user-1',
    authorName: 'Current User',
    body: 'My comment',
    editedAt: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'comment-2',
    taskId: 'task-1',
    authorId: 'user-2',
    authorName: 'Other User',
    body: 'Their comment',
    editedAt: '2024-01-02T00:00:00Z',
    createdAt: '2024-01-01T12:00:00Z',
  },
];

describe('TaskCommentSection', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.setItem('user', JSON.stringify({ userId: 'user-1', displayName: 'Current User' }));
  });

  it('renders all comments', () => {
    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    expect(screen.getByText('My comment')).toBeInTheDocument();
    expect(screen.getByText('Their comment')).toBeInTheDocument();
  });

  it('shows "(edited)" for edited comments', () => {
    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    expect(screen.getByText('(edited)')).toBeInTheDocument();
  });

  it('shows edit/delete buttons only for own comments', () => {
    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    // The current user (user-1) should see Edit/Delete on their own comment
    const editButtons = screen.getAllByText('Edit');
    const deleteButtons = screen.getAllByText('Delete');

    // Only 1 edit and 1 delete button (for user-1's comment only)
    expect(editButtons).toHaveLength(1);
    expect(deleteButtons).toHaveLength(1);
  });

  it('submits a new comment', async () => {
    const user = userEvent.setup();
    mockedCreateComment.mockResolvedValueOnce({
      id: 'comment-3',
      taskId: 'task-1',
      authorId: 'user-1',
      authorName: 'Current User',
      body: 'New comment',
      editedAt: null,
      createdAt: '2024-01-03T00:00:00Z',
    });

    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    const textarea = screen.getByPlaceholderText(/add a comment/i);
    await user.type(textarea, 'New comment');
    await user.click(screen.getByRole('button', { name: /add comment/i }));

    await waitFor(() => {
      expect(mockedCreateComment).toHaveBeenCalledWith('task-1', { body: 'New comment' });
    });
  });

  it('shows validation error for empty comment body', async () => {
    const user = userEvent.setup();
    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    await user.click(screen.getByRole('button', { name: /add comment/i }));

    await waitFor(() => {
      expect(screen.getByText(/comment body is required/i)).toBeInTheDocument();
    });
  });

  it('enters edit mode when Edit is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    await user.click(screen.getByText('Edit'));

    // The edit form should now be visible with a Save button
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
    });
  });

  it('calls deleteComment when Delete is clicked and confirmed', async () => {
    const user = userEvent.setup();
    mockedDeleteComment.mockResolvedValueOnce(undefined);
    vi.spyOn(window, 'confirm').mockReturnValueOnce(true);

    renderWithProviders(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    await user.click(screen.getByText('Delete'));

    await waitFor(() => {
      expect(mockedDeleteComment).toHaveBeenCalledWith('task-1', 'comment-1');
    });
  });
});
