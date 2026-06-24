<script setup lang="ts">
import { RouterView, RouterLink, useRouter } from 'vue-router'
import { useAuth } from './composables/useAuth'

const router = useRouter()
const { isLoggedIn, username, logout } = useAuth()

const handleLogout = () => {
  logout()
  router.push('/')
}

const handleNewPost = () => {
  if (!isLoggedIn.value) {
    alert('新規投稿はログインしてから利用できます')
    return
  }
  router.push('/posts/new')
}
</script>

<template>
  <header class="header">
    <div class="header-inner">
      <RouterLink to="/" class="site-title">匿名掲示板</RouterLink>
      <nav class="header-nav">
        <template v-if="isLoggedIn">
          <span class="username">{{ username }}</span>
          <button class="btn-new" @click="handleNewPost">+ 新規投稿</button>
          <button class="btn-logout" @click="handleLogout">ログアウト</button>
        </template>
        <template v-else>
          <button class="btn-new" @click="handleNewPost">+ 新規投稿</button>
          <RouterLink to="/login" class="btn-login">ログイン</RouterLink>
          <RouterLink to="/register" class="btn-register">新規登録</RouterLink>
        </template>
      </nav>
    </div>
  </header>

  <div class="layout">
    <main class="main-content">
      <RouterView />
    </main>

    <aside class="sidebar">
      <div class="sidebar-card">
        <h2 class="sidebar-heading">メニュー</h2>
        <button class="sidebar-btn" @click="handleNewPost">+ 新規投稿</button>
        <RouterLink to="/" class="sidebar-link">📋 投稿一覧</RouterLink>
      </div>

      <div class="sidebar-card">
        <h2 class="sidebar-heading">このサイトについて</h2>
        <p class="sidebar-text">誰でも匿名で自由に投稿できる掲示板です。</p>
        <ul class="sidebar-rules">
          <li>誹謗中傷は禁止</li>
          <li>個人情報を書かない</li>
          <li>節度ある投稿を</li>
        </ul>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.header {
  background-color: #2c3e50;
  padding: 0 1.5rem;
  position: sticky;
  top: 0;
  z-index: 10;
}

.header-inner {
  max-width: 1200px;
  margin: 0 auto;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.site-title {
  color: #fff;
  font-size: 1.2rem;
  font-weight: bold;
  text-decoration: none;
}

.header-nav {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.username {
  color: #a8d8c4;
  font-size: 0.85rem;
}

.btn-new {
  background-color: #42b883;
  color: #fff;
  padding: 6px 14px;
  border-radius: 4px;
  border: none;
  font-size: 0.9rem;
  cursor: pointer;
}

.btn-login {
  color: #fff;
  text-decoration: none;
  font-size: 0.9rem;
  padding: 6px 14px;
  border: 1px solid rgba(255,255,255,0.4);
  border-radius: 4px;
}

.btn-register {
  background-color: #42b883;
  color: #fff;
  padding: 6px 14px;
  border-radius: 4px;
  text-decoration: none;
  font-size: 0.9rem;
}

.btn-logout {
  background: none;
  border: 1px solid rgba(255,255,255,0.4);
  color: #ccc;
  padding: 5px 12px;
  border-radius: 4px;
  font-size: 0.85rem;
  cursor: pointer;
}

/* ── レイアウト ── */
.layout {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem 1.5rem;
  display: flex;
  gap: 1.5rem;
  align-items: flex-start;
}

.main-content {
  flex: 1;
  min-width: 0;
}

/* サイドバーはデフォルト非表示 */
.sidebar {
  display: none;
}

/* 900px 以上でサイドバーを表示 */
@media (min-width: 900px) {
  .sidebar {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    width: 220px;
    flex-shrink: 0;
  }

  /* ヘッダーの新規投稿ボタンはワイド時に非表示 */
  .btn-new {
    display: none;
  }
}

/* ── サイドバーカード ── */
.sidebar-card {
  background: #f8f8f8;
  border: 1px solid #e8e8e8;
  border-radius: 6px;
  padding: 1rem;
}

.sidebar-heading {
  font-size: 0.85rem;
  font-weight: bold;
  color: #555;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.75rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.sidebar-btn {
  display: block;
  width: 100%;
  background-color: #42b883;
  color: #fff;
  text-align: center;
  padding: 8px 12px;
  border-radius: 4px;
  border: none;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
  cursor: pointer;
}

.sidebar-link {
  display: block;
  color: #2c3e50;
  text-decoration: none;
  padding: 6px 4px;
  font-size: 0.9rem;
  border-radius: 4px;
}

.sidebar-link:hover {
  background-color: #eee;
}

.sidebar-text {
  font-size: 0.85rem;
  color: #666;
  margin-bottom: 0.75rem;
  line-height: 1.5;
}

.sidebar-rules {
  font-size: 0.8rem;
  color: #888;
  padding-left: 1.2rem;
  line-height: 1.8;
}
</style>
