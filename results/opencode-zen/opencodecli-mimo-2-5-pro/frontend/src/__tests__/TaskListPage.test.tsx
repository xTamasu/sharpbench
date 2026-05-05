// Tests for TaskListPage component.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from './test-utils';
import TaskListPage from '../pages/TaskListPage';

// Mock the API modules
vi.mock('../api/tasksApi', () => ({
  tasksApi: {
    getTasks: vi.fn(),
    createTask: vi.fn(),
  },
}));

import { tasksApi } from '../api/tasksApi';
import type { Task } from '../types';

const mockTasks: Task[] = [
  {
    id: '1',
    title: 'Test Task 1',
    description: 'Description 1',
    status: 'Todo',
    priority: 'High',
    dueDate: null,
    assignedTo: null,
    createdBy: { id: '1', email: 'user@test.com', displayName: 'Test User' },
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    title: 'Test Task 2',
    description: 'Description 2',
    status: 'InProgress',
    priority: 'Low',
    dueDate: null,
    assignedTo: null,
    createdBy: { id: '1', email: 'user@test.com', displayName: 'Test User' },
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
];

describe('TaskListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(tasksApi.getTasks).mockResolvedValue(mockTasks);
  });

  it('renders task list with tasks', async () => {
    render(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });
  });

  it('renders filter controls', () => {
    render(<TaskListPage />);

    expect(screen.getByText('All Statuses')).toBeInTheDocument();
    expect(screen.getByText('All Priorities')).toBeInTheDocument();
  });

  it('filters by status when status filter is changed', async () => {
    const user = userEvent.setup();
    render(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
    });

    const statusSelect = screen.getByDisplayValue('All Statuses');
    await user.selectOptions(statusSelect, 'Todo');

    await waitFor(() => {
      expect(tasksApi.getTasks).toHaveBeenCalledWith(
        expect.objectContaining({ status: 'Todo' })
      );
    });
  });

  it('filters by priority when priority filter is changed', async () => {
    const user = userEvent.setup();
    render(<TaskListPage />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
    });

    const prioritySelect = screen.getByDisplayValue('All Priorities');
    await user.selectOptions(prioritySelect, 'High');

    await waitFor(() => {
      expect(tasksApi.getTasks).toHaveBeenCalledWith(
        expect.objectContaining({ priority: 'High' })
      );
    });
  });

  it('opens create modal when create button is clicked', async () => {
    const user = userEvent.setup();
    render(<TaskListPage />);

    await user.click(screen.getByText('Create Task'));

    expect(screen.getByText('Create New Task')).toBeInTheDocument();
  });
});
