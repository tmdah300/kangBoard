import apiClient from './client'
import type { Post, CreatePostRequest } from '../types/post'

// 投稿一覧を取得
export const getPosts = () => {
  return apiClient.get<Post[]>('/posts')
}

// 投稿1件を取得
export const getPost = (id: number) => {
  return apiClient.get<Post>(`/posts/${id}`)
}

// 投稿を作成
export const createPost = (data: CreatePostRequest) => {
  return apiClient.post<Post>('/posts', data)
}

// いいねを送る
export const likePost = (id: number) => {
  return apiClient.post<{ likeCount: number }>(`/posts/${id}/like`)
}

// 投稿を削除（DelFlag）
export const deletePost = (id: number) => {
  return apiClient.delete(`/posts/${id}`)
}
