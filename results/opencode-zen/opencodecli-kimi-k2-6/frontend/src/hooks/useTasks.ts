import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { tasksApi, commentsApi } from '../api/services'
import type { TaskFilter, CreateTaskRequest, UpdateTaskRequest, CreateCommentRequest, UpdateCommentRequest } from '../types'

const TASKS_KEY = 'tasks'
const TASK_KEY = 'task'

/**
 * Hook to fetch all tasks with optional filtering.
 */
export function useTasks(filter?: TaskFilter) {
  return useQuery({
    queryKey: [TASKS_KEY, filter],
    queryFn: () => tasksApi.getAll(filter),
  })
}

/**
 * Hook to fetch a single task by ID.
 */
export function useTask(id: string) {
  return useQuery({
    queryKey: [TASK_KEY, id],
    queryFn: () => tasksApi.getById(id),
    enabled: !!id,
  })
}

/**
 * Hook to create a new task.
 * Invalidates the tasks list query on success.
 */
export function useCreateTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateTaskRequest) => tasksApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TASKS_KEY] })
    },
  })
}

/**
 * Hook to update an existing task.
 * Invalidates both the tasks list and the specific task query on success.
 */
export function useUpdateTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTaskRequest }) => tasksApi.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [TASKS_KEY] })
      queryClient.invalidateQueries({ queryKey: [TASK_KEY, variables.id] })
    },
  })
}

/**
 * Hook to delete a task.
 * Invalidates the tasks list query on success.
 */
export function useDeleteTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => tasksApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TASKS_KEY] })
    },
  })
}

/**
 * Hook to create a comment on a task.
 * Invalidates the specific task query on success.
 */
export function useCreateComment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ taskId, data }: { taskId: string; data: CreateCommentRequest }) =>
      commentsApi.create(taskId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [TASK_KEY, variables.taskId] })
    },
  })
}

/**
 * Hook to update an existing comment.
 * Invalidates the specific task query on success.
 */
export function useUpdateComment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      taskId,
      commentId,
      data,
    }: {
      taskId: string
      commentId: string
      data: UpdateCommentRequest
    }) => commentsApi.update(taskId, commentId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [TASK_KEY, variables.taskId] })
    },
  })
}

/**
 * Hook to delete a comment.
 * Invalidates the specific task query on success.
 */
export function useDeleteComment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ taskId, commentId }: { taskId: string; commentId: string }) =>
      commentsApi.delete(taskId, commentId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [TASK_KEY, variables.taskId] })
    },
  })
}
