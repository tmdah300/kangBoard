<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { getPosts, deletePost } from '../api/posts'
import { useAuth } from '../composables/useAuth'
import type { Post } from '../types/post'

const { isLoggedIn } = useAuth()

const posts = ref<Post[]>([])
const loading = ref(true)
const error = ref('')

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('ja-JP')
}

onMounted(async () => {
  try {
    const res = await getPosts()
    posts.value = res.data
  } catch {
    error.value = 'データの取得に失敗しました'
  } finally {
    loading.value = false
  }
})

const handleDeletePost = async (post: Post) => {
  if (!confirm(`「${post.title}」を削除しますか？`)) return
  try {
    await deletePost(post.id)
    posts.value = posts.value.filter(p => p.id !== post.id)
  } catch {
    alert('削除に失敗しました')
  }
}
</script>

<template>
  <div>
    <h1 class="page-title">投稿一覧</h1>

    <p v-if="!isLoggedIn" class="notice">
      ℹ 新規投稿はログインしてから利用できます。
      <RouterLink to="/login">ログインはこちら</RouterLink>
    </p>

    <div v-if="loading" class="message">読み込み中...</div>
    <div v-else-if="error" class="message error">{{ error }}</div>
    <div v-else-if="posts.length === 0" class="message">まだ投稿がありません</div>

    <ul v-else class="post-list">
      <li v-for="post in posts" :key="post.id" class="post-item">
        <RouterLink :to="`/posts/${post.id}`" class="post-link">
          <span class="post-title">{{ post.title }}</span>
          <span class="post-meta">
            {{ formatDate(post.createdAt) }}
            &nbsp;👁 {{ post.viewCount }}
            &nbsp;👍 {{ post.likeCount }}
          </span>
        </RouterLink>
        <button
          v-if="isLoggedIn"
          class="btn-delete"
          @click.prevent="handleDeletePost(post)"
          title="削除"
        >🗑</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.page-title {
  font-size: 1.4rem;
  margin-bottom: 1.5rem;
  border-bottom: 2px solid #42b883;
  padding-bottom: 0.5rem;
}

.notice {
  font-size: 0.85rem;
  color: #666;
  background: #f5f9ff;
  border: 1px solid #d0e4f7;
  border-radius: 4px;
  padding: 0.6rem 0.9rem;
  margin-bottom: 1.2rem;
}

.notice a {
  color: #42b883;
  text-decoration: none;
  font-weight: bold;
}

.message { color: #666; padding: 1rem 0; }
.message.error { color: #e74c3c; }

.post-list { list-style: none; padding: 0; margin: 0; }

.post-item {
  display: flex;
  align-items: center;
  border-bottom: 1px solid #eee;
}

.post-link {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0.5rem;
  text-decoration: none;
  color: inherit;
  min-width: 0;
}

.post-link:hover { background-color: #f8f8f8; }

.post-title {
  font-size: 1rem;
  font-weight: bold;
  color: #2c3e50;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.post-meta {
  font-size: 0.8rem;
  color: #999;
  white-space: nowrap;
  margin-left: 1rem;
  flex-shrink: 0;
}

.btn-delete {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  padding: 0.4rem 0.6rem;
  color: #ccc;
  flex-shrink: 0;
}

.btn-delete:hover { color: #e74c3c; }
</style>
