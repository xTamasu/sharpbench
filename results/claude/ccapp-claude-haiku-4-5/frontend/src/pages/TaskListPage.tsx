import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTasks, useAuth } from '../hooks'
import { CreateTaskRequest, TaskStatus, TaskPriority } from '../types'

const createTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title is too long'),
  description: z.string().max(5000, 'Description is too long').optional(),
  priority: z.coerce.number().min(0).max(2),
  dueDate: z.string().optional(),
})

type CreateTaskFormData = z.infer<typeof createTaskSchema>

export const TaskListPage = () => {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const [statusFilter, setStatusFilter] = useState<string>()
  const [priorityFilter, setPriorityFilter] = useState<string>()
  const [showCreateForm, setShowCreateForm] = useState(false)

  const { tasksQuery, createMutation } = useTasks(statusFilter, priorityFilter)
  const { register, handleSubmit, reset, formState: { errors } } = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
  })

  const onCreateSubmit = async (data: CreateTaskFormData) => {
    try {
      const request: CreateTaskRequest = {
        title: data.title,
        description: data.description,
        priority: data.priority,
        dueDate: data.dueDate ? new Date(data.dueDate).toISOString() : undefined,
      }
      await createMutation.mutateAsync(request)
      reset()
      setShowCreateForm(false)
    } catch (error) {
      console.error('Failed to create task:', error)
    }
  }

  const getStatusLabel = (status: number) => {
    return Object.entries(TaskStatus).find(([_, v]) => v === status)?.[0] || 'Unknown'
  }

  const getPriorityLabel = (priority: number) => {
    return Object.entries(TaskPriority).find(([_, v]) => v === priority)?.[0] || 'Unknown'
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <h1 className="text-2xl font-bold text-gray-900">Task Manager</h1>
          <button
            onClick={logout}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Logout
          </button>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-semibold text-gray-900">Tasks</h2>
          <button
            onClick={() => setShowCreateForm(!showCreateForm)}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
          >
            New Task
          </button>
        </div>

        {showCreateForm && (
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <form onSubmit={handleSubmit(onCreateSubmit)} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Title</label>
                <input
                  {...register('title')}
                  type="text"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none"
                />
                {errors.title && <p className="text-red-500 text-sm mt-1">{errors.title.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  {...register('description')}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none"
                  rows={4}
                />
                {errors.description && <p className="text-red-500 text-sm mt-1">{errors.description.message}</p>}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Priority</label>
                  <select
                    {...register('priority')}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="0">Low</option>
                    <option value="1">Medium</option>
                    <option value="2">High</option>
                  </select>
                  {errors.priority && <p className="text-red-500 text-sm mt-1">{errors.priority.message}</p>}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Due Date</label>
                  <input
                    {...register('dueDate')}
                    type="datetime-local"
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none"
                  />
                  {errors.dueDate && <p className="text-red-500 text-sm mt-1">{errors.dueDate.message}</p>}
                </div>
              </div>

              <div className="flex gap-2">
                <button
                  type="submit"
                  disabled={createMutation.isPending}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
                >
                  {createMutation.isPending ? 'Creating...' : 'Create Task'}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowCreateForm(false)
                    reset()
                  }}
                  className="px-4 py-2 bg-gray-300 text-gray-900 rounded-md hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Filter by Status</label>
              <select
                value={statusFilter || ''}
                onChange={(e) => setStatusFilter(e.target.value || undefined)}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900"
              >
                <option value="">All</option>
                <option value="0">Todo</option>
                <option value="1">In Progress</option>
                <option value="2">Done</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Filter by Priority</label>
              <select
                value={priorityFilter || ''}
                onChange={(e) => setPriorityFilter(e.target.value || undefined)}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-gray-900"
              >
                <option value="">All</option>
                <option value="0">Low</option>
                <option value="1">Medium</option>
                <option value="2">High</option>
              </select>
            </div>
          </div>
        </div>

        {tasksQuery.isLoading && <p className="text-center text-gray-500">Loading tasks...</p>}
        {tasksQuery.isError && <p className="text-center text-red-500">Failed to load tasks</p>}

        {tasksQuery.data && (
          <div className="space-y-4">
            {tasksQuery.data.length === 0 ? (
              <p className="text-center text-gray-500">No tasks found</p>
            ) : (
              tasksQuery.data.map((task) => (
                <div
                  key={task.id}
                  onClick={() => navigate(`/tasks/${task.id}`)}
                  className="bg-white rounded-lg shadow-sm p-6 cursor-pointer hover:shadow-md transition-shadow"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <h3 className="text-lg font-semibold text-gray-900">{task.title}</h3>
                      {task.description && <p className="text-gray-600 text-sm mt-1">{task.description}</p>}
                    </div>
                    <div className="flex gap-2 ml-4">
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">
                        {getStatusLabel(task.status)}
                      </span>
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
                        {getPriorityLabel(task.priority)}
                      </span>
                    </div>
                  </div>
                  <div className="mt-4 flex justify-between items-center text-sm text-gray-500">
                    <span>Created by: {task.createdByDisplayName}</span>
                    {task.assignedToDisplayName && <span>Assigned to: {task.assignedToDisplayName}</span>}
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </main>
    </div>
  )
}
