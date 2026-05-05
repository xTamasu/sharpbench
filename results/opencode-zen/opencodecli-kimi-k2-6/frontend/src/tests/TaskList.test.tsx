import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import TaskListPage from '../pages/TaskListPage'

/**
 * Unit tests for the TaskListPage component.
 */

const mockTasks = [
  {
    id: 'task-1',
    title: 'Test Task 1',
    description: 'Description 1',
    status: 'Todo' as const,
    priority: 'High' as const,
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-1',
    createdByName: 'Test User',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'task-2',
    title: 'Test Task 2',
    description: 'Description 2',
    status: 'Done' as const,
    priority: 'Low' as const,
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-2',
    createdByName: 'Other User',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
]

// Mock auth hooks
vi.mock('../hooks/useAuth', () => ({
  getCurrentUser: () => ({ id: 'user-1', email: 'test@example.com', displayName: 'Test User' }),
  logout: vi.fn(),
}))

// Mock task hooks
vi.mock('../hooks/useTasks', () => ({
  useTasks: vi.fn((filter) => {
    let tasks = [...mockTasks]
    if (filter?.status) {
      tasks = tasks.filter((t) => t.status === filter.status)
    }
    if (filter?.priority) {
      tasks = tasks.filter((t) => t.priority === filter.priority)
    }
    return { data: tasks, isLoading: false, error: null }
  }),
  useDeleteTask: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
}))

function renderTaskListPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <TaskListPage />
      </BrowserRouter>
    </QueryClientProvider>
  )
}

describe('TaskListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders tasks list', () => {
    renderTaskListPage()

    expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    expect(screen.getByText('Test Task 2')).toBeInTheDocument()
  })

  it('filters tasks by status', async () => {
    const { useTasks } = await import('../hooks/useTasks')
    renderTaskListPage()

    const statusFilter = screen.getByLabelText(/status/i)
    await userEvent.selectOptions(statusFilter, 'Done')

    await waitFor(() => {
      expect(useTasks).toHaveBeenCalledWith(expect.objectContaining({ status: 'Done' }))
    })
  })

  it('filters tasks by priority', async () => {
    const { useTasks } = await import('../hooks/useTasks')
    renderTaskListPage()

    const priorityFilter = screen.getByLabelText(/priority/i)
    await userEvent.selectOptions(priorityFilter, 'High')

    await waitFor(() => {
      expect(useTasks).toHaveBeenCalledWith(expect.objectContaining({ priority: 'High' }))
    })
  })

  it('shows clear filters button', async () => {
    renderTaskListPage()

    const clearButton = screen.getByRole('button', { name: /clear filters/i })
    expect(clearButton).toBeInTheDocument()

    await userEvent.click(clearButton)
  })
})
