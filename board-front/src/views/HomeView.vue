<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { getPosts, deletePost } from '../api/posts'
import { useAuth } from '../composables/useAuth'
import type { Post } from '../types/post'

const { isLoggedIn } = useAuth()

const posts = ref<Post[]>([])
const loading = ref(true)
const error = ref('')
const page = ref(1)
const pageSize = ref(30)
const totalCount = ref(0)
const pageSizeOptions = [10, 30, 50, 100]

const totalPages = () => Math.ceil(totalCount.value / pageSize.value)

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('ja-JP')
}

const fetchPosts = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await getPosts(page.value, pageSize.value)
    posts.value = res.data.items
    totalCount.value = res.data.totalCount
  } catch {
    error.value = 'データの取得に失敗しました'
  } finally {
    loading.value = false
  }
}

onMounted(fetchPosts)

watch(pageSize, () => {
  page.value = 1
  fetchPosts()
})

const handleDeletePost = async (post: Post) => {
  if (!confirm(`「${post.title}」を削除しますか？`)) return
  try {
    await deletePost(post.id)
    posts.value = posts.value.filter(p => p.id !== post.id)
    totalCount.value--
  } catch {
    alert('削除に失敗しました')
  }
}

const goToPage = (p: number) => {
  if (p < 1 || p > totalPages()) return
  page.value = p
  fetchPosts()
}
</script>

<template>
  <div>
    <div class="list-header">
      <h1 class="page-title">投稿一覧</h1>
      <div class="page-size-selector">
        <span class="selector-label">表示件数:</span>
        <button
          v-for="size in pageSizeOptions"
          :key="size"
          class="size-btn"
          :class="{ active: pageSize === size }"
          @click="pageSize = size"
        >{{ size }}件</button>
      </div>
    </div>

    <p v-if="!isLoggedIn" class="notice">
      ℹ 新規投稿はログインしてから利用できます。
      <RouterLink to="/login">ログインはこちら</RouterLink>
    </p>

    <div v-if="loading" class="message">読み込み中...</div>
    <div v-else-if="error" class="message error">{{ error }}</div>
    <div v-else-if="posts.length === 0" class="message">まだ投稿がありません</div>

    <template v-else>
      <ul class="post-list">
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

      <div class="pagination">
        <button class="page-btn" :disabled="page === 1" @click="goToPage(page - 1)">← 前へ</button>
        <span class="page-info">{{ page }} / {{ totalPages() }}ページ（全{{ totalCount }}件）</span>
        <button class="page-btn" :disabled="page === totalPages()" @click="goToPage(page + 1)">次へ →</button>
      </div>
    </template>
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

.list-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.list-header .page-title { margin-bottom: 0; border-bottom: none; }

.page-size-selector {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.selector-label { font-size: 0.82rem; color: #888; }

.size-btn {
  background: none;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 3px 10px;
  font-size: 0.82rem;
  cursor: pointer;
  color: #555;
}

.size-btn:hover { border-color: #42b883; color: #42b883; }
.size-btn.active { background: #42b883; color: #fff; border-color: #42b883; }

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.page-btn {
  background: none;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 5px 14px;
  font-size: 0.85rem;
  cursor: pointer;
  color: #42b883;
}

.page-btn:hover:not(:disabled) { background: #42b883; color: #fff; border-color: #42b883; }
.page-btn:disabled { color: #ccc; border-color: #eee; cursor: not-allowed; }

.page-info { font-size: 0.85rem; color: #666; }
</style>
