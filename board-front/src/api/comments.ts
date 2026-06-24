import apiClient from './client'
import type { Comment, CreateCommentRequest } from '../types/comment'

export const getComments = (postId: number) => {
  return apiClient.get<Comment[]>(`/posts/${postId}/comments`)
}

export const createComment = (data: CreateCommentRequest) => {
  return apiClient.post<Comment>(`/posts/${data.postId}/comments`, data)
}

// コメントを削除（DelFlag）
export const deleteComment = (postId: number, commentId: number) => {
  return apiClient.delete(`/posts/${postId}/comments/${commentId}`)
}
