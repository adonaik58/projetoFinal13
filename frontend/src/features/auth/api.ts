import { CSRF_HEADER, http } from '@/lib/http'
import { session } from '@/lib/session'

import type { AuthResponse, AuthUser, ChangePasswordRequest, LoginRequest } from './types'

export async function login(request: LoginRequest): Promise<AuthUser> {
  const { data } = await http.post<AuthResponse>('/auth/login', request)
  session.set(data)
  return data.user
}

export async function logout(): Promise<void> {
  try {
    await http.post('/auth/logout', null, { headers: CSRF_HEADER })
  } finally {
    session.clear()
  }
}

export async function fetchMe(): Promise<AuthUser> {
  const { data } = await http.get<AuthUser>('/auth/me')
  session.setUser(data)
  return data
}

export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await http.post('/auth/change-password', request)
}
