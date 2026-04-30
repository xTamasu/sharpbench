// TaskCommentSection.test.tsx
// Verifies the comment thread rendering, that submitting calls addComment,
// and that the current user's own comments expose Edit / Delete actions
// while others' comments do not.
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TaskCommentSection from '../components/TaskCommentSection';
import { renderWithProviders } from '../test/testUtils';
import type { Comment, User } from '../types';

vi.mock('../api/comments', () => ({
  addComment: vi.fn(),
  updateComment: vi.fn(),
  deleteComment: vi.fn(),
}));

import { addComment } from '../api/comments';
const addMock = vi.mocked(addComment);

const me: User    = { id: 'me', email: 'me@x.io', displayName: 'Me' };
const other: User = { id: 'other', email: 'o@x.io', displayName: 'Other' };

const taskId = 'task-1';

const comments: Comment[] = [
  { id: 'c1', taskId, author: other, body: 'Hi from other', createdAt: new Date().toISOString(), editedAt: null },
  { id: 'c2', taskId, author: me,    body: 'My own comment', createdAt: new Date().toISOString(), editedAt: null },
];

describe('TaskCommentSection', () => {
  beforeEach(() => addMock.mockReset());

  it('renders all comments', () => {
    renderWithProviders(<TaskCommentSection taskId={taskId} comments={comments} />, { authenticatedAs: me });

    expect(screen.getByText('Hi from other')).toBeInTheDocument();
    expect(screen.getByText('My own comment')).toBeInTheDocument();
  });

  it('only shows Edit/Delete on the current user’s own comments', () => {
    renderWithProviders(<TaskCommentSection taskId={taskId} comments={comments} />, { authenticatedAs: me });

    const items = screen.getAllByTestId('comment-item');
    // First item belongs to "other" — no Edit/Delete buttons.
    expect(within(items[0]).queryByRole('button', { name: /edit/i })).toBeNull();
    expect(within(items[0]).queryByRole('button', { name: /delete/i })).toBeNull();
    // Second item belongs to "me" — both buttons appear.
    expect(within(items[1]).getByRole('button', { name: /edit/i })).toBeInTheDocument();
    expect(within(items[1]).getByRole('button', { name: /delete/i })).toBeInTheDocument();
  });

  it('submits a new comment via addComment', async () => {
    addMock.mockResolvedValue({
      id: 'new', taskId, author: me, body: 'A new comment',
      createdAt: new Date().toISOString(), editedAt: null,
    });

    const user = userEvent.setup();
    renderWithProviders(<TaskCommentSection taskId={taskId} comments={comments} />, { authenticatedAs: me });

    await user.type(screen.getByLabelText(/add a comment/i), 'A new comment');
    await user.click(screen.getByRole('button', { name: /post comment/i }));

    await waitFor(() => expect(addMock).toHaveBeenCalledWith(taskId, 'A new comment'));
  });
});
