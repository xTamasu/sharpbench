// TaskListPage — displays all tasks with a filter bar for status and priority.

import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTasks, deleteTask } from '@/api/services';
import type { TaskStatus, TaskPriority, Task } from '@/types';

const statusOptions: { value: TaskStatus | ''; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'Todo', label: 'Todo' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Done', label: 'Done' },
];

const priorityOptions: { value: TaskPriority | ''; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
];

const priorityColors: Record<TaskPriority, string> = {
  Low: 'bg-green-100 text-green-800',
  Medium: 'bg-yellow-100 text-yellow-800',
  High: 'bg-red-100 text-red-800',
};

const statusColors: Record<TaskStatus, string> = {
  Todo: 'bg-gray-100 text-gray-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Done: 'bg-green-100 text-green-800',
};

export default function TaskListPage() {
  const queryClient = useQueryClient();
  const [statusFilter, setStatusFilter] = useState<TaskStatus | ''>('');
  const [priorityFilter, setPriorityFilter] = useState<TaskPriority | ''>('');

  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks', statusFilter, priorityFilter],
    queryFn: () =>
      getTasks({
        status: statusFilter || undefined,
        priority: priorityFilter || undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteTask(id),
    onSuccess: () => {
      // Invalidate tasks query to refetch the list after deletion
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('displayName');
    window.location.href = '/login';
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
          <h1 className="text-xl font-bold">Task Manager</h1>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-600">
              {localStorage.getItem('displayName')}
            </span>
            <button
              onClick={handleLogout}
              className="text-sm text-red-600 hover:underline"
            >
              Logout
            </button>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto px-4 py-6">
        {/* Filter Bar */}
        <div className="bg-white p-4 rounded-lg shadow-sm mb-6 flex flex-wrap gap-4 items-end">
          <div>
            <label htmlFor="status-filter" className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              id="status-filter"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as TaskStatus | '')}
              className="border border-gray-300 rounded-md px-3 py-2"
            >
              {statusOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="priority-filter" className="block text-sm font-medium text-gray-700 mb-1">
              Priority
            </label>
            <select
              id="priority-filter"
              value={priorityFilter}
              onChange={(e) =>
                setPriorityFilter(e.target.value as TaskPriority | '')
              }
              className="border border-gray-300 rounded-md px-3 py-2"
            >
              {priorityOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Task List */}
        {isLoading ? (
          <p className="text-gray-500">Loading tasks...</p>
        ) : tasks && tasks.length > 0 ? (
          <div className="space-y-3">
            {tasks.map((task: Task) => (
              <div
                key={task.id}
                className="bg-white p-4 rounded-lg shadow-sm flex justify-between items-start"
              >
                <div className="flex-1">
                  <Link
                    to={`/tasks/${task.id}`}
                    className="text-lg font-semibold text-blue-600 hover:underline"
                  >
                    {task.title}
                  </Link>
                  <div className="flex gap-2 mt-2">
                    <span
                      className={`px-2 py-1 rounded text-xs font-medium ${statusColors[task.status]}`}
                    >
                      {task.status}
                    </span>
                    <span
                      className={`px-2 py-1 rounded text-xs font-medium ${priorityColors[task.priority]}`}
                    >
                      {task.priority}
                    </span>
                  </div>
                  {task.assignedToName && (
                    <p className="text-sm text-gray-500 mt-1">
                      Assigned to: {task.assignedToName}
                    </p>
                  )}
                </div>
                <button
                  onClick={() => deleteMutation.mutate(task.id)}
                  className="text-red-600 hover:text-red-800 text-sm ml-4"
                >
                  Delete
                </button>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No tasks found.</p>
        )}
      </div>
    </div>
  );
}
