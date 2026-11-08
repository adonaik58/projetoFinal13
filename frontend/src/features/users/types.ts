import type { Role } from '@/features/auth/types'

export interface UserDto {
  id: number
  username: string
  fullName: string
  role: Role
  isActive: boolean
  isOnline: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface UserQueryParams {
  page: number
  pageSize: number
  search?: string | undefined
  role?: Role | undefined
  isActive?: boolean | undefined
}
