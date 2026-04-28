import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import TaskListPage from './TaskListPage';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { apiClient } from '../apiClient';

vi.mock('../apiClient');

describe('TaskListPage', () => {
  const queryClient = new QueryClient();

  const renderComponent = () => render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <TaskListPage />
      </BrowserRouter>
    </QueryClientProvider>
  );

  it('renders tasks', async () => {
    const mockTasks = [
      { id: '1', title: 'Task 1', status: 0, priority: 1, createdById: 'u1', createdAt: '', updatedAt: '' },
      { id: '2', title: 'Task 2', status: 2, priority: 2, createdById: 'u1', createdAt: '', updatedAt: '' },
    ];
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockTasks });

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Task 1')).toBeInTheDocument();
      expect(screen.getByText('Task 2')).toBeInTheDocument();
    });
  });

  it('filters work by triggering new fetch', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: [] });
    renderComponent();

    await waitFor(() => {
      expect(apiClient.get).toHaveBeenCalledWith('/tasks?');
    });

    fireEvent.change(screen.getByLabelText(/Status:/i), { target: { value: '0' } });

    await waitFor(() => {
      expect(apiClient.get).toHaveBeenCalledWith('/tasks?status=0');
    });
  });
});
