import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/apiClient'
import { TaskDto, CreateTaskRequest, UpdateTaskRequest } from '../types'

export const useTasks = (status?: string, priority?: string, assignedToId?: string) => {
  const queryClient = useQueryClient()

  const tasksQuery = useQuery({
    queryKey: ['tasks', status, priority, assignedToId],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (status) params.append('status', status)
      if (priority) params.append('priority', priority)
      if (assignedToId) params.append('assignedToId', assignedToId)

      const response = await api.get<TaskDto[]>(`/tasks?${params.toString()}`)
      return response.data
    },
  })

  const getTaskQuery = (taskId: string) =>
    useQuery({
      queryKey: ['tasks', taskId],
      queryFn: async () => {
        const response = await api.get<TaskDto>(`/tasks/${taskId}`)
        return response.data
      },
      enabled: !!taskId,
    })

  const createMutation = useMutation({
    mutationFn: async (request: CreateTaskRequest) => {
      const response = await api.post<TaskDto>('/tasks', request)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    },
  })

  const updateMutation = useMutation({
    mutationFn: async ({ id, request }: { id: string; request: UpdateTaskRequest }) => {
      const response = await api.put<TaskDto>(`/tasks/${id}`, request)
      return response.data
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['tasks', id] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (taskId: string) => {
      await api.delete(`/tasks/${taskId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    },
  })

  return {
    tasksQuery,
    getTaskQuery,
    createMutation,
    updateMutation,
    deleteMutation,
  }
}
