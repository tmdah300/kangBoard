<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPost, likePost, deletePost } from '../api/posts'
import { getComments, createComment, deleteComment } from '../api/comments'
import { useAuth } from '../composables/useAuth'
import type { Post } from '../types/post'
import type { Comment } from '../types/comment'

const { isLoggedIn, userId } = useAuth()

const route = useRoute()
const router = useRouter()

const post = ref<Post | null>(null)
const comments = ref<Comment[]>([])
const newComment = ref('')
const loading = ref(true)
const submitting = ref(false)
const liking = ref(false)
const error = ref('')

const postId = Number(route.params.id)

const isOwnPost = computed(() => post.value?.userId != null && post.value.userId === userId.value)

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('ja-JP')
}

onMounted(async () => {
  try {
    const [postRes, commentsRes] = await Promise.all([
      getPost(postId),
      getComments(postId),
    ])
    post.value = postRes.data
    comments.value = commentsRes.data
  } catch {
    error.value = '投稿の取得に失敗しました'
  } finally {
    loading.value = false
  }
})

const handleLike = async () => {
  if (!isLoggedIn.value) {
    alert('いいねするにはログインが必要です')
    return
  }
  if (!post.value || liking.value) return
  liking.value = true
  try {
    const res = await likePost(postId)
    post.value.likeCount = res.data.likeCount
  } catch {
    alert('いいねの送信に失敗しました')
  } finally {
    liking.value = false
  }
}

const handleDeletePost = async () => {
  if (!post.value) return
  if (!confirm(`「${post.value.title}」を削除しますか？`)) return
  try {
    await deletePost(postId)
    router.push('/')
  } catch {
    alert('削除に失敗しました')
  }
}

const handleDeleteComment = async (comment: Comment) => {
  if (!confirm('このコメントを削除しますか？')) return
  try {
    await deleteComment(postId, comment.id)
    comments.value = comments.value.filter(c => c.id !== comment.id)
  } catch {
    alert('削除に失敗しました')
  }
}

const submitComment = async () => {
  if (!newComment.value.trim()) return
  submitting.value = true
  try {
    const res = await createComment({ postId, content: newComment.value })
    comments.value.push(res.data)
    newComment.value = ''
  } catch {
    alert('コメントの送信に失敗しました')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <button class="back-btn" @click="router.back()">← 一覧に戻る</button>

    <div v-if="loading" class="message">読み込み中...</div>
    <div v-else-if="error" class="message error">{{ error }}</div>

    <template v-else-if="post">
      <!-- 投稿本文 -->
      <article class="post-card">
        <div class="post-header">
          <h1 class="post-title">{{ post.title }}</h1>
          <button v-if="isOwnPost" class="btn-delete-post" @click="handleDeletePost" title="投稿を削除">
            🗑 削除
          </button>
        </div>
        <div class="post-meta">
          {{ formatDate(post.createdAt) }}
          &nbsp;👁 {{ post.viewCount }}
        </div>
        <p class="post-content">{{ post.content }}</p>
        <div class="like-area">
          <button class="like-btn" :disabled="liking" @click="handleLike">
            👍 いいね！ {{ post.likeCount }}
          </button>
        </div>
      </article>

      <!-- コメント一覧 -->
      <section class="comments-section">
        <h2 class="section-title">コメント（{{ comments.length }}件）</h2>

        <div v-if="comments.length === 0" class="message">まだコメントがありません</div>

        <ul class="comment-list">
          <li v-for="comment in comments" :key="comment.id" class="comment-item">
            <div class="comment-body">
              <p class="comment-content">{{ comment.content }}</p>
              <span class="comment-date">{{ formatDate(comment.createdAt) }}</span>
            </div>
            <button
              v-if="isLoggedIn"
              class="btn-delete-comment"
              @click="handleDeleteComment(comment)"
              title="削除"
            >🗑</button>
          </li>
        </ul>

        <!-- コメント投稿フォーム -->
        <form class="comment-form" @submit.prevent="submitComment">
          <textarea
            v-model="newComment"
            placeholder="コメントを入力..."
            rows="3"
            class="comment-input"
          />
          <button type="submit" class="btn-submit" :disabled="submitting">
            {{ submitting ? '送信中...' : 'コメントする' }}
          </button>
        </form>
      </section>
    </template>
  </div>
</template>

<style scoped>
.back-btn {
  background: none;
  border: none;
  color: #42b883;
  cursor: pointer;
  font-size: 0.9rem;
  padding: 0;
  margin-bottom: 1.5rem;
}

.message { color: #666; padding: 1rem 0; }
.message.error { color: #e74c3c; }

.post-card {
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.post-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
}

.post-title { font-size: 1.4rem; margin: 0; }

.btn-delete-post {
  background: none;
  border: 1px solid #e0e0e0;
  color: #aaa;
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 0.8rem;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
  margin-left: 1rem;
}

.btn-delete-post:hover { border-color: #e74c3c; color: #e74c3c; }

.post-meta { font-size: 0.8rem; color: #999; margin-bottom: 1rem; }

.post-content { white-space: pre-wrap; line-height: 1.8; }

.like-area {
  margin-top: 1.2rem;
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.like-btn {
  background: #fff;
  border: 2px solid #42b883;
  color: #42b883;
  padding: 8px 20px;
  border-radius: 20px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}

.like-btn:hover:not(:disabled) { background: #42b883; color: #fff; }
.like-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.section-title {
  font-size: 1.1rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.5rem;
  margin-bottom: 1rem;
}

.comment-list { list-style: none; padding: 0; }

.comment-item {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  border-bottom: 1px solid #f0f0f0;
  padding: 0.8rem 0;
}

.comment-body { flex: 1; min-width: 0; }

.comment-content { margin: 0 0 0.3rem; }

.comment-date { font-size: 0.75rem; color: #aaa; }

.btn-delete-comment {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 0.95rem;
  padding: 0.2rem 0.4rem;
  color: #ddd;
  flex-shrink: 0;
}

.btn-delete-comment:hover { color: #e74c3c; }

.comment-form { margin-top: 1.5rem; }

.comment-input {
  width: 100%;
  padding: 0.7rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.95rem;
  resize: vertical;
  box-sizing: border-box;
}

.btn-submit {
  margin-top: 0.7rem;
  background-color: #42b883;
  color: #fff;
  border: none;
  padding: 8px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.btn-submit:disabled { background-color: #aaa; cursor: not-allowed; }
</style>
