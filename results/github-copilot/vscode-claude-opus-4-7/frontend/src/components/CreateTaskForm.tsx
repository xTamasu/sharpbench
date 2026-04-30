// CreateTaskForm.tsx — inline form for creating a new task. On success the
// "tasks" query is invalidated so the list refreshes.
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createTask } from '../api/tasks';
import type { TaskPriority, TaskStatus } from '../types';

const schema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().max(5000).optional(),
  status: z.enum(['Todo', 'InProgress', 'Done']),
  priority: z.enum(['Low', 'Medium', 'High']),
});
type FormValues = z.infer<typeof schema>;

export default function CreateTaskForm() {
  const qc = useQueryClient();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { status: 'Todo' as TaskStatus, priority: 'Medium' as TaskPriority },
  });

  const mutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      // Invalidate every "tasks" query (independent of filter args).
      qc.invalidateQueries({ queryKey: ['tasks'] });
      reset({ title: '', description: '', status: 'Todo', priority: 'Medium' });
    },
  });

  return (
    <form
      onSubmit={handleSubmit((v) => mutation.mutate({ ...v, description: v.description ?? null }))}
      className="bg-white border border-slate-200 rounded p-4 space-y-3 mb-6"
      noValidate
    >
      <h2 className="font-semibold">Create task</h2>
      <div>
        <label htmlFor="title" className="block text-sm font-medium mb-1">Title</label>
        <input id="title" className="w-full border border-slate-300 rounded px-2 py-1.5" {...register('title')} />
        {errors.title && <p role="alert" className="text-sm text-red-600 mt-1">{errors.title.message}</p>}
      </div>
      <div>
        <label htmlFor="description" className="block text-sm font-medium mb-1">Description</label>
        <textarea id="description" rows={3} className="w-full border border-slate-300 rounded px-2 py-1.5" {...register('description')} />
        {errors.description && <p role="alert" className="text-sm text-red-600 mt-1">{errors.description.message}</p>}
      </div>
      <div className="flex gap-3">
        <label className="text-sm">
          <span className="block mb-1 font-medium">Status</span>
          <select className="border border-slate-300 rounded px-2 py-1" {...register('status')}>
            <option value="Todo">Todo</option>
            <option value="InProgress">InProgress</option>
            <option value="Done">Done</option>
          </select>
        </label>
        <label className="text-sm">
          <span className="block mb-1 font-medium">Priority</span>
          <select className="border border-slate-300 rounded px-2 py-1" {...register('priority')}>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </label>
      </div>

      {mutation.isError && (
        <p role="alert" className="text-sm text-red-600">{(mutation.error as Error).message}</p>
      )}

      <button
        type="submit"
        disabled={mutation.isPending}
        className="bg-slate-900 text-white rounded px-4 py-1.5 disabled:opacity-60"
      >
        {mutation.isPending ? 'Creating…' : 'Create task'}
      </button>
    </form>
  );
}
