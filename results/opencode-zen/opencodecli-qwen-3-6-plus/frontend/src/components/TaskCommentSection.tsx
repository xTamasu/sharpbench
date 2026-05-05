// TaskCommentSection — renders comments for a task with add, edit, and delete functionality.

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { getComments, createComment, updateComment, deleteComment } from '@/api/services';
import { commentSchema } from '@/schemas';
import type { Comment } from '@/types';
import { z } from 'zod';

type CommentFormData = z.infer<typeof commentSchema>;

export default function TaskCommentSection({ taskId }: { taskId: string }) {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editBody, setEditBody] = useState('');
  const currentUserId = localStorage.getItem('userId') ?? '';

  const { data: comments, isLoading } = useQuery({
    queryKey: ['comments', taskId],
    queryFn: () => getComments(taskId),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  const addMutation = useMutation({
    mutationFn: (data: CommentFormData) => createComment(taskId, data),
    onSuccess: () => {
      // Invalidate comments query to refetch after adding a new comment
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] });
      reset();
    },
  });

  const editMutation = useMutation({
    mutationFn: (data: { commentId: string; body: string }) =>
      updateComment(taskId, data.commentId, { body: data.body }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] });
      setEditingId(null);
      setEditBody('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => deleteComment(taskId, commentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] });
    },
  });

  const onSubmit = (data: CommentFormData) => {
    addMutation.mutate(data);
  };

  const startEdit = (comment: Comment) => {
    setEditingId(comment.id);
    setEditBody(comment.body);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditBody('');
  };

  const saveEdit = (commentId: string) => {
    if (editBody.trim()) {
      editMutation.mutate({ commentId, body: editBody });
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-sm">
      <h2 className="text-xl font-bold mb-4">Comments</h2>

      {/* Add Comment Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="mb-6">
        <textarea
          {...register('body')}
          className="w-full border border-gray-300 rounded-md px-3 py-2 resize-none"
          rows={3}
          placeholder="Write a comment..."
        />
        {errors.body && (
          <p className="text-red-500 text-sm mt-1">{errors.body.message}</p>
        )}
        <button
          type="submit"
          className="mt-2 bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
        >
          Add Comment
        </button>
      </form>

      {/* Comments List */}
      {isLoading ? (
        <p className="text-gray-500">Loading comments...</p>
      ) : comments && comments.length > 0 ? (
        <div className="space-y-4">
          {comments.map((comment: Comment) => (
            <div key={comment.id} className="border-b border-gray-200 pb-4 last:border-0">
              <div className="flex justify-between items-start">
                <div>
                  <span className="font-medium text-sm">{comment.authorName}</span>
                  <span className="text-gray-400 text-sm ml-2">
                    {new Date(comment.createdAt).toLocaleDateString()}
                  </span>
                  {comment.editedAt && (
                    <span className="text-gray-400 text-sm ml-2">(edited)</span>
                  )}
                </div>
                {comment.authorId === currentUserId && (
                  <div className="flex gap-2">
                    <button
                      onClick={() => startEdit(comment)}
                      className="text-blue-600 text-sm hover:underline"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => deleteMutation.mutate(comment.id)}
                      className="text-red-600 text-sm hover:underline"
                    >
                      Delete
                    </button>
                  </div>
                )}
              </div>

              {editingId === comment.id ? (
                <div className="mt-2">
                  <textarea
                    value={editBody}
                    onChange={(e) => setEditBody(e.target.value)}
                    className="w-full border border-gray-300 rounded-md px-3 py-2 resize-none"
                    rows={2}
                  />
                  <div className="flex gap-2 mt-2">
                    <button
                      onClick={() => saveEdit(comment.id)}
                      className="bg-blue-600 text-white px-3 py-1 rounded text-sm hover:bg-blue-700"
                    >
                      Save
                    </button>
                    <button
                      onClick={cancelEdit}
                      className="text-gray-600 px-3 py-1 text-sm hover:underline"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <p className="text-gray-700 mt-1">{comment.body}</p>
              )}
            </div>
          ))}
        </div>
      ) : (
        <p className="text-gray-500">No comments yet.</p>
      )}
    </div>
  );
}
