// Tests for TaskCommentSection component.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from './test-utils';
import TaskCommentSection from '../components/TaskCommentSection';

// Mock the API modules
vi.mock('../api/commentsApi', () => ({
  commentsApi: {
    addComment: vi.fn(),
    updateComment: vi.fn(),
    deleteComment: vi.fn(),
  },
}));

// Mock useAuth to return a specific user
vi.mock('../hooks/useAuth', () => ({
  useAuth: () => ({
    user: { id: 'user-1', email: 'test@test.com', displayName: 'Test User' },
    token: 'fake-token',
    isAuthenticated: true,
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  }),
}));

import { commentsApi } from '../api/commentsApi';

const mockComments = [
  {
    id: 'comment-1',
    taskId: 'task-1',
    author: { id: 'user-1', email: 'test@test.com', displayName: 'Test User' },
    body: 'This is my comment',
    editedAt: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'comment-2',
    taskId: 'task-1',
    author: { id: 'user-2', email: 'other@test.com', displayName: 'Other User' },
    body: 'Another user comment',
    editedAt: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

describe('TaskCommentSection', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders comments list', () => {
    render(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    expect(screen.getByText('This is my comment')).toBeInTheDocument();
    expect(screen.getByText('Another user comment')).toBeInTheDocument();
    expect(screen.getByText('Test User')).toBeInTheDocument();
    expect(screen.getByText('Other User')).toBeInTheDocument();
  });

  it('shows empty state when no comments', () => {
    render(<TaskCommentSection taskId="task-1" comments={[]} />);

    expect(screen.getByText('No comments yet.')).toBeInTheDocument();
  });

  it('shows edit and delete buttons only for own comments', () => {
    render(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    // Own comment should have edit/delete buttons
    const editButtons = screen.getAllByText('Edit');
    const deleteButtons = screen.getAllByText('Delete');
    expect(editButtons).toHaveLength(1);
    expect(deleteButtons).toHaveLength(1);
  });

  it('adds a comment when form is submitted', async () => {
    vi.mocked(commentsApi.addComment).mockResolvedValue({
      id: 'new-comment',
      taskId: 'task-1',
      author: { id: 'user-1', email: 'test@test.com', displayName: 'Test User' },
      body: 'New comment',
      editedAt: null,
      createdAt: '2024-01-03T00:00:00Z',
    });

    const user = userEvent.setup();
    render(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    const textarea = screen.getByPlaceholderText('Add a comment...');
    await user.type(textarea, 'New comment');
    await user.click(screen.getByText('Add Comment'));

    await waitFor(() => {
      expect(commentsApi.addComment).toHaveBeenCalledWith('task-1', { body: 'New comment' });
    });
  });

  it('shows validation error for empty comment body', async () => {
    const user = userEvent.setup();
    render(<TaskCommentSection taskId="task-1" comments={mockComments} />);

    await user.click(screen.getByText('Add Comment'));

    await waitFor(() => {
      expect(screen.getByText(/comment body is required/i)).toBeInTheDocument();
    });
  });
});
