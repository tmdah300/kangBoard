export interface Comment {
  id: number
  postId: number
  content: string
  createdAt: string
}

export interface CreateCommentRequest {
  postId: number
  content: string
}
