<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { createPost } from '../api/posts'

const router = useRouter()

const title = ref('')
const content = ref('')
const submitting = ref(false)
const errors = ref({ title: '', content: '' })

const validate = () => {
  errors.value = { title: '', content: '' }
  if (!title.value.trim()) errors.value.title = 'タイトルを入力してください'
  if (!content.value.trim()) errors.value.content = '内容を入力してください'
  return !errors.value.title && !errors.value.content
}

const submit = async () => {
  if (!validate()) return

  submitting.value = true
  try {
    await createPost({ title: title.value, content: content.value })
    // 投稿成功後に一覧ページへ遷移
    router.push('/')
  } catch {
    alert('投稿に失敗しました')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <h1 class="page-title">新規投稿</h1>

    <form class="form" @submit.prevent="submit">
      <div class="form-group">
        <label class="label">タイトル</label>
        <!-- v-model で入力値とref()を双方向バインド -->
        <input v-model="title" type="text" class="input" placeholder="タイトルを入力" />
        <span v-if="errors.title" class="error-msg">{{ errors.title }}</span>
      </div>

      <div class="form-group">
        <label class="label">内容</label>
        <textarea v-model="content" class="textarea" rows="8" placeholder="内容を入力" />
        <span v-if="errors.content" class="error-msg">{{ errors.content }}</span>
      </div>

      <div class="form-actions">
        <button type="button" class="btn-cancel" @click="router.back()">キャンセル</button>
        <button type="submit" class="btn-submit" :disabled="submitting">
          {{ submitting ? '投稿中...' : '投稿する' }}
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.page-title {
  font-size: 1.4rem;
  margin-bottom: 1.5rem;
  border-bottom: 2px solid #42b883;
  padding-bottom: 0.5rem;
}

.form { display: flex; flex-direction: column; gap: 1.2rem; }

.form-group { display: flex; flex-direction: column; gap: 0.4rem; }

.label { font-weight: bold; font-size: 0.9rem; color: #2c3e50; }

.input, .textarea {
  padding: 0.7rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
  width: 100%;
}

.textarea { resize: vertical; }

.error-msg { color: #e74c3c; font-size: 0.82rem; }

.form-actions { display: flex; justify-content: flex-end; gap: 0.8rem; margin-top: 0.5rem; }

.btn-cancel {
  background: none;
  border: 1px solid #ddd;
  padding: 8px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.btn-submit {
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
