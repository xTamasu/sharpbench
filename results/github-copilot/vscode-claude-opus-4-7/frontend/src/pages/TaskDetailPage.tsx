// TaskDetailPage.tsx
// Shows a single task's details and its comment thread. The creator can delete
// the task; deletion redirects back to the list and invalidates the cache.
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { deleteTask, getTask } from '../api/tasks';
import TaskCommentSection from '../components/TaskCommentSection';
import { useAuth } from '../auth/AuthContext';

export default function TaskDetailPage() {
  const { id = '' } = useParams<{ id: string }>();
  const { user } = useAuth();
  const qc = useQueryClient();
  const navigate = useNavigate();

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['task', id],
    queryFn: () => getTask(id),
    enabled: !!id,
  });

  const deleteM = useMutation({
    mutationFn: () => deleteTask(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      navigate('/tasks', { replace: true });
    },
  });

  if (isLoading) return <p>Loading…</p>;
  if (isError) return <p role="alert" className="text-red-600">{(error as Error).message}</p>;
  if (!data) return null;

  const isCreator = data.createdBy.id === user?.id;

  return (
    <div>
      <div className="mb-4">
        <Link to="/tasks" className="text-sm text-blue-600 underline">← Back to tasks</Link>
      </div>
      <header className="bg-white border border-slate-200 rounded p-4 mb-4">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-semibold">{data.title}</h1>
            <p className="text-xs text-slate-500 mt-1">
              {data.status} · {data.priority} · created by {data.createdBy.displayName}
            </p>
          </div>
          {isCreator && (
            <button
              type="button"
              onClick={() => deleteM.mutate()}
              disabled={deleteM.isPending}
              className="border border-red-300 text-red-700 rounded px-3 py-1 text-sm disabled:opacity-60"
            >
              {deleteM.isPending ? 'Deleting…' : 'Delete task'}
            </button>
          )}
        </div>
        {data.description && <p className="mt-3 whitespace-pre-wrap text-sm">{data.description}</p>}
        {deleteM.isError && (
          <p role="alert" className="text-sm text-red-600 mt-2">{(deleteM.error as Error).message}</p>
        )}
      </header>

      <TaskCommentSection taskId={data.id} comments={data.comments} />
    </div>
  );
}
