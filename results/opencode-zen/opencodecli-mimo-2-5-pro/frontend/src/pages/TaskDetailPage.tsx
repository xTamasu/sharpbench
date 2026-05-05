// Task detail page with edit functionality and comment section.

import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { tasksApi } from '../api/tasksApi';
import { useAuth } from '../hooks/useAuth';
import type { TaskStatus, Priority, UpdateTaskRequest } from '../types';
import TaskCommentSection from '../components/TaskCommentSection';

const updateTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().max(5000).optional(),
  status: z.enum(['Todo', 'InProgress', 'Done']),
  priority: z.enum(['Low', 'Medium', 'High']),
  dueDate: z.string().optional(),
});

type UpdateTaskFormData = z.infer<typeof updateTaskSchema>;

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [isEditing, setIsEditing] = useState(false);

  const { data: task, isLoading } = useQuery({
    queryKey: ['task', id],
    queryFn: () => tasksApi.getTask(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateTaskRequest) => tasksApi.updateTask(id!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', id] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setIsEditing(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => tasksApi.deleteTask(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      navigate('/tasks');
    },
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateTaskFormData>({
    resolver: zodResolver(updateTaskSchema),
    values: task
      ? {
          title: task.title,
          description: task.description || '',
          status: task.status,
          priority: task.priority,
          dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
        }
      : undefined,
  });

  const onSubmit = (data: UpdateTaskFormData) => {
    updateMutation.mutate({
      ...data,
      description: data.description || undefined,
      dueDate: data.dueDate || undefined,
    });
  };

  if (isLoading) {
    return <div className="text-center py-8">Loading task...</div>;
  }

  if (!task) {
    return <div className="text-center py-8">Task not found.</div>;
  }

  const isCreator = user?.id === task.createdBy.id;

  const statusColors: Record<TaskStatus, string> = {
    Todo: 'bg-gray-100 text-gray-800',
    InProgress: 'bg-blue-100 text-blue-800',
    Done: 'bg-green-100 text-green-800',
  };

  const priorityColors: Record<Priority, string> = {
    Low: 'bg-green-100 text-green-800',
    Medium: 'bg-yellow-100 text-yellow-800',
    High: 'bg-red-100 text-red-800',
  };

  return (
    <div className="max-w-3xl mx-auto">
      <button
        onClick={() => navigate('/tasks')}
        className="mb-4 text-blue-600 hover:underline"
      >
        ← Back to Tasks
      </button>

      {isEditing ? (
        <form onSubmit={handleSubmit(onSubmit)} className="bg-white rounded-lg shadow p-6 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
            <input
              {...register('title')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
            {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              {...register('description')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...register('status')} className="w-full px-3 py-2 border border-gray-300 rounded-md">
                <option value="Todo">Todo</option>
                <option value="InProgress">In Progress</option>
                <option value="Done">Done</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select {...register('priority')} className="w-full px-3 py-2 border border-gray-300 rounded-md">
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
            <input
              {...register('dueDate')}
              type="date"
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>

          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={() => setIsEditing(false)}
              className="px-4 py-2 text-gray-600 hover:text-gray-800"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={updateMutation.isPending}
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
            >
              {updateMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      ) : (
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex justify-between items-start mb-4">
            <h1 className="text-2xl font-bold">{task.title}</h1>
            <div className="flex gap-2">
              <button
                onClick={() => setIsEditing(true)}
                className="px-3 py-1 text-sm text-blue-600 hover:text-blue-800 border border-blue-300 rounded"
              >
                Edit
              </button>
              {isCreator && (
                <button
                  onClick={() => {
                    if (confirm('Are you sure you want to delete this task?')) {
                      deleteMutation.mutate();
                    }
                  }}
                  className="px-3 py-1 text-sm text-red-600 hover:text-red-800 border border-red-300 rounded"
                >
                  Delete
                </button>
              )}
            </div>
          </div>

          <div className="flex gap-2 mb-4">
            <span className={`px-2 py-1 text-xs rounded-full ${statusColors[task.status]}`}>
              {task.status}
            </span>
            <span className={`px-2 py-1 text-xs rounded-full ${priorityColors[task.priority]}`}>
              {task.priority}
            </span>
          </div>

          {task.description && (
            <p className="text-gray-700 mb-4 whitespace-pre-wrap">{task.description}</p>
          )}

          <div className="text-sm text-gray-500 space-y-1">
            <p>Created by {task.createdBy.displayName}</p>
            {task.assignedTo && <p>Assigned to {task.assignedTo.displayName}</p>}
            {task.dueDate && <p>Due: {new Date(task.dueDate).toLocaleDateString()}</p>}
            <p>Created: {new Date(task.createdAt).toLocaleString()}</p>
            <p>Updated: {new Date(task.updatedAt).toLocaleString()}</p>
          </div>
        </div>
      )}

      {/* Comment section */}
      <TaskCommentSection taskId={task.id} comments={task.comments} />
    </div>
  );
}
