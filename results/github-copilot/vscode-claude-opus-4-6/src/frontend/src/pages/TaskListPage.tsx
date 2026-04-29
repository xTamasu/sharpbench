// Task list page: displays all tasks with filter bar for status and priority.
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { getTasks, createTask, deleteTask } from '../api';
import { TaskItemStatus, TaskPriority } from '../types';
import type { TaskResponse } from '../types';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createTaskSchema, type CreateTaskFormData } from '../schemas';

const statusLabels: Record<TaskItemStatus, string> = {
  [TaskItemStatus.Todo]: 'Todo',
  [TaskItemStatus.InProgress]: 'In Progress',
  [TaskItemStatus.Done]: 'Done',
};

const priorityLabels: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Low',
  [TaskPriority.Medium]: 'Medium',
  [TaskPriority.High]: 'High',
};

function TaskListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [statusFilter, setStatusFilter] = useState<TaskItemStatus | ''>('');
  const [priorityFilter, setPriorityFilter] = useState<TaskPriority | ''>('');
  const [showCreateForm, setShowCreateForm] = useState(false);

  const { data: tasks = [], isLoading } = useQuery({
    queryKey: ['tasks', statusFilter, priorityFilter],
    queryFn: () =>
      getTasks({
        status: statusFilter !== '' ? statusFilter : undefined,
        priority: priorityFilter !== '' ? priorityFilter : undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTask,
    onSuccess: () => {
      // Invalidate task list after deletion
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
    defaultValues: {
      status: TaskItemStatus.Todo,
      priority: TaskPriority.Medium,
    },
  });

  const createMutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      // Invalidate and close form after creating a task
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setShowCreateForm(false);
      reset();
    },
  });

  const onCreateSubmit = (data: CreateTaskFormData) => {
    createMutation.mutate({
      title: data.title,
      description: data.description || undefined,
      status: data.status,
      priority: data.priority,
      dueDate: data.dueDate || undefined,
      assignedToId: data.assignedToId || undefined,
    });
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  const currentUser = JSON.parse(localStorage.getItem('user') || '{}');

  return (
    <div className="mx-auto max-w-6xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Tasks</h1>
        <div className="flex items-center gap-4">
          <span className="text-sm text-gray-600">Hi, {currentUser.displayName}</span>
          <button
            onClick={handleLogout}
            className="rounded bg-gray-200 px-3 py-1 text-sm hover:bg-gray-300"
          >
            Logout
          </button>
        </div>
      </div>

      {/* Filter bar */}
      <div className="mb-6 flex flex-wrap gap-4 rounded-lg bg-white p-4 shadow-sm">
        <div>
          <label className="block text-xs font-medium text-gray-500">Status</label>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value === '' ? '' : Number(e.target.value) as TaskItemStatus)}
            className="mt-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm"
          >
            <option value="">All</option>
            <option value={TaskItemStatus.Todo}>Todo</option>
            <option value={TaskItemStatus.InProgress}>In Progress</option>
            <option value={TaskItemStatus.Done}>Done</option>
          </select>
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500">Priority</label>
          <select
            value={priorityFilter}
            onChange={(e) => setPriorityFilter(e.target.value === '' ? '' : Number(e.target.value) as TaskPriority)}
            className="mt-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm"
          >
            <option value="">All</option>
            <option value={TaskPriority.Low}>Low</option>
            <option value={TaskPriority.Medium}>Medium</option>
            <option value={TaskPriority.High}>High</option>
          </select>
        </div>
        <div className="ml-auto flex items-end">
          <button
            onClick={() => setShowCreateForm(!showCreateForm)}
            className="rounded-md bg-blue-600 px-4 py-1.5 text-sm text-white hover:bg-blue-700"
          >
            {showCreateForm ? 'Cancel' : '+ New Task'}
          </button>
        </div>
      </div>

      {/* Create task form */}
      {showCreateForm && (
        <div className="mb-6 rounded-lg bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold">Create New Task</h2>
          <form onSubmit={handleSubmit(onCreateSubmit)} className="space-y-4">
            <div>
              <input
                {...register('title')}
                placeholder="Task title"
                className="w-full rounded-md border border-gray-300 px-3 py-2"
              />
              {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>}
            </div>
            <div>
              <textarea
                {...register('description')}
                placeholder="Description (optional)"
                rows={3}
                className="w-full rounded-md border border-gray-300 px-3 py-2"
              />
              {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>}
            </div>
            <div className="flex gap-4">
              <div>
                <label className="block text-xs font-medium text-gray-500">Status</label>
                <select {...register('status', { valueAsNumber: true })} className="mt-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm">
                  <option value={TaskItemStatus.Todo}>Todo</option>
                  <option value={TaskItemStatus.InProgress}>In Progress</option>
                  <option value={TaskItemStatus.Done}>Done</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-500">Priority</label>
                <select {...register('priority', { valueAsNumber: true })} className="mt-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm">
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-500">Due Date</label>
                <input type="date" {...register('dueDate')} className="mt-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm" />
              </div>
            </div>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700 disabled:opacity-50"
            >
              {createMutation.isPending ? 'Creating...' : 'Create Task'}
            </button>
          </form>
        </div>
      )}

      {/* Task list */}
      {isLoading ? (
        <p className="text-gray-500">Loading tasks...</p>
      ) : tasks.length === 0 ? (
        <p className="text-gray-500">No tasks found.</p>
      ) : (
        <div className="space-y-3">
          {tasks.map((task: TaskResponse) => (
            <div
              key={task.id}
              className="flex items-center justify-between rounded-lg bg-white p-4 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
              onClick={() => navigate(`/tasks/${task.id}`)}
            >
              <div className="flex-1">
                <h3 className="font-medium text-gray-900">{task.title}</h3>
                <div className="mt-1 flex gap-2 text-xs">
                  <span className="rounded bg-blue-100 px-2 py-0.5 text-blue-800">
                    {statusLabels[task.status]}
                  </span>
                  <span className="rounded bg-purple-100 px-2 py-0.5 text-purple-800">
                    {priorityLabels[task.priority]}
                  </span>
                  {task.assignedToName && (
                    <span className="text-gray-500">→ {task.assignedToName}</span>
                  )}
                </div>
              </div>
              {task.createdById === currentUser.userId && (
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    if (confirm('Delete this task?')) {
                      deleteMutation.mutate(task.id);
                    }
                  }}
                  className="ml-4 rounded bg-red-100 px-2 py-1 text-xs text-red-700 hover:bg-red-200"
                >
                  Delete
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default TaskListPage;
