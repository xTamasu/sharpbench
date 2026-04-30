// Task detail page: shows full task info and the comment section.
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTask, updateTask, deleteTask } from '../api';
import { TaskItemStatus, TaskPriority } from '../types';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createTaskSchema, type CreateTaskFormData } from '../schemas';
import { useState } from 'react';
import TaskCommentSection from '../components/TaskCommentSection';

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

function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);

  const { data: task, isLoading } = useQuery({
    queryKey: ['task', id],
    queryFn: () => getTask(id!),
    enabled: !!id,
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
    values: task
      ? {
          title: task.title,
          description: task.description || '',
          status: task.status,
          priority: task.priority,
          dueDate: task.dueDate?.split('T')[0] || '',
          assignedToId: task.assignedToId || '',
        }
      : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (data: CreateTaskFormData) =>
      updateTask(id!, {
        title: data.title,
        description: data.description || undefined,
        status: data.status,
        priority: data.priority,
        dueDate: data.dueDate || undefined,
        assignedToId: data.assignedToId || undefined,
      }),
    onSuccess: () => {
      // Invalidate task detail after update
      queryClient.invalidateQueries({ queryKey: ['task', id] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setIsEditing(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteTask(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      navigate('/tasks');
    },
  });

  const currentUser = JSON.parse(localStorage.getItem('user') || '{}');

  if (isLoading) {
    return <p className="p-6 text-gray-500">Loading task...</p>;
  }

  if (!task) {
    return <p className="p-6 text-gray-500">Task not found.</p>;
  }

  return (
    <div className="mx-auto max-w-4xl p-6">
      <button
        onClick={() => navigate('/tasks')}
        className="mb-4 text-sm text-blue-600 hover:underline"
      >
        ← Back to Tasks
      </button>

      {isEditing ? (
        <div className="rounded-lg bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold">Edit Task</h2>
          <form onSubmit={handleSubmit((data) => updateMutation.mutate(data))} className="space-y-4">
            <div>
              <input
                {...register('title')}
                className="w-full rounded-md border border-gray-300 px-3 py-2"
              />
              {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>}
            </div>
            <div>
              <textarea
                {...register('description')}
                rows={4}
                className="w-full rounded-md border border-gray-300 px-3 py-2"
              />
            </div>
            <div className="flex gap-4">
              <select {...register('status', { valueAsNumber: true })} className="rounded-md border border-gray-300 px-3 py-1.5 text-sm">
                <option value={TaskItemStatus.Todo}>Todo</option>
                <option value={TaskItemStatus.InProgress}>In Progress</option>
                <option value={TaskItemStatus.Done}>Done</option>
              </select>
              <select {...register('priority', { valueAsNumber: true })} className="rounded-md border border-gray-300 px-3 py-1.5 text-sm">
                <option value={TaskPriority.Low}>Low</option>
                <option value={TaskPriority.Medium}>Medium</option>
                <option value={TaskPriority.High}>High</option>
              </select>
              <input type="date" {...register('dueDate')} className="rounded-md border border-gray-300 px-3 py-1.5 text-sm" />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700">
                Save
              </button>
              <button type="button" onClick={() => setIsEditing(false)} className="rounded-md bg-gray-200 px-4 py-2 hover:bg-gray-300">
                Cancel
              </button>
            </div>
          </form>
        </div>
      ) : (
        <div className="rounded-lg bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between">
            <h1 className="text-2xl font-bold text-gray-900">{task.title}</h1>
            <div className="flex gap-2">
              <button
                onClick={() => setIsEditing(true)}
                className="rounded bg-blue-100 px-3 py-1 text-sm text-blue-700 hover:bg-blue-200"
              >
                Edit
              </button>
              {task.createdById === currentUser.userId && (
                <button
                  onClick={() => {
                    if (confirm('Delete this task?')) {
                      deleteMutation.mutate();
                    }
                  }}
                  className="rounded bg-red-100 px-3 py-1 text-sm text-red-700 hover:bg-red-200"
                >
                  Delete
                </button>
              )}
            </div>
          </div>

          {task.description && (
            <p className="mt-4 text-gray-700">{task.description}</p>
          )}

          <div className="mt-4 flex flex-wrap gap-3 text-sm">
            <span className="rounded bg-blue-100 px-2 py-1 text-blue-800">
              {statusLabels[task.status]}
            </span>
            <span className="rounded bg-purple-100 px-2 py-1 text-purple-800">
              {priorityLabels[task.priority]}
            </span>
            {task.dueDate && (
              <span className="text-gray-500">
                Due: {new Date(task.dueDate).toLocaleDateString()}
              </span>
            )}
          </div>

          <div className="mt-4 border-t pt-4 text-sm text-gray-500">
            <p>Created by: {task.createdByName}</p>
            {task.assignedToName && <p>Assigned to: {task.assignedToName}</p>}
            <p>Created: {new Date(task.createdAt).toLocaleString()}</p>
            <p>Updated: {new Date(task.updatedAt).toLocaleString()}</p>
          </div>
        </div>
      )}

      {/* Comment section */}
      <TaskCommentSection taskId={id!} comments={task.comments} />
    </div>
  );
}

export default TaskDetailPage;
