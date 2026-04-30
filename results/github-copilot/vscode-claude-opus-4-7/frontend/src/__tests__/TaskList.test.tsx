// TaskList.test.tsx
// Verifies the task list renders the response and that the status filter triggers
// a re-query with the chosen filter argument.
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TaskList from '../components/TaskList';
import { renderWithProviders } from '../test/testUtils';
import type { TaskItem } from '../types';

vi.mock('../api/tasks', () => ({
  listTasks: vi.fn(),
}));

import { listTasks } from '../api/tasks';
const listTasksMock = vi.mocked(listTasks);

const sampleTasks: TaskItem[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'First task',
    description: 'desc',
    status: 'Todo',
    priority: 'High',
    dueDate: null,
    assignedTo: null,
    createdBy: { id: 'u1', email: 'a@b.c', displayName: 'Alice' },
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    title: 'Second task',
    description: null,
    status: 'Done',
    priority: 'Low',
    dueDate: null,
    assignedTo: null,
    createdBy: { id: 'u1', email: 'a@b.c', displayName: 'Alice' },
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

describe('TaskList', () => {
  beforeEach(() => listTasksMock.mockReset());

  it('renders tasks from the API', async () => {
    listTasksMock.mockResolvedValue(sampleTasks);
    renderWithProviders(<TaskList />);

    expect(await screen.findByText('First task')).toBeInTheDocument();
    expect(screen.getByText('Second task')).toBeInTheDocument();
  });

  it('passes the selected filter values to listTasks', async () => {
    listTasksMock.mockResolvedValue(sampleTasks);
    const user = userEvent.setup();
    renderWithProviders(<TaskList />);

    await screen.findByText('First task');

    await user.selectOptions(screen.getByLabelText(/filter by status/i), 'Todo');
    await user.selectOptions(screen.getByLabelText(/filter by priority/i), 'High');

    await waitFor(() =>
      expect(listTasksMock).toHaveBeenLastCalledWith({ status: 'Todo', priority: 'High' }),
    );
  });
});
