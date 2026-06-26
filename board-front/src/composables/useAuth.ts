import { ref, computed } from 'vue'
import { login as loginApi, register as registerApi } from '../api/auth'

const token = ref<string | null>(localStorage.getItem('token'))
const username = ref<string | null>(localStorage.getItem('username'))
const storedUserId = localStorage.getItem('userId')
const userId = ref<number | null>(storedUserId ? Number(storedUserId) : null)

export function useAuth() {
  const isLoggedIn = computed(() => !!token.value)

  const login = async (usernameInput: string, password: string) => {
    const res = await loginApi({ username: usernameInput, password })
    token.value = res.data.token
    username.value = res.data.username
    userId.value = res.data.userId
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('username', res.data.username)
    localStorage.setItem('userId', String(res.data.userId))
  }

  const register = async (usernameInput: string, password: string) => {
    const res = await registerApi({ username: usernameInput, password })
    token.value = res.data.token
    username.value = res.data.username
    userId.value = res.data.userId
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('username', res.data.username)
    localStorage.setItem('userId', String(res.data.userId))
  }

  const logout = () => {
    token.value = null
    username.value = null
    userId.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('userId')
  }

  return { token, username, userId, isLoggedIn, login, register, logout }
}
