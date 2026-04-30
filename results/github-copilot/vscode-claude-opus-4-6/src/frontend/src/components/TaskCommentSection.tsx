// TaskCommentSection component: displays comments and allows adding/editing/deleting own comments.
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createComment, updateComment, deleteComment } from '../api';
import { commentSchema, type CommentFormData } from '../schemas';
import type { CommentResponse } from '../types';

interface TaskCommentSectionProps {
  taskId: string;
  comments: CommentResponse[];
}

function TaskCommentSection({ taskId, comments }: TaskCommentSectionProps) {
  const queryClient = useQueryClient();
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const currentUser = JSON.parse(localStorage.getItem('user') || '{}');

  // Form for new comments
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  // Form for editing comments
  const {
    register: registerEdit,
    handleSubmit: handleEditSubmit,
    setValue: setEditValue,
    formState: { errors: editErrors },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  const createMutation = useMutation({
    mutationFn: (data: CommentFormData) => createComment(taskId, data),
    onSuccess: () => {
      // Invalidate task query to refresh comments
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      reset();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ commentId, data }: { commentId: string; data: CommentFormData }) =>
      updateComment(taskId, commentId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      setEditingCommentId(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => deleteComment(taskId, commentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
    },
  });

  const startEditing = (comment: CommentResponse) => {
    setEditingCommentId(comment.id);
    setEditValue('body', comment.body);
  };

  return (
    <div className="mt-6 rounded-lg bg-white p-6 shadow-sm">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">Comments</h2>

      {/* Comment list */}
      {comments.length === 0 ? (
        <p className="mb-4 text-sm text-gray-500">No comments yet.</p>
      ) : (
        <div className="mb-6 space-y-4">
          {comments.map((comment) => (
            <div key={comment.id} className="border-b pb-3 last:border-b-0">
              {editingCommentId === comment.id ? (
                <form
                  onSubmit={handleEditSubmit((data) =>
                    updateMutation.mutate({ commentId: comment.id, data })
                  )}
                  className="space-y-2"
                >
                  <textarea
                    {...registerEdit('body')}
                    rows={2}
                    className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                  />
                  {editErrors.body && (
                    <p className="text-sm text-red-600">{editErrors.body.message}</p>
                  )}
                  <div className="flex gap-2">
                    <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700">
                      Save
                    </button>
                    <button type="button" onClick={() => setEditingCommentId(null)} className="rounded bg-gray-200 px-3 py-1 text-xs hover:bg-gray-300">
                      Cancel
                    </button>
                  </div>
                </form>
              ) : (
                <>
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-900">
                      {comment.authorName}
                    </span>
                    <div className="flex items-center gap-2">
                      {comment.editedAt && (
                        <span className="text-xs text-gray-400">(edited)</span>
                      )}
                      <span className="text-xs text-gray-400">
                        {new Date(comment.createdAt).toLocaleString()}
                      </span>
                      {/* Only show edit/delete for own comments */}
                      {comment.authorId === currentUser.userId && (
                        <div className="flex gap-1">
                          <button
                            onClick={() => startEditing(comment)}
                            className="rounded px-2 py-0.5 text-xs text-blue-600 hover:bg-blue-50"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => {
                              if (confirm('Delete this comment?')) {
                                deleteMutation.mutate(comment.id);
                              }
                            }}
                            className="rounded px-2 py-0.5 text-xs text-red-600 hover:bg-red-50"
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                  <p className="mt-1 text-sm text-gray-700">{comment.body}</p>
                </>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Add new comment form */}
      <form onSubmit={handleSubmit((data) => createMutation.mutate(data))} className="space-y-3">
        <textarea
          {...register('body')}
          placeholder="Add a comment..."
          rows={3}
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
        />
        {errors.body && <p className="text-sm text-red-600">{errors.body.message}</p>}
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {createMutation.isPending ? 'Posting...' : 'Add Comment'}
        </button>
      </form>
    </div>
  );
}

export default TaskCommentSection;
