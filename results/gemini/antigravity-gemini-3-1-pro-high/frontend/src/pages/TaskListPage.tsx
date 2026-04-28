import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../apiClient';
import type { Task, Status, Priority } from '../types';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const taskSchema = z.object({
  title: z.string().min(1).max(200),
  description: z.string().max(5000).optional(),
  priority: z.number(),
});

type TaskForm = z.infer<typeof taskSchema>;

export default function TaskListPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<Status | ''>('');
  const [priorityFilter, setPriorityFilter] = useState<Priority | ''>('');
  const [isCreating, setIsCreating] = useState(false);

  const { register, handleSubmit, reset } = useForm<TaskForm>({
    resolver: zodResolver(taskSchema),
    defaultValues: { priority: 1 }
  });

  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks', statusFilter, priorityFilter],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (statusFilter !== '') params.append('status', statusFilter.toString());
      if (priorityFilter !== '') params.append('priority', priorityFilter.toString());
      const res = await apiClient.get<Task[]>(`/tasks?${params.toString()}`);
      return res.data;
    }
  });

  const createMutation = useMutation({
    mutationFn: (data: TaskForm) => apiClient.post('/tasks', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setIsCreating(false);
      reset();
    }
  });

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    navigate('/login');
  };

  return (
    <div className="container mx-auto p-4 max-w-4xl">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Tasks</h1>
        <button onClick={handleLogout} className="text-red-500 hover:underline">Logout</button>
      </div>

      <div className="bg-white p-4 rounded shadow mb-6 flex gap-4">
        <div>
          <label htmlFor="status" className="mr-2">Status:</label>
          <select 
            id="status"
            value={statusFilter} 
            onChange={e => setStatusFilter(e.target.value === '' ? '' : Number(e.target.value) as Status)}
            className="border p-1 rounded"
          >
            <option value="">All</option>
            <option value="0">Todo</option>
            <option value="1">In Progress</option>
            <option value="2">Done</option>
          </select>
        </div>
        <div>
          <label htmlFor="priority" className="mr-2">Priority:</label>
          <select 
            id="priority"
            value={priorityFilter} 
            onChange={e => setPriorityFilter(e.target.value === '' ? '' : Number(e.target.value) as Priority)}
            className="border p-1 rounded"
          >
            <option value="">All</option>
            <option value="0">Low</option>
            <option value="1">Medium</option>
            <option value="2">High</option>
          </select>
        </div>
        <button 
          onClick={() => setIsCreating(!isCreating)}
          className="ml-auto bg-green-500 text-white px-4 py-1 rounded hover:bg-green-600"
        >
          {isCreating ? 'Cancel' : 'New Task'}
        </button>
      </div>

      {isCreating && (
        <form onSubmit={handleSubmit((data) => createMutation.mutate(data))} className="bg-gray-50 p-4 rounded shadow mb-6">
          <input {...register('title')} placeholder="Title" className="w-full border p-2 mb-2 rounded" />
          <textarea {...register('description')} placeholder="Description" className="w-full border p-2 mb-2 rounded" />
          <select {...register('priority', { valueAsNumber: true })} className="border p-2 mb-2 rounded mr-2">
            <option value={0}>Low</option>
            <option value={1}>Medium</option>
            <option value={2}>High</option>
          </select>
          <button type="submit" disabled={createMutation.isPending} className="bg-blue-500 text-white px-4 py-2 rounded">
            Save
          </button>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <div className="grid gap-4">
          {tasks?.map(task => (
            <Link to={`/tasks/${task.id}`} key={task.id} className="block bg-white p-4 rounded shadow hover:shadow-md transition">
              <h3 className="text-xl font-semibold">{task.title}</h3>
              <div className="flex gap-2 mt-2 text-sm text-gray-600">
                <span className={`px-2 py-1 rounded ${task.status === 2 ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}`}>
                  {task.status === 0 ? 'Todo' : task.status === 1 ? 'In Progress' : 'Done'}
                </span>
                <span className={`px-2 py-1 rounded ${task.priority === 2 ? 'bg-red-100 text-red-800' : 'bg-blue-100 text-blue-800'}`}>
                  Priority: {task.priority === 0 ? 'Low' : task.priority === 1 ? 'Medium' : 'High'}
                </span>
              </div>
            </Link>
          ))}
          {tasks?.length === 0 && <p className="text-gray-500">No tasks found.</p>}
        </div>
      )}
    </div>
  );
}
