// TaskDetailPage — shows task details and the comment section for a single task.

import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTask, updateTask } from '@/api/services';
import TaskCommentSection from '@/components/TaskCommentSection';
import type { TaskStatus, TaskPriority } from '@/types';

const statusOptions: TaskStatus[] = ['Todo', 'InProgress', 'Done'];
const priorityOptions: TaskPriority[] = ['Low', 'Medium', 'High'];

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: task, isLoading } = useQuery({
    queryKey: ['task', id],
    queryFn: () => getTask(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: { status: TaskStatus; priority: TaskPriority }) =>
      updateTask(id!, {
        title: task!.title,
        description: task!.description ?? undefined,
        status: data.status,
        priority: data.priority,
        dueDate: task!.dueDate ?? undefined,
        assignedToId: task!.assignedToId ?? undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', id] });
    },
  });

  if (isLoading) return <div className="p-8">Loading...</div>;
  if (!task) return <div className="p-8">Task not found.</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <button
            onClick={() => navigate('/tasks')}
            className="text-blue-600 hover:underline"
          >
            &larr; Back to Tasks
          </button>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto px-4 py-6">
        <div className="bg-white p-6 rounded-lg shadow-sm mb-6">
          <h1 className="text-2xl font-bold mb-2">{task.title}</h1>
          {task.description && (
            <p className="text-gray-600 mb-4">{task.description}</p>
          )}

          <div className="grid grid-cols-2 gap-4 mb-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                value={task.status}
                onChange={(e) =>
                  updateMutation.mutate({
                    status: e.target.value as TaskStatus,
                    priority: task.priority,
                  })
                }
                className="border border-gray-300 rounded-md px-3 py-2 w-full"
              >
                {statusOptions.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Priority
              </label>
              <select
                value={task.priority}
                onChange={(e) =>
                  updateMutation.mutate({
                    status: task.status,
                    priority: e.target.value as TaskPriority,
                  })
                }
                className="border border-gray-300 rounded-md px-3 py-2 w-full"
              >
                {priorityOptions.map((p) => (
                  <option key={p} value={p}>
                    {p}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="text-sm text-gray-500">
            {task.dueDate && <p>Due: {new Date(task.dueDate).toLocaleDateString()}</p>}
            <p>Created by: {task.createdByName ?? 'Unknown'}</p>
            <p>Created: {new Date(task.createdAt).toLocaleDateString()}</p>
            <p>Updated: {new Date(task.updatedAt).toLocaleDateString()}</p>
          </div>
        </div>

        <TaskCommentSection taskId={id!} />
      </div>
    </div>
  );
}
