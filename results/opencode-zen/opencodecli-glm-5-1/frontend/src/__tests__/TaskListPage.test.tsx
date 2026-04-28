// Tests for TaskList — renders tasks, filters work
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '../auth/AuthContext';
import TaskListPage from '../pages/TaskListPage';

// Mock the taskApi module to avoid real HTTP calls
vi.mock('../api/taskApi', () => ({
  fetchTasks: vi.fn(),
  createTask: vi.fn(),
  deleteTask: vi.fn(),
}));

import { fetchTasks } from '../api/taskApi';

const mockedFetchTasks = vi.mocked(fetchTasks);

const mockTasks = [
  {
    id: '1',
    title: 'Test Task 1',
    description: 'Description 1',
    status: 'Todo',
    priority: 'High',
    dueDate: null,
    createdById: 'user1',
    createdByName: 'User One',
    assignedToId: null,
    assignedToName: null,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    title: 'Test Task 2',
    description: null,
    status: 'InProgress',
    priority: 'Medium',
    dueDate: '2024-12-31T00:00:00Z',
    createdById: 'user2',
    createdByName: 'User Two',
    assignedToId: 'user1',
    assignedToName: 'User One',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
];

function renderTaskListPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <TaskListPage />
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>,
  );
}

describe('TaskListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('userId', 'user1');
    localStorage.setItem('displayName', 'Test User');
  });

  it('renders tasks returned from the API', async () => {
    mockedFetchTasks.mockResolvedValueOnce(mockTasks);

    renderTaskListPage();

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });
  });

  it('renders status and priority filter dropdowns', () => {
    mockedFetchTasks.mockResolvedValueOnce([]);

    renderTaskListPage();

    expect(screen.getByDisplayValue(/all statuses/i)).toBeInTheDocument();
    expect(screen.getByDisplayValue(/all priorities/i)).toBeInTheDocument();
  });

  it('shows loading state while fetching tasks', () => {
    mockedFetchTasks.mockReturnValue(new Promise(() => {}));

    renderTaskListPage();

    expect(screen.getByText(/loading tasks/i)).toBeInTheDocument();
  });

  it('shows empty state when no tasks exist', async () => {
    mockedFetchTasks.mockResolvedValueOnce([]);

    renderTaskListPage();

    await waitFor(() => {
      expect(screen.getByText(/no tasks found/i)).toBeInTheDocument();
    });
  });
});