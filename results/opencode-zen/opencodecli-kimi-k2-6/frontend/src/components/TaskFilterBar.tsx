import type { TaskStatus, Priority, TaskFilter } from '../types'

/**
 * Component for filtering tasks by status, priority, and assignee.
 */
interface TaskFilterBarProps {
  filter: TaskFilter
  onFilterChange: (filter: TaskFilter) => void
}

export default function TaskFilterBar({ filter, onFilterChange }: TaskFilterBarProps) {
  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value as TaskStatus | ''
    onFilterChange({ ...filter, status: value || undefined })
  }

  const handlePriorityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value as Priority | ''
    onFilterChange({ ...filter, priority: value || undefined })
  }

  const handleClear = () => {
    onFilterChange({})
  }

  return (
    <div className="bg-white p-4 rounded-lg shadow mb-6 flex flex-wrap gap-4 items-end">
      <div>
        <label htmlFor="status-filter" className="block text-sm font-medium text-gray-700 mb-1">
          Status
        </label>
        <select
          id="status-filter"
          value={filter.status || ''}
          onChange={handleStatusChange}
          className="block w-40 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border px-3 py-2"
        >
          <option value="">All Statuses</option>
          <option value="Todo">Todo</option>
          <option value="InProgress">In Progress</option>
          <option value="Done">Done</option>
        </select>
      </div>

      <div>
        <label htmlFor="priority-filter" className="block text-sm font-medium text-gray-700 mb-1">
          Priority
        </label>
        <select
          id="priority-filter"
          value={filter.priority || ''}
          onChange={handlePriorityChange}
          className="block w-40 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border px-3 py-2"
        >
          <option value="">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
      </div>

      <button
        onClick={handleClear}
        className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500"
      >
        Clear Filters
      </button>
    </div>
  )
}
