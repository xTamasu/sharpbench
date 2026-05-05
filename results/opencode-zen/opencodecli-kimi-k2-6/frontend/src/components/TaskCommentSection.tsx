import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useCreateComment, useUpdateComment, useDeleteComment } from '../hooks/useTasks'
import { getCurrentUser } from '../hooks/useAuth'
import type { Comment } from '../types'

/**
 * Zod schema for comment validation.
 */
const commentSchema = z.object({
  body: z.string().min(1, 'Comment is required').max(2000, 'Comment must not exceed 2000 characters'),
})

type CommentFormData = z.infer<typeof commentSchema>

/**
 * Component for displaying and managing comments on a task.
 */
interface TaskCommentSectionProps {
  taskId: string
  comments: Comment[]
}

export default function TaskCommentSection({ taskId, comments }: TaskCommentSectionProps) {
  const currentUser = getCurrentUser()
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null)

  const createComment = useCreateComment()
  const updateComment = useUpdateComment()
  const deleteComment = useDeleteComment()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  })

  const onSubmit = (data: CommentFormData) => {
    createComment.mutate(
      { taskId, data },
      {
        onSuccess: () => {
          reset()
        },
      }
    )
  }

  const handleUpdate = (commentId: string, data: CommentFormData) => {
    updateComment.mutate(
      { taskId, commentId, data },
      {
        onSuccess: () => {
          setEditingCommentId(null)
        },
      }
    )
  }

  const handleDelete = (commentId: string) => {
    if (window.confirm('Are you sure you want to delete this comment?')) {
      deleteComment.mutate({ taskId, commentId })
    }
  }

  return (
    <div className="mt-8">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Comments</h3>

      {/* Add comment form */}
      <form onSubmit={handleSubmit(onSubmit)} className="mb-6">
        <div className="mb-2">
          <textarea
            {...register('body')}
            rows={3}
            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border px-3 py-2"
            placeholder="Add a comment..."
          />
          {errors.body && (
            <p className="mt-1 text-sm text-red-600">{errors.body.message}</p>
          )}
        </div>
        <button
          type="submit"
          disabled={createComment.isPending}
          className="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
        >
          {createComment.isPending ? 'Posting...' : 'Post Comment'}
        </button>
      </form>

      {/* Comments list */}
      <div className="space-y-4">
        {comments.length === 0 && (
          <p className="text-gray-500 text-sm">No comments yet.</p>
        )}

        {comments.map((comment) => (
          <CommentItem
            key={comment.id}
            comment={comment}
            isAuthor={currentUser?.id === comment.authorId}
            isEditing={editingCommentId === comment.id}
            onStartEdit={() => setEditingCommentId(comment.id)}
            onCancelEdit={() => setEditingCommentId(null)}
            onUpdate={(data) => handleUpdate(comment.id, data)}
            onDelete={() => handleDelete(comment.id)}
            isUpdating={updateComment.isPending}
          />
        ))}
      </div>
    </div>
  )
}

/**
 * Individual comment item component.
 */
interface CommentItemProps {
  comment: Comment
  isAuthor: boolean
  isEditing: boolean
  onStartEdit: () => void
  onCancelEdit: () => void
  onUpdate: (data: CommentFormData) => void
  onDelete: () => void
  isUpdating: boolean
}

function CommentItem({
  comment,
  isAuthor,
  isEditing,
  onStartEdit,
  onCancelEdit,
  onUpdate,
  onDelete,
  isUpdating,
}: CommentItemProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
    defaultValues: { body: comment.body },
  })

  if (isEditing) {
    return (
      <form onSubmit={handleSubmit(onUpdate)} className="bg-gray-50 p-4 rounded-lg">
        <textarea
          {...register('body')}
          rows={3}
          className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border px-3 py-2"
        />
        {errors.body && (
          <p className="mt-1 text-sm text-red-600">{errors.body.message}</p>
        )}
        <div className="mt-2 flex gap-2">
          <button
            type="submit"
            disabled={isUpdating}
            className="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
          >
            {isUpdating ? 'Saving...' : 'Save'}
          </button>
          <button
            type="button"
            onClick={onCancelEdit}
            className="inline-flex justify-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            Cancel
          </button>
        </div>
      </form>
    )
  }

  return (
    <div className="bg-gray-50 p-4 rounded-lg">
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-2">
          <span className="font-medium text-sm text-gray-900">{comment.authorName}</span>
          <span className="text-xs text-gray-500">
            {new Date(comment.createdAt).toLocaleString()}
          </span>
          {comment.editedAt && (
            <span className="text-xs text-gray-400">(edited)</span>
          )}
        </div>
        {isAuthor && (
          <div className="flex gap-2">
            <button
              onClick={onStartEdit}
              className="text-xs text-blue-600 hover:text-blue-800"
            >
              Edit
            </button>
            <button
              onClick={onDelete}
              className="text-xs text-red-600 hover:text-red-800"
            >
              Delete
            </button>
          </div>
        )}
      </div>
      <p className="text-sm text-gray-700 whitespace-pre-wrap">{comment.body}</p>
    </div>
  )
}
