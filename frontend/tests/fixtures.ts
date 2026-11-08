import type { AuthResponse, AuthUser } from '@/features/auth/types'

export const adminUser: AuthUser = {
  id: 1,
  username: 'admin',
  fullName: 'Administrador do Sistema',
  role: 'Admin',
  permissions: [
    'dashboard.view',
    'spaces.view',
    'spaces.manage',
    'tickets.view',
    'tickets.open',
    'tickets.close',
    'catalog.view',
    'catalog.manage',
    'settings.view',
    'settings.manage',
    'users.manage',
    'audit.view',
  ],
}

export function authResponse(accessToken: string, user: AuthUser = adminUser): AuthResponse {
  return {
    accessToken,
    accessTokenExpiresAt: new Date(Date.now() + 15 * 60_000).toISOString(),
    user,
  }
}
