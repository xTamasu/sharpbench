import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useTasks, useDeleteTask } from '../hooks/useTasks'
import { logout, getCurrentUser } from '../hooks/useAuth'
import TaskFilterBar from '../components/TaskFilterBar'
import type { TaskFilter, Task } from '../types'

/**
 * Page component for displaying the task list with filtering capabilities.
 */
export default function TaskListPage() {
  const navigate = useNavigate()
  const currentUser = getCurrentUser()
  const [filter, setFilter] = useState<TaskFilter>({})

  const { data: tasks, isLoading, error } = useTasks(filter)
  const deleteTask = useDeleteTask()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High':
        return 'bg-red-100 text-red-800'
      case 'Medium':
        return 'bg-yellow-100 text-yellow-800'
      case 'Low':
        return 'bg-green-100 text-green-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Done':
        return 'bg-green-100 text-green-800'
      case 'InProgress':
        return 'bg-blue-100 text-blue-800'
      case 'Todo':
        return 'bg-gray-100 text-gray-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Tasks</h1>
          <p className="text-sm text-gray-600 mt-1">Welcome, {currentUser?.displayName}</p>
        </div>
        <div className="flex gap-4">
          <Link
            to="/tasks/new"
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            New Task
          </Link>
          <button
            onClick={handleLogout}
            className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Logout
          </button>
        </div>
      </div>

      <TaskFilterBar filter={filter} onFilterChange={setFilter} />

      {isLoading && (
        <div className="text-center py-12">
          <p className="text-gray-500">Loading tasks...</p>
        </div>
      )}

      {error && (
        <div className="rounded-md bg-red-50 p-4 mb-6">
          <p className="text-sm text-red-800">Failed to load tasks. Please try again.</p>
        </div>
      )}

      {!isLoading && tasks && tasks.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-500">No tasks found.</p>
        </div>
      )}

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {tasks?.map((task: Task) => (
          <div
            key={task.id}
            className="bg-white rounded-lg shadow hover:shadow-md transition-shadow p-6"
          >
            <div className="flex justify-between items-start mb-4">
              <Link to={`/tasks/${task.id}`} className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900 hover:text-blue-600">
                  {task.title}
                </h3>
              </Link>
              <div className="flex gap-2 ml-2">
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(task.status)}`}>
                  {task.status}
                </span>
              </div>
            </div>

            <p className="text-sm text-gray-600 mb-4 line-clamp-2">
              {task.description || 'No description'}
            </p>

            <div className="flex items-center justify-between text-sm">
              <div className="flex items-center gap-2">
                <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${getPriorityColor(task.priority)}`}>
                  {task.priority}
                </span>
              </div>
              <span className="text-gray-500">
                {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No due date'}
              </span>
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100 flex items-center justify-between text-sm text-gray-500">
              <span>By {task.createdByName}</span>
              {task.assignedToName && (
                <span>Assigned: {task.assignedToName}</span>
              )}
            </div>

            <div className="mt-4 flex gap-2">
              <Link
                to={`/tasks/${task.id}`}
                className="flex-1 text-center px-3 py-1.5 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                View
              </Link>
              {currentUser?.id === task.createdById && (
                <button
                  onClick={() => {
                    if (window.confirm('Are you sure you want to delete this task?')) {
                      deleteTask.mutate(task.id)
                    }
                  }}
                  className="px-3 py-1.5 border border-red-300 rounded-md text-sm font-medium text-red-700 hover:bg-red-50"
                >
                  Delete
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
