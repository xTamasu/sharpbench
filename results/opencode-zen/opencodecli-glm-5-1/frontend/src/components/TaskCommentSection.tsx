// Task comment section with add, edit, and delete functionality for own comments
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createComment, updateComment, deleteComment } from '../api/commentApi';
import type { CommentResponse } from '../types';

const commentSchema = z.object({
  body: z.string().min(1, 'Comment cannot be empty').max(2000, 'Comment must be at most 2000 characters'),
});

type CommentFormData = z.infer<typeof commentSchema>;

interface TaskCommentSectionProps {
  taskId: string;
  comments: CommentResponse[];
  currentUserId: string;
}

export default function TaskCommentSection({ taskId, comments, currentUserId }: TaskCommentSectionProps) {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<string | null>(null);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  const createMutation = useMutation({
    mutationFn: (body: string) => createComment(taskId, { body }),
    onSuccess: () => {
      // Invalidate task query to refresh comments after creating
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      reset();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ commentId, body }: { commentId: string; body: string }) =>
      updateComment(taskId, commentId, { body }),
    onSuccess: () => {
      // Invalidate task query to refresh comments after editing
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      setEditingId(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => deleteComment(taskId, commentId),
    onSuccess: () => {
      // Invalidate task query to refresh comments after deletion
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
    },
  });

  const onCreate = (data: CommentFormData) => {
    createMutation.mutate(data.body);
  };

  const onStartEdit = (commentId: string) => {
    setEditingId(commentId);
  };

  const onSaveEdit = (commentId: string, body: string) => {
    updateMutation.mutate({ commentId, body });
  };

  return (
    <div>
      <h3 className="text-lg font-semibold mb-4">Comments</h3>

      <form onSubmit={handleSubmit(onCreate)} className="mb-6">
        <div>
          <textarea
            {...register('body')}
            placeholder="Write a comment..."
            rows={3}
            className="w-full border rounded px-3 py-2"
          />
          {errors.body && <p className="text-red-500 text-sm">{errors.body.message}</p>}
        </div>
        <button
          type="submit"
          className="mt-2 bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
          disabled={createMutation.isPending}
        >
          {createMutation.isPending ? 'Adding...' : 'Add Comment'}
        </button>
      </form>

      <div className="space-y-4">
        {comments.map((comment: CommentResponse) => (
          <div key={comment.id} className="bg-white p-4 rounded-lg shadow">
            <div className="flex items-center justify-between mb-2">
              <span className="font-medium text-sm">{comment.authorName}</span>
              <span className="text-xs text-gray-500">
                {new Date(comment.createdAt).toLocaleDateString()}
                {comment.editedAt && ' (edited)'}
              </span>
            </div>
            {editingId === comment.id ? (
              <EditCommentForm
                initialBody={comment.body}
                onSave={(body) => onSaveEdit(comment.id, body)}
                onCancel={() => setEditingId(null)}
                isPending={updateMutation.isPending}
              />
            ) : (
              <p className="text-gray-700">{comment.body}</p>
            )}
            {comment.authorId === currentUserId && editingId !== comment.id && (
              <div className="flex gap-2 mt-2">
                <button
                  onClick={() => onStartEdit(comment.id)}
                  className="text-sm text-blue-600 hover:underline"
                >
                  Edit
                </button>
                <button
                  onClick={() => { if (window.confirm('Delete this comment?')) deleteMutation.mutate(comment.id); }}
                  className="text-sm text-red-600 hover:underline"
                >
                  Delete
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

function EditCommentForm({ initialBody, onSave, onCancel, isPending }: {
  initialBody: string;
  onSave: (body: string) => void;
  onCancel: () => void;
  isPending: boolean;
}) {
  const [body, setBody] = useState(initialBody);
  return (
    <div>
      <textarea
        value={body}
        onChange={(e) => setBody(e.target.value)}
        rows={3}
        className="w-full border rounded px-3 py-2"
      />
      <div className="flex gap-2 mt-2">
        <button
          onClick={() => onSave(body)}
          className="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 text-sm"
          disabled={isPending}
        >
          {isPending ? 'Saving...' : 'Save'}
        </button>
        <button
          onClick={onCancel}
          className="bg-gray-300 text-gray-700 px-3 py-1 rounded hover:bg-gray-400 text-sm"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}