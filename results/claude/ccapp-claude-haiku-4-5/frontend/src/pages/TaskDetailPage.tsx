import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTasks, useComments, useAuth } from '../hooks'
import { UpdateTaskRequest, CreateCommentRequest, TaskStatus, TaskPriority } from '../types'

const commentSchema = z.object({
  body: z.string().min(1, 'Comment is required').max(2000, 'Comment is too long'),
})

type CommentFormData = z.infer<typeof commentSchema>

export const TaskDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [isEditing, setIsEditing] = useState(false)
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null)

  const { getTaskQuery, deleteMutation } = useTasks()
  const taskQuery = getTaskQuery(id || '')
  const { commentsQuery, createMutation, deleteMutation: deleteCommentMutation } = useComments(id)

  const { register, handleSubmit, reset, formState: { errors } } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  })

  const onCommentSubmit = async (data: CommentFormData) => {
    try {
      const request: CreateCommentRequest = { body: data.body }
      await createMutation.mutateAsync(request)
      reset()
    } catch (error) {
      console.error('Failed to create comment:', error)
    }
  }

  const getStatusLabel = (status: number) => {
    return Object.entries(TaskStatus).find(([_, v]) => v === status)?.[0] || 'Unknown'
  }

  const getPriorityLabel = (priority: number) => {
    return Object.entries(TaskPriority).find(([_, v]) => v === priority)?.[0] || 'Unknown'
  }

  if (taskQuery.isLoading) return <p className="text-center p-4">Loading...</p>
  if (taskQuery.isError) return <p className="text-center p-4 text-red-500">Failed to load task</p>
  if (!taskQuery.data) return <p className="text-center p-4">Task not found</p>

  const task = taskQuery.data
  const canEdit = user?.id === task.createdById

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-4xl mx-auto px-4 py-4">
          <button
            onClick={() => navigate('/tasks')}
            className="text-blue-600 hover:text-blue-800 text-sm font-medium"
          >
            ← Back to Tasks
          </button>
        </div>
      </nav>

      <main className="max-w-4xl mx-auto py-6 px-4">
        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="flex justify-between items-start mb-4">
            <h1 className="text-3xl font-bold text-gray-900">{task.title}</h1>
            {canEdit && (
              <div className="flex gap-2">
                <button
                  onClick={() => setIsEditing(!isEditing)}
                  className="px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100"
                >
                  {isEditing ? 'Cancel' : 'Edit'}
                </button>
                <button
                  onClick={async () => {
                    if (window.confirm('Are you sure you want to delete this task?')) {
                      await deleteMutation.mutateAsync(task.id)
                      navigate('/tasks')
                    }
                  }}
                  className="px-4 py-2 text-sm font-medium text-red-600 bg-red-50 rounded-md hover:bg-red-100"
                >
                  Delete
                </button>
              </div>
            )}
          </div>

          {isEditing && canEdit ? (
            <TaskEditForm task={task} onCancel={() => setIsEditing(false)} onSave={() => setIsEditing(false)} />
          ) : (
            <div>
              {task.description && (
                <p className="text-gray-600 mb-4">{task.description}</p>
              )}
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <p className="text-sm text-gray-500">Status</p>
                  <p className="text-lg font-semibold text-gray-900">{getStatusLabel(task.status)}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Priority</p>
                  <p className="text-lg font-semibold text-gray-900">{getPriorityLabel(task.priority)}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Created by</p>
                  <p className="text-lg font-semibold text-gray-900">{task.createdByDisplayName}</p>
                </div>
                {task.assignedToDisplayName && (
                  <div>
                    <p className="text-sm text-gray-500">Assigned to</p>
                    <p className="text-lg font-semibold text-gray-900">{task.assignedToDisplayName}</p>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Comments</h2>

          <form onSubmit={handleSubmit(onCommentSubmit)} className="mb-6 pb-6 border-b">
            <textarea
              {...register('body')}
              placeholder="Add a comment..."
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none"
              rows={3}
            />
            {errors.body && <p className="text-red-500 text-sm mt-1">{errors.body.message}</p>}
            <div className="mt-2">
              <button
                type="submit"
                disabled={createMutation.isPending}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
              >
                {createMutation.isPending ? 'Posting...' : 'Post Comment'}
              </button>
            </div>
          </form>

          {commentsQuery.isLoading && <p className="text-center text-gray-500">Loading comments...</p>}
          {commentsQuery.isError && <p className="text-center text-red-500">Failed to load comments</p>}

          {commentsQuery.data && (
            <div className="space-y-4">
              {commentsQuery.data.length === 0 ? (
                <p className="text-center text-gray-500">No comments yet</p>
              ) : (
                commentsQuery.data.map((comment) => (
                  <div key={comment.id} className="bg-gray-50 rounded-lg p-4">
                    <div className="flex justify-between items-start mb-2">
                      <p className="font-semibold text-gray-900">{comment.authorDisplayName}</p>
                      {user?.id === comment.authorId && (
                        <div className="flex gap-2">
                          <button
                            onClick={() => setEditingCommentId(editingCommentId === comment.id ? null : comment.id)}
                            className="text-blue-600 hover:text-blue-800 text-sm"
                          >
                            {editingCommentId === comment.id ? 'Cancel' : 'Edit'}
                          </button>
                          <button
                            onClick={async () => {
                              if (window.confirm('Delete this comment?')) {
                                await deleteCommentMutation.mutateAsync(comment.id)
                              }
                            }}
                            className="text-red-600 hover:text-red-800 text-sm"
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </div>
                    {editingCommentId === comment.id ? (
                      <CommentEditForm
                        comment={comment}
                        onSave={() => setEditingCommentId(null)}
                        onCancel={() => setEditingCommentId(null)}
                      />
                    ) : (
                      <p className="text-gray-700">{comment.body}</p>
                    )}
                    <p className="text-xs text-gray-500 mt-2">
                      {new Date(comment.createdAt).toLocaleString()}
                      {comment.editedAt && ' (edited)'}
                    </p>
                  </div>
                ))
              )}
            </div>
          )}
        </div>
      </main>
    </div>
  )
}

const TaskEditForm = ({ task, onCancel, onSave }: any) => {
  const { updateMutation } = useTasks()
  const { register, handleSubmit } = useForm({
    defaultValues: {
      title: task.title,
      description: task.description,
      status: task.status.toString(),
      priority: task.priority.toString(),
    },
  })

  const onSubmit = async (data: any) => {
    const request: UpdateTaskRequest = {
      title: data.title,
      description: data.description,
      status: parseInt(data.status),
      priority: parseInt(data.priority),
    }
    await updateMutation.mutateAsync({ id: task.id, request })
    onSave()
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <input
        {...register('title')}
        className="w-full text-3xl font-bold rounded-md border border-gray-300 px-3 py-2"
      />
      <textarea
        {...register('description')}
        className="w-full rounded-md border border-gray-300 px-3 py-2"
        rows={4}
      />
      <div className="grid grid-cols-2 gap-4">
        <select {...register('status')} className="rounded-md border border-gray-300 px-3 py-2">
          <option value="0">Todo</option>
          <option value="1">In Progress</option>
          <option value="2">Done</option>
        </select>
        <select {...register('priority')} className="rounded-md border border-gray-300 px-3 py-2">
          <option value="0">Low</option>
          <option value="1">Medium</option>
          <option value="2">High</option>
        </select>
      </div>
      <div className="flex gap-2">
        <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
          Save
        </button>
        <button type="button" onClick={onCancel} className="px-4 py-2 bg-gray-300 rounded-md hover:bg-gray-400">
          Cancel
        </button>
      </div>
    </form>
  )
}

const CommentEditForm = ({ comment, onSave, onCancel }: any) => {
  const { updateMutation } = useComments(comment.taskId)
  const { register, handleSubmit } = useForm({
    defaultValues: { body: comment.body },
  })

  const onSubmit = async (data: any) => {
    await updateMutation.mutateAsync({
      commentId: comment.id,
      request: { body: data.body },
    })
    onSave()
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-2">
      <textarea
        {...register('body')}
        className="w-full rounded-md border border-gray-300 px-3 py-2"
        rows={3}
      />
      <div className="flex gap-2">
        <button type="submit" className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700">
          Save
        </button>
        <button type="button" onClick={onCancel} className="px-3 py-1 bg-gray-300 text-sm rounded hover:bg-gray-400">
          Cancel
        </button>
      </div>
    </form>
  )
}
