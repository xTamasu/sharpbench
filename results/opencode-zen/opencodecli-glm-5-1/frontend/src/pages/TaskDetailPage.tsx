// Task detail page with inline edit and comment section
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '../auth/AuthContext';
import { fetchTaskById, updateTask, deleteTask } from '../api/taskApi';
import TaskCommentSection from '../components/TaskCommentSection';

const updateTaskSchema = z.object({
  title: z.string().max(200, 'Title must be at most 200 characters').optional(),
  description: z.string().max(5000, 'Description must be at most 5000 characters').optional().nullable(),
  status: z.enum(['Todo', 'InProgress', 'Done']).optional(),
  priority: z.enum(['Low', 'Medium', 'High']).optional(),
});

type UpdateTaskFormData = z.infer<typeof updateTaskSchema>;

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

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { userId, logout } = useAuth();
  const queryClient = useQueryClient();

  const { data: task, isLoading } = useQuery({
    queryKey: ['task', id],
    queryFn: () => fetchTaskById(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: Parameters<typeof updateTask>[1]) => updateTask(id!, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['task', id] }),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteTask(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      window.location.href = '/tasks';
    },
  });

  const { register, handleSubmit } = useForm<UpdateTaskFormData>({
    resolver: zodResolver(updateTaskSchema),
  });

  const onUpdate = (data: UpdateTaskFormData) => {
    updateMutation.mutate({
      title: data.title,
      description: data.description ?? undefined,
      status: data.status,
      priority: data.priority,
      dueDate: undefined,
      assignedToId: undefined,
    });
  };

  if (isLoading) return <div className="text-center py-10">Loading...</div>;
  if (!task) return <div className="text-center py-10">Task not found</div>;

  const isCreator = task.createdById === userId;

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow px-6 py-4 flex items-center justify-between">
        <Link to="/tasks" className="text-blue-600 hover:underline">← Back to Tasks</Link>
        <button
          onClick={() => { logout(); window.location.href = '/login'; }}
          className="text-sm text-red-600 hover:underline"
        >
          Logout
        </button>
      </nav>

      <div className="max-w-2xl mx-auto px-4 py-6">
        <div className="bg-white p-6 rounded-lg shadow">
          <form onSubmit={handleSubmit(onUpdate)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Title</label>
              <input
                {...register('title')}
                defaultValue={task.title}
                className="mt-1 block w-full border rounded px-3 py-2"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Description</label>
              <textarea
                {...register('description')}
                defaultValue={task.description ?? ''}
                rows={4}
                className="mt-1 block w-full border rounded px-3 py-2"
              />
            </div>
            <div className="flex gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700">Status</label>
                <select {...register('status')} defaultValue={task.status} className="mt-1 border rounded px-3 py-2">
                  <option value="Todo">Todo</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Priority</label>
                <select {...register('priority')} defaultValue={task.priority} className="mt-1 border rounded px-3 py-2">
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                </select>
              </div>
            </div>
            <div className="flex gap-2">
              <span className={`px-2 py-0.5 rounded text-xs font-medium ${statusColors[task.status] || 'bg-gray-200'}`}>
                {task.status}
              </span>
              <span className={`px-2 py-0.5 rounded text-xs font-medium ${priorityColors[task.priority] || 'bg-gray-100'}`}>
                {task.priority}
              </span>
              <span className="text-xs text-gray-500">
                Created by {task.createdByName}
                {task.assignedToName && ` · Assigned to ${task.assignedToName}`}
              </span>
            </div>
            <div className="flex gap-2">
              <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700" disabled={updateMutation.isPending}>
                {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
              </button>
              {isCreator && (
                <button
                  type="button"
                  onClick={() => { if (window.confirm('Delete this task?')) deleteMutation.mutate(); }}
                  className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
                  disabled={deleteMutation.isPending}
                >
                  Delete Task
                </button>
              )}
            </div>
          </form>
        </div>

        <div className="mt-6">
          <TaskCommentSection taskId={task.id} comments={task.comments} currentUserId={userId ?? ''} />
        </div>
      </div>
    </div>
  );
}