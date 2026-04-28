// Task list page with filtering and task creation
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '../auth/AuthContext';
import { fetchTasks, createTask, deleteTask } from '../api/taskApi';
import type { TaskResponse, TaskStatusEnum, TaskPriorityEnum } from '../types';

const createTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title must be at most 200 characters'),
  description: z.string().max(5000, 'Description must be at most 5000 characters').optional().default(''),
  status: z.enum(['Todo', 'InProgress', 'Done']),
  priority: z.enum(['Low', 'Medium', 'High']),
  dueDate: z.string().optional().default(''),
});

type CreateTaskFormData = z.infer<typeof createTaskSchema>;

const statusColors: Record<string, string> = {
  Todo: 'bg-gray-200 text-gray-800',
  InProgress: 'bg-blue-200 text-blue-800',
  Done: 'bg-green-200 text-green-800',
};

const priorityColors: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-700',
  Medium: 'bg-yellow-100 text-yellow-700',
  High: 'bg-red-100 text-red-700',
};

export default function TaskListPage() {
  const { userId, displayName, logout } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [priorityFilter, setPriorityFilter] = useState<string>('');
  const [showCreate, setShowCreate] = useState(false);

  const params: Record<string, string> = {};
  if (statusFilter) params.status = statusFilter;
  if (priorityFilter) params.priority = priorityFilter;

  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks', statusFilter, priorityFilter],
    queryFn: () => fetchTasks(params),
  });

  const createMutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setShowCreate(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTask,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tasks'] }),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
    defaultValues: { status: 'Todo', priority: 'Medium' },
  });

  const onCreate = (data: CreateTaskFormData) => {
    createMutation.mutate({
      title: data.title,
      description: data.description || null,
      status: data.status as TaskStatusEnum,
      priority: data.priority as TaskPriorityEnum,
      dueDate: data.dueDate || null,
      assignedToId: null,
    });
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      deleteMutation.mutate(id);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow px-6 py-4 flex items-center justify-between">
        <h1 className="text-xl font-bold">TaskManager</h1>
        <div className="flex items-center gap-4">
          <span className="text-sm text-gray-600">Hello, {displayName}</span>
          <button
            onClick={() => { logout(); navigate('/login'); }}
            className="text-sm text-red-600 hover:underline"
          >
            Logout
          </button>
        </div>
      </nav>

      <div className="max-w-4xl mx-auto px-4 py-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex gap-3">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="border rounded px-3 py-1.5 text-sm"
            >
              <option value="">All Statuses</option>
              <option value="Todo">Todo</option>
              <option value="InProgress">In Progress</option>
              <option value="Done">Done</option>
            </select>
            <select
              value={priorityFilter}
              onChange={(e) => setPriorityFilter(e.target.value)}
              className="border rounded px-3 py-1.5 text-sm"
            >
              <option value="">All Priorities</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>
          <button
            onClick={() => setShowCreate(!showCreate)}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 text-sm"
          >
            New Task
          </button>
        </div>

        {showCreate && (
          <div className="bg-white p-4 rounded-lg shadow mb-6">
            <h2 className="text-lg font-semibold mb-3">Create Task</h2>
            <form onSubmit={handleSubmit(onCreate)} className="space-y-3">
              <div>
                <input {...register('title')} placeholder="Title" className="w-full border rounded px-3 py-2" />
                {errors.title && <p className="text-red-500 text-sm">{errors.title.message}</p>}
              </div>
              <div>
                <textarea {...register('description')} placeholder="Description (optional)" className="w-full border rounded px-3 py-2" rows={3} />
                {errors.description && <p className="text-red-500 text-sm">{errors.description.message}</p>}
              </div>
              <div className="flex gap-3">
                <select {...register('status')} className="border rounded px-3 py-2">
                  <option value="Todo">Todo</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
                <select {...register('priority')} className="border rounded px-3 py-2">
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                </select>
                <input {...register('dueDate')} type="date" className="border rounded px-3 py-2" />
              </div>
              <div className="flex gap-2">
                <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700" disabled={createMutation.isPending}>
                  Create
                </button>
                <button type="button" onClick={() => { setShowCreate(false); reset(); }} className="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400">
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        {isLoading ? (
          <p className="text-center text-gray-500">Loading tasks...</p>
        ) : !tasks || tasks.length === 0 ? (
          <p className="text-center text-gray-500">No tasks found. Create one!</p>
        ) : (
          <div className="space-y-3">
            {tasks.map((task: TaskResponse) => (
              <div key={task.id} className="bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow">
                <div className="flex items-start justify-between">
                  <Link to={`/tasks/${task.id}`} className="text-lg font-medium text-blue-600 hover:underline">
                    {task.title}
                  </Link>
                  {task.createdById === userId && (
                    <button
                      onClick={(e) => { e.preventDefault(); handleDelete(task.id); }}
                      className="text-red-500 text-sm hover:underline"
                    >
                      Delete
                    </button>
                  )}
                </div>
                <div className="flex gap-2 mt-2">
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusColors[task.status] || 'bg-gray-200'}`}>
                    {task.status}
                  </span>
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${priorityColors[task.priority] || 'bg-gray-100'}`}>
                    {task.priority}
                  </span>
                  {task.assignedToName && (
                    <span className="text-xs text-gray-500">Assigned: {task.assignedToName}</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}