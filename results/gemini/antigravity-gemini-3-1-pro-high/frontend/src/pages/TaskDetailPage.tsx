import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../apiClient';
import type { TaskDetail, Status, Priority } from '../types';
import { useParams, useNavigate, Link } from 'react-router-dom';
import TaskCommentSection from '../components/TaskCommentSection';

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const userId = localStorage.getItem('userId');
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({ title: '', description: '', status: 0 as Status, priority: 1 as Priority });

  const { data: task, isLoading } = useQuery({
    queryKey: ['tasks', id],
    queryFn: async () => {
      const res = await apiClient.get<TaskDetail>(`/tasks/${id}`);
      return res.data;
    }
  });

  const updateMutation = useMutation({
    mutationFn: (data: typeof editForm) => apiClient.put(`/tasks/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks', id] });
      setIsEditing(false);
    }
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.delete(`/tasks/${id}`),
    onSuccess: () => navigate('/tasks')
  });

  if (isLoading) return <div className="p-4">Loading...</div>;
  if (!task) return <div className="p-4">Task not found</div>;

  const isCreator = task.createdById === userId;

  return (
    <div className="container mx-auto p-4 max-w-4xl">
      <Link to="/tasks" className="text-blue-500 hover:underline mb-4 inline-block">&larr; Back to Tasks</Link>
      
      <div className="bg-white p-6 rounded shadow mb-6">
        {isEditing ? (
          <div className="space-y-4">
            <input 
              value={editForm.title} 
              onChange={e => setEditForm(prev => ({ ...prev, title: e.target.value }))}
              className="w-full border p-2 rounded" 
            />
            <textarea 
              value={editForm.description} 
              onChange={e => setEditForm(prev => ({ ...prev, description: e.target.value }))}
              className="w-full border p-2 rounded h-32" 
            />
            <div className="flex gap-4">
              <select 
                value={editForm.status} 
                onChange={e => setEditForm(prev => ({ ...prev, status: Number(e.target.value) as Status }))}
                className="border p-2 rounded"
              >
                <option value={0}>Todo</option>
                <option value={1}>In Progress</option>
                <option value={2}>Done</option>
              </select>
              <select 
                value={editForm.priority} 
                onChange={e => setEditForm(prev => ({ ...prev, priority: Number(e.target.value) as Priority }))}
                className="border p-2 rounded"
              >
                <option value={0}>Low</option>
                <option value={1}>Medium</option>
                <option value={2}>High</option>
              </select>
            </div>
            <div className="flex gap-2">
              <button 
                onClick={() => updateMutation.mutate(editForm)}
                className="bg-blue-500 text-white px-4 py-2 rounded"
              >Save</button>
              <button 
                onClick={() => setIsEditing(false)}
                className="bg-gray-300 px-4 py-2 rounded"
              >Cancel</button>
            </div>
          </div>
        ) : (
          <div>
            <div className="flex justify-between items-start">
              <h1 className="text-3xl font-bold mb-2">{task.title}</h1>
              <div className="flex gap-2">
                <button 
                  onClick={() => {
                    setEditForm({ title: task.title, description: task.description || '', status: task.status, priority: task.priority });
                    setIsEditing(true);
                  }}
                  className="text-blue-500 hover:underline"
                >Edit</button>
                {isCreator && (
                  <button 
                    onClick={() => {
                      if(confirm('Are you sure you want to delete this task?')) deleteMutation.mutate();
                    }}
                    className="text-red-500 hover:underline"
                  >Delete</button>
                )}
              </div>
            </div>
            <div className="flex gap-2 mb-4 text-sm">
              <span className={`px-2 py-1 rounded ${task.status === 2 ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}`}>
                {task.status === 0 ? 'Todo' : task.status === 1 ? 'In Progress' : 'Done'}
              </span>
              <span className={`px-2 py-1 rounded ${task.priority === 2 ? 'bg-red-100 text-red-800' : 'bg-blue-100 text-blue-800'}`}>
                Priority: {task.priority === 0 ? 'Low' : task.priority === 1 ? 'Medium' : 'High'}
              </span>
            </div>
            <p className="whitespace-pre-wrap text-gray-700">{task.description}</p>
          </div>
        )}
      </div>

      <TaskCommentSection taskId={task.id} comments={task.comments} />
    </div>
  );
}
