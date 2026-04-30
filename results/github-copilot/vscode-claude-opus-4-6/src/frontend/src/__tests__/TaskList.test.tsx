// Tests for TaskListPage: renders tasks, filters work.
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import TaskListPage from '../pages/TaskListPage';
import { TaskItemStatus, TaskPriority } from '../types';

// Mock the API module
vi.mock('../api', () => ({
  getTasks: vi.fn(),
  createTask: vi.fn(),
  deleteTask: vi.fn(),
}));

import { getTasks } from '../api';

const mockedGetTasks = vi.mocked(getTasks);

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{ui}</BrowserRouter>
    </QueryClientProvider>
  );
}

const mockTasks = [
  {
    id: '1',
    title: 'First Task',
    description: 'Description 1',
    status: TaskItemStatus.Todo,
    priority: TaskPriority.High,
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-1',
    createdByName: 'User One',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    comments: [],
  },
  {
    id: '2',
    title: 'Second Task',
    description: null,
    status: TaskItemStatus.InProgress,
    priority: TaskPriority.Low,
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-2',
    createdByName: 'User Two',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
    comments: [],
  },
];

describe('TaskListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.setItem('user', JSON.stringify({ userId: 'user-1', displayName: 'Test User' }));
    localStorage.setItem('token', 'test-token');
  });

  it('renders the list of tasks', async () => {
    mockedGetTasks.mockResolvedValueOnce(mockTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('First Task')).toBeInTheDocument();
      expect(screen.getByText('Second Task')).toBeInTheDocument();
    });
  });

  it('shows "No tasks found" when the list is empty', async () => {
    mockedGetTasks.mockResolvedValueOnce([]);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText(/no tasks found/i)).toBeInTheDocument();
    });
  });

  it('calls getTasks with filter params when status filter is changed', async () => {
    const user = userEvent.setup();
    mockedGetTasks.mockResolvedValue(mockTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('First Task')).toBeInTheDocument();
    });

    // Change the status filter
    const statusSelect = screen.getAllByRole('combobox')[0];
    await user.selectOptions(statusSelect, '0'); // Todo

    await waitFor(() => {
      expect(mockedGetTasks).toHaveBeenCalledWith(
        expect.objectContaining({ status: TaskItemStatus.Todo })
      );
    });
  });

  it('calls getTasks with priority filter params', async () => {
    const user = userEvent.setup();
    mockedGetTasks.mockResolvedValue(mockTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('First Task')).toBeInTheDocument();
    });

    // Change the priority filter
    const prioritySelect = screen.getAllByRole('combobox')[1];
    await user.selectOptions(prioritySelect, '2'); // High

    await waitFor(() => {
      expect(mockedGetTasks).toHaveBeenCalledWith(
        expect.objectContaining({ priority: TaskPriority.High })
      );
    });
  });

  it('shows delete button only for tasks created by the current user', async () => {
    mockedGetTasks.mockResolvedValueOnce(mockTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('First Task')).toBeInTheDocument();
    });

    // Only one delete button should show (for user-1's task)
    const deleteButtons = screen.getAllByText('Delete');
    expect(deleteButtons).toHaveLength(1);
  });
});
