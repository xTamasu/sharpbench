import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/apiClient'
import { CommentDto, CreateCommentRequest, UpdateCommentRequest } from '../types'

export const useComments = (taskId?: string) => {
  const queryClient = useQueryClient()

  const commentsQuery = useQuery({
    queryKey: ['comments', taskId],
    queryFn: async () => {
      const response = await api.get<CommentDto[]>(`/tasks/${taskId}/comments`)
      return response.data
    },
    enabled: !!taskId,
  })

  const createMutation = useMutation({
    mutationFn: async (request: CreateCommentRequest) => {
      const response = await api.post<CommentDto>(`/tasks/${taskId}/comments`, request)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] })
    },
  })

  const updateMutation = useMutation({
    mutationFn: async ({ commentId, request }: { commentId: string; request: UpdateCommentRequest }) => {
      const response = await api.put<CommentDto>(`/tasks/${taskId}/comments/${commentId}`, request)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (commentId: string) => {
      await api.delete(`/tasks/${taskId}/comments/${commentId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', taskId] })
    },
  })

  return {
    commentsQuery,
    createMutation,
    updateMutation,
    deleteMutation,
  }
}
