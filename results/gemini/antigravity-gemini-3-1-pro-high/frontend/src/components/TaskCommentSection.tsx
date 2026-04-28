import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../apiClient';
import type { Comment } from '../types';

interface Props {
  taskId: string;
  comments: Comment[];
}

export default function TaskCommentSection({ taskId, comments }: Props) {
  const queryClient = useQueryClient();
  const userId = localStorage.getItem('userId');
  const [newComment, setNewComment] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editBody, setEditBody] = useState('');

  const addMutation = useMutation({
    mutationFn: (body: string) => apiClient.post(`/tasks/${taskId}/comments`, { body }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks', taskId] });
      setNewComment('');
    }
  });

  const editMutation = useMutation({
    mutationFn: ({ id, body }: { id: string, body: string }) => apiClient.put(`/tasks/${taskId}/comments/${id}`, { body }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks', taskId] });
      setEditingId(null);
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/tasks/${taskId}/comments/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tasks', taskId] })
  });

  return (
    <div className="bg-white p-6 rounded shadow" data-testid="comment-section">
      <h2 className="text-xl font-bold mb-4">Comments</h2>
      
      <div className="space-y-4 mb-6">
        {comments.map(c => (
          <div key={c.id} className="border-b pb-4 last:border-0">
            {editingId === c.id ? (
              <div className="flex gap-2">
                <input 
                  value={editBody} 
                  onChange={e => setEditBody(e.target.value)} 
                  className="flex-1 border p-2 rounded" 
                  data-testid="edit-comment-input"
                />
                <button 
                  onClick={() => editMutation.mutate({ id: c.id, body: editBody })}
                  className="bg-blue-500 text-white px-3 py-1 rounded"
                >Save</button>
                <button 
                  onClick={() => setEditingId(null)}
                  className="bg-gray-300 px-3 py-1 rounded"
                >Cancel</button>
              </div>
            ) : (
              <div>
                <p className="text-gray-800">{c.body}</p>
                <div className="flex justify-between items-center mt-2 text-sm text-gray-500">
                  <span>{new Date(c.createdAt).toLocaleString()}</span>
                  {c.authorId === userId && (
                    <div className="flex gap-2">
                      <button 
                        onClick={() => { setEditingId(c.id); setEditBody(c.body); }}
                        className="text-blue-500 hover:underline"
                      >Edit</button>
                      <button 
                        onClick={() => { if(confirm('Delete comment?')) deleteMutation.mutate(c.id); }}
                        className="text-red-500 hover:underline"
                      >Delete</button>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        ))}
        {comments.length === 0 && <p className="text-gray-500">No comments yet.</p>}
      </div>

      <div className="flex gap-2">
        <input 
          value={newComment} 
          onChange={e => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          className="flex-1 border p-2 rounded"
          data-testid="new-comment-input"
        />
        <button 
          onClick={() => { if(newComment.trim()) addMutation.mutate(newComment); }}
          disabled={!newComment.trim() || addMutation.isPending}
          className="bg-blue-500 text-white px-4 py-2 rounded disabled:opacity-50"
        >
          Post
        </button>
      </div>
    </div>
  );
}
