import apiClient from './client'
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types/auth'

export const login = (data: LoginRequest) => {
  return apiClient.post<AuthResponse>('/auth/login', data)
}

export const register = (data: RegisterRequest) => {
  return apiClient.post<AuthResponse>('/auth/register', data)
}
