export interface Post {
  id: number
  title: string
  content: string
  createdAt: string
  viewCount: number
  likeCount: number
  userId: number | null
}

// 投稿作成時に送信するデータの型
export interface CreatePostRequest {
  title: string
  content: string
}

// ページング付きレスポンスの型
export interface PagedPosts {
  items: Post[]
  totalCount: number
  page: number
  pageSize: number
}
