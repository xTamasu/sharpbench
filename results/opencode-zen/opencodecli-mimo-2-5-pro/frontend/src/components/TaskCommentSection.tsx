// Task comment section component with add, edit, and delete functionality.

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { commentsApi } from '../api/commentsApi';
import { useAuth } from '../hooks/useAuth';
import type { Comment } from '../types';

const commentSchema = z.object({
  body: z.string().min(1, 'Comment body is required').max(2000, 'Comment must not exceed 2000 characters'),
});

type CommentFormData = z.infer<typeof commentSchema>;

interface TaskCommentSectionProps {
  taskId: string;
  comments: Comment[];
}

export default function TaskCommentSection({ taskId, comments }: TaskCommentSectionProps) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);

  // Add comment form
  const {
    register: registerAdd,
    handleSubmit: handleSubmitAdd,
    reset: resetAdd,
    formState: { errors: errorsAdd },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  // Edit comment form
  const {
    register: registerEdit,
    handleSubmit: handleSubmitEdit,
    reset: resetEdit,
    formState: { errors: errorsEdit },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  const addMutation = useMutation({
    mutationFn: (data: { body: string }) => commentsApi.addComment(taskId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      resetAdd();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: { commentId: string; body: string }) =>
      commentsApi.updateComment(taskId, data.commentId, { body: data.body }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      setEditingCommentId(null);
      resetEdit();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => commentsApi.deleteComment(taskId, commentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
    },
  });

  return (
    <div className="mt-8">
      <h2 className="text-xl font-bold mb-4">Comments</h2>

      {/* Add comment form */}
      <form onSubmit={handleSubmitAdd((data) => addMutation.mutate(data))} className="mb-6">
        <textarea
          {...registerAdd('body')}
          rows={3}
          placeholder="Add a comment..."
          className="w-full px-3 py-2 border border-gray-300 rounded-md"
        />
        {errorsAdd.body && (
          <p className="mt-1 text-sm text-red-600">{errorsAdd.body.message}</p>
        )}
        <button
          type="submit"
          disabled={addMutation.isPending}
          className="mt-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {addMutation.isPending ? 'Adding...' : 'Add Comment'}
        </button>
      </form>

      {/* Comments list */}
      <div className="space-y-4">
        {comments.map((comment) => (
          <div key={comment.id} className="bg-gray-50 rounded-lg p-4">
            {editingCommentId === comment.id ? (
              <form onSubmit={handleSubmitEdit((data) => updateMutation.mutate({ commentId: comment.id, body: data.body }))}>
                <textarea
                  {...registerEdit('body')}
                  defaultValue={comment.body}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                />
                {errorsEdit.body && (
                  <p className="mt-1 text-sm text-red-600">{errorsEdit.body.message}</p>
                )}
                <div className="mt-2 flex gap-2">
                  <button
                    type="submit"
                    disabled={updateMutation.isPending}
                    className="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700"
                  >
                    Save
                  </button>
                  <button
                    type="button"
                    onClick={() => setEditingCommentId(null)}
                    className="px-3 py-1 text-sm text-gray-600 hover:text-gray-800"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            ) : (
              <>
                <div className="flex justify-between items-start">
                  <div>
                    <span className="font-semibold">{comment.author.displayName}</span>
                    <span className="text-sm text-gray-500 ml-2">
                      {new Date(comment.createdAt).toLocaleString()}
                      {comment.editedAt && ' (edited)'}
                    </span>
                  </div>
                  {/* Show edit/delete buttons only for own comments */}
                  {user?.id === comment.author.id && (
                    <div className="flex gap-2">
                      <button
                        onClick={() => setEditingCommentId(comment.id)}
                        className="text-sm text-blue-600 hover:text-blue-800"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => {
                          if (confirm('Delete this comment?')) {
                            deleteMutation.mutate(comment.id);
                          }
                        }}
                        className="text-sm text-red-600 hover:text-red-800"
                      >
                        Delete
                      </button>
                    </div>
                  )}
                </div>
                <p className="mt-2 text-gray-700 whitespace-pre-wrap">{comment.body}</p>
              </>
            )}
          </div>
        ))}

        {comments.length === 0 && (
          <p className="text-gray-500 text-center py-4">No comments yet.</p>
        )}
      </div>
    </div>
  );
}
