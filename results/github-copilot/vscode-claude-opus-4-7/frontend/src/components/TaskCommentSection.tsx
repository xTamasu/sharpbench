// TaskCommentSection.tsx
// Renders the comment thread for a task and an inline form to add new ones.
// Comments authored by the current user expose Edit / Delete buttons.
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { addComment, deleteComment, updateComment } from '../api/comments';
import type { Comment } from '../types';
import { useAuth } from '../auth/AuthContext';

const schema = z.object({
  body: z.string().min(1, 'Comment cannot be empty').max(2000, 'Max 2000 characters'),
});
type FormValues = z.infer<typeof schema>;

export interface TaskCommentSectionProps {
  taskId: string;
  comments: Comment[];
}

export default function TaskCommentSection({ taskId, comments }: TaskCommentSectionProps) {
  const { user } = useAuth();
  const qc = useQueryClient();

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { body: '' },
  });

  const addM = useMutation({
    mutationFn: (body: string) => addComment(taskId, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['task', taskId] });
      reset({ body: '' });
    },
  });

  return (
    <section className="mt-6">
      <h2 className="font-semibold mb-2">Comments</h2>

      <ul className="space-y-3 mb-4" data-testid="comment-list">
        {comments.length === 0 && <li className="text-sm text-slate-600">No comments yet.</li>}
        {comments.map((c) => (
          <CommentRow key={c.id} comment={c} taskId={taskId} canManage={c.author.id === user?.id} />
        ))}
      </ul>

      <form
        onSubmit={handleSubmit((v) => addM.mutate(v.body))}
        className="space-y-2"
        aria-label="Add comment"
        noValidate
      >
        <label htmlFor="comment-body" className="block text-sm font-medium">Add a comment</label>
        <textarea
          id="comment-body"
          rows={3}
          className="w-full border border-slate-300 rounded px-2 py-1.5"
          {...register('body')}
        />
        {errors.body && <p role="alert" className="text-sm text-red-600">{errors.body.message}</p>}
        {addM.isError && (
          <p role="alert" className="text-sm text-red-600">{(addM.error as Error).message}</p>
        )}
        <button
          type="submit"
          disabled={addM.isPending}
          className="bg-slate-900 text-white rounded px-4 py-1.5 disabled:opacity-60"
        >
          {addM.isPending ? 'Posting…' : 'Post comment'}
        </button>
      </form>
    </section>
  );
}

interface CommentRowProps { comment: Comment; taskId: string; canManage: boolean; }

function CommentRow({ comment, taskId, canManage }: CommentRowProps) {
  const qc = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(comment.body);

  const editM = useMutation({
    mutationFn: (body: string) => updateComment(taskId, comment.id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['task', taskId] });
      setIsEditing(false);
    },
  });

  const deleteM = useMutation({
    mutationFn: () => deleteComment(taskId, comment.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['task', taskId] }),
  });

  return (
    <li className="border border-slate-200 rounded p-3 bg-white" data-testid="comment-item">
      <div className="flex items-center justify-between text-xs text-slate-500 mb-1">
        <span>
          <span className="font-medium text-slate-700">{comment.author.displayName}</span>
          {' · '}
          {new Date(comment.createdAt).toLocaleString()}
          {comment.editedAt && <span title={new Date(comment.editedAt).toLocaleString()}> (edited)</span>}
        </span>
        {canManage && !isEditing && (
          <span className="flex gap-2">
            <button
              type="button"
              onClick={() => { setDraft(comment.body); setIsEditing(true); }}
              className="text-blue-600 hover:underline"
            >
              Edit
            </button>
            <button
              type="button"
              onClick={() => deleteM.mutate()}
              disabled={deleteM.isPending}
              className="text-red-600 hover:underline disabled:opacity-60"
            >
              Delete
            </button>
          </span>
        )}
      </div>

      {isEditing ? (
        <div className="space-y-2">
          <textarea
            aria-label="Edit comment"
            rows={3}
            className="w-full border border-slate-300 rounded px-2 py-1.5"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
          />
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => editM.mutate(draft)}
              disabled={editM.isPending || draft.trim().length === 0}
              className="bg-slate-900 text-white rounded px-3 py-1 disabled:opacity-60"
            >
              Save
            </button>
            <button
              type="button"
              onClick={() => setIsEditing(false)}
              className="border border-slate-300 rounded px-3 py-1"
            >
              Cancel
            </button>
          </div>
        </div>
      ) : (
        <p className="text-sm whitespace-pre-wrap">{comment.body}</p>
      )}
    </li>
  );
}
