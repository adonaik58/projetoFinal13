import type { Permission } from './permissions'

export type Role = 'Admin' | 'Operator'

export interface AuthUser {
  id: number
  username: string
  fullName: string
  role: Role
  permissions: Permission[]
}

export interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
  user: AuthUser
}

export interface LoginRequest {
  username: string
  password: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}
