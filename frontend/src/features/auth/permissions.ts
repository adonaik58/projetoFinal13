import type { AuthUser, Role } from './types'

export const PERMISSIONS = [
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
] as const

export type Permission = (typeof PERMISSIONS)[number]

export const ROLE_LABELS: Record<Role, string> = {
  Admin: 'Administrador',
  Operator: 'Operador',
}

export function hasPermission(user: AuthUser | null | undefined, permission: Permission): boolean {
  return !!user && user.permissions.includes(permission)
}

/** First page a user may open: admins land on the dashboard, operators on the spaces board. */
export function homePathFor(
  user: AuthUser | null | undefined,
): '/' | '/spaces' | '/tickets' | '/profile' {
  if (hasPermission(user, 'dashboard.view')) return '/'
  if (hasPermission(user, 'spaces.view')) return '/spaces'
  if (hasPermission(user, 'tickets.view')) return '/tickets'
  return '/profile'
}
