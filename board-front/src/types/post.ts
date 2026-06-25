// APIから返ってくるPostの型（C#のPostモデルと対応）
export interface Post {
  id: number
  title: string
  content: string
  createdAt: string  // C#のDateTimeはJSON化するとstring
  viewCount: number
  likeCount: number
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
