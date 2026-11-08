import { redirect, type ParsedLocation } from '@tanstack/react-router'

import { refreshSession } from '@/lib/http'
import { session, type SessionState } from '@/lib/session'

import { hasPermission, homePathFor, type Permission } from './permissions'

/** Restores the session from the refresh cookie the first time it is needed. */
export async function ensureSession(): Promise<SessionState> {
  if (session.getState().status === 'unknown') await refreshSession()
  return session.getState()
}

/** Only allows same-origin, absolute paths as post-login targets (prevents open redirects). */
export function safeRedirectTarget(target: string | undefined): string | undefined {
  if (!target?.startsWith('/') || target.startsWith('//') || target.startsWith('/login')) {
    return undefined
  }
  return target
}

export async function requireAuthenticated(location: ParsedLocation) {
  const state = await ensureSession()
  if (state.status !== 'authenticated' || !state.user) {
    throw redirect({ to: '/login', search: { redirect: location.href } })
  }
  return { user: state.user }
}

export function requirePermission(permission: Permission) {
  return () => {
    const user = session.getUser()
    if (!hasPermission(user, permission)) {
      throw redirect({ to: homePathFor(user), replace: true })
    }
  }
}
