// TaskList.tsx
// Renders a filterable list of tasks. Filter state lives in this component
// and is passed to React Query as part of the query key.
import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { listTasks, type TaskFilters } from '../api/tasks';
import type { TaskItem, TaskPriority, TaskStatus } from '../types';

const STATUS_OPTIONS: TaskStatus[] = ['Todo', 'InProgress', 'Done'];
const PRIORITY_OPTIONS: TaskPriority[] = ['Low', 'Medium', 'High'];

export interface TaskListProps {
  /** Optional initial filters (mostly used by tests). */
  initialFilters?: TaskFilters;
}

export default function TaskList({ initialFilters = {} }: TaskListProps) {
  const [status, setStatus] = useState<TaskStatus | ''>(initialFilters.status ?? '');
  const [priority, setPriority] = useState<TaskPriority | ''>(initialFilters.priority ?? '');

  const filters = useMemo<TaskFilters>(
    () => ({ status: status || undefined, priority: priority || undefined }),
    [status, priority],
  );

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['tasks', filters],
    queryFn: () => listTasks(filters),
  });

  return (
    <div>
      <div className="flex flex-wrap gap-3 items-end mb-4" data-testid="task-filter-bar">
        <label className="text-sm">
          <span className="block mb-1 font-medium">Status</span>
          <select
            aria-label="Filter by status"
            value={status}
            onChange={(e) => setStatus(e.target.value as TaskStatus | '')}
            className="border border-slate-300 rounded px-2 py-1"
          >
            <option value="">All</option>
            {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
        </label>

        <label className="text-sm">
          <span className="block mb-1 font-medium">Priority</span>
          <select
            aria-label="Filter by priority"
            value={priority}
            onChange={(e) => setPriority(e.target.value as TaskPriority | '')}
            className="border border-slate-300 rounded px-2 py-1"
          >
            <option value="">All</option>
            {PRIORITY_OPTIONS.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
        </label>
      </div>

      {isLoading && <p>Loading tasks…</p>}
      {isError && <p role="alert" className="text-red-600">{(error as Error).message}</p>}

      {data && data.length === 0 && !isLoading && (
        <p className="text-slate-600">No tasks match the current filters.</p>
      )}

      <ul className="divide-y divide-slate-200 border border-slate-200 rounded bg-white" data-testid="task-list">
        {data?.map((t: TaskItem) => (
          <li key={t.id} className="p-3 hover:bg-slate-50">
            <Link to={`/tasks/${t.id}`} className="block">
              <div className="flex items-center justify-between">
                <span className="font-medium">{t.title}</span>
                <span className="text-xs text-slate-500">{t.status} · {t.priority}</span>
              </div>
              {t.description && <p className="text-sm text-slate-600 mt-1 truncate">{t.description}</p>}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
