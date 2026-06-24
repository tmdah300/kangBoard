import { ref, computed } from 'vue'
import { login as loginApi, register as registerApi } from '../api/auth'

const token = ref<string | null>(localStorage.getItem('token'))
const username = ref<string | null>(localStorage.getItem('username'))

export function useAuth() {
  const isLoggedIn = computed(() => !!token.value)

  const login = async (usernameInput: string, password: string) => {
    const res = await loginApi({ username: usernameInput, password })
    token.value = res.data.token
    username.value = res.data.username
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('username', res.data.username)
  }

  const register = async (usernameInput: string, password: string) => {
    const res = await registerApi({ username: usernameInput, password })
    token.value = res.data.token
    username.value = res.data.username
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('username', res.data.username)
  }

  const logout = () => {
    token.value = null
    username.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('username')
  }

  return { token, username, isLoggedIn, login, register, logout }
}
