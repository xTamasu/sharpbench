// TaskListPage tests — verifies task rendering and filter functionality.

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import TaskListPage from '@/pages/TaskListPage';
import * as services from '@/api/services';
import type { Task } from '@/types';

vi.mock('@/api/services', () => ({
  getTasks: vi.fn(),
  createTask: vi.fn(),
  deleteTask: vi.fn(),
}));

const mockTasks: Task[] = [
  {
    id: 'task-1',
    title: 'Task One',
    description: 'Description 1',
    status: 'Todo',
    priority: 'High',
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-1',
    createdByName: 'Creator',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    comments: [],
  },
  {
    id: 'task-2',
    title: 'Task Two',
    description: 'Description 2',
    status: 'Done',
    priority: 'Low',
    dueDate: null,
    assignedToId: null,
    assignedToName: null,
    createdById: 'user-1',
    createdByName: 'Creator',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
    comments: [],
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
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{component}</MemoryRouter>
    </QueryClientProvider>,
  );
};

describe('TaskListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.setItem('token', 'fake-token');
    localStorage.setItem('userId', 'user-1');
    localStorage.setItem('displayName', 'Test User');
  });

  it('renders tasks from the API', async () => {
    vi.mocked(services.getTasks).mockResolvedValue(mockTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Task One')).toBeInTheDocument();
      expect(screen.getByText('Task Two')).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    vi.mocked(services.getTasks).mockReturnValue(
      new Promise(() => {}),
    );

    renderWithProviders(<TaskListPage />);

    expect(screen.getByText(/loading tasks/i)).toBeInTheDocument();
  });

  it('filters tasks by status', async () => {
    const filteredTasks = mockTasks.filter((t) => t.status === 'Done');
    vi.mocked(services.getTasks)
      .mockResolvedValueOnce(mockTasks)
      .mockResolvedValueOnce(filteredTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Task One')).toBeInTheDocument();
    });

    const statusSelect = screen.getByRole('combobox', { name: 'Status' });
    fireEvent.change(statusSelect, { target: { value: 'Done' } });

    await waitFor(() => {
      expect(services.getTasks).toHaveBeenCalledWith(
        expect.objectContaining({ status: 'Done' }),
      );
    });
  });

  it('filters tasks by priority', async () => {
    const filteredTasks = mockTasks.filter((t) => t.priority === 'High');
    vi.mocked(services.getTasks)
      .mockResolvedValueOnce(mockTasks)
      .mockResolvedValueOnce(filteredTasks);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Task One')).toBeInTheDocument();
    });

    const prioritySelect = screen.getByRole('combobox', { name: 'Priority' });
    fireEvent.change(prioritySelect, { target: { value: 'High' } });

    await waitFor(() => {
      expect(services.getTasks).toHaveBeenCalledWith(
        expect.objectContaining({ priority: 'High' }),
      );
    });
  });

  it('shows no tasks message when list is empty', async () => {
    vi.mocked(services.getTasks).mockResolvedValue([]);

    renderWithProviders(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('No tasks found.')).toBeInTheDocument();
    });
  });
});
