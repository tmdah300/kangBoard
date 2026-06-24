<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'

const router = useRouter()
const { login } = useAuth()

const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

const handleSubmit = async () => {
  if (!username.value.trim() || !password.value) return
  error.value = ''
  loading.value = true
  try {
    await login(username.value.trim(), password.value)
    router.push('/')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'ログインに失敗しました'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h1 class="auth-title">ログイン</h1>

      <div v-if="error" class="error-msg">{{ error }}</div>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <div class="field">
          <label class="label">ユーザー名</label>
          <input v-model="username" type="text" class="input" placeholder="ユーザー名" required />
        </div>
        <div class="field">
          <label class="label">パスワード</label>
          <input v-model="password" type="password" class="input" placeholder="パスワード" required />
        </div>
        <button type="submit" class="btn-submit" :disabled="loading">
          {{ loading ? 'ログイン中...' : 'ログイン' }}
        </button>
      </form>

      <p class="switch-link">
        アカウントをお持ちでない方は
        <RouterLink to="/register">新規登録</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-wrapper {
  display: flex;
  justify-content: center;
  padding: 2rem 1rem;
}

.auth-card {
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
  padding: 2rem;
  width: 100%;
  max-width: 400px;
}

.auth-title {
  font-size: 1.4rem;
  margin-bottom: 1.5rem;
  text-align: center;
  border-bottom: 2px solid #42b883;
  padding-bottom: 0.5rem;
}

.error-msg {
  background: #fef0f0;
  color: #e74c3c;
  border: 1px solid #fcd6d6;
  border-radius: 4px;
  padding: 0.7rem;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.label {
  font-size: 0.85rem;
  color: #555;
  font-weight: bold;
}

.input {
  padding: 0.6rem 0.8rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.15s;
}

.input:focus {
  border-color: #42b883;
}

.btn-submit {
  background-color: #42b883;
  color: #fff;
  border: none;
  padding: 10px;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 0.5rem;
}

.btn-submit:disabled {
  background-color: #aaa;
  cursor: not-allowed;
}

.switch-link {
  text-align: center;
  margin-top: 1.2rem;
  font-size: 0.85rem;
  color: #666;
}

.switch-link a {
  color: #42b883;
  text-decoration: none;
  font-weight: bold;
}
</style>
