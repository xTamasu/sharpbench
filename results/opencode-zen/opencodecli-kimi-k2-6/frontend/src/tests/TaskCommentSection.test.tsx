import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import TaskCommentSection from '../components/TaskCommentSection'

/**
 * Unit tests for the TaskCommentSection component.
 */

const mockComments = [
  {
    id: 'comment-1',
    taskId: 'task-1',
    authorId: 'user-1',
    authorName: 'Test User',
    body: 'First comment',
    editedAt: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'comment-2',
    taskId: 'task-1',
    authorId: 'user-2',
    authorName: 'Other User',
    body: 'Second comment',
    editedAt: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
]

// Mock auth hooks to simulate current user as 'user-1'
vi.mock('../hooks/useAuth', () => ({
  getCurrentUser: () => ({ id: 'user-1', email: 'test@example.com', displayName: 'Test User' }),
}))

// Mock task hooks for comment operations
vi.mock('../hooks/useTasks', () => ({
  useCreateComment: () => ({
    mutate: vi.fn((_, options) => {
      options?.onSuccess?.()
    }),
    isPending: false,
  }),
  useUpdateComment: () => ({
    mutate: vi.fn((_, options) => {
      options?.onSuccess?.()
    }),
    isPending: false,
  }),
  useDeleteComment: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
}))

function renderCommentSection(comments = mockComments) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <TaskCommentSection taskId="task-1" comments={comments} />
    </QueryClientProvider>
  )
}

describe('TaskCommentSection', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders comments list', () => {
    renderCommentSection()

    expect(screen.getByText('First comment')).toBeInTheDocument()
    expect(screen.getByText('Second comment')).toBeInTheDocument()
    expect(screen.getByText('Test User')).toBeInTheDocument()
    expect(screen.getByText('Other User')).toBeInTheDocument()
  })

  it('shows edit and delete buttons for own comments', () => {
    renderCommentSection()

    // First comment is by user-1 (current user), so it should have edit/delete
    const editButtons = screen.getAllByText('Edit')
    const deleteButtons = screen.getAllByText('Delete')

    // Only one comment is by the current user
    expect(editButtons.length).toBe(1)
    expect(deleteButtons.length).toBe(1)
  })

  it('does not show edit/delete for other users comments', () => {
    renderCommentSection()

    // The second comment by Other User should not have visible edit/delete buttons
    // We verify this by checking only 1 set of buttons exists for 2 comments
    const editButtons = screen.getAllByText('Edit')
    expect(editButtons.length).toBe(1)
  })

  it('shows comment form', () => {
    renderCommentSection()

    expect(screen.getByPlaceholderText(/add a comment/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /post comment/i })).toBeInTheDocument()
  })

  it('shows validation error for empty comment', async () => {
    renderCommentSection()

    const submitButton = screen.getByRole('button', { name: /post comment/i })
    await userEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/comment is required/i)).toBeInTheDocument()
    })
  })

  it('enters edit mode when clicking edit', async () => {
    renderCommentSection()

    const editButton = screen.getByText('Edit')
    await userEvent.click(editButton)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
    })
  })

  it('shows empty state when no comments', () => {
    renderCommentSection([])

    expect(screen.getByText(/no comments yet/i)).toBeInTheDocument()
  })
})
