import { useSyncExternalStore } from 'react'

import { session, type SessionState } from '@/lib/session'

import { hasPermission, type Permission } from './permissions'

export function useSession(): SessionState {
  return useSyncExternalStore(session.subscribe, session.getState, session.getState)
}

export function useAuth() {
  const state = useSession()
  return {
    ...state,
    can: (permission: Permission) => hasPermission(state.user, permission),
  }
}
