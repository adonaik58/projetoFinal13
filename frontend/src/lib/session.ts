import type { AuthResponse, AuthUser } from '@/features/auth/types'

export type SessionStatus = 'unknown' | 'authenticated' | 'anonymous'

export interface SessionState {
  status: SessionStatus
  user: AuthUser | null
  accessToken: string | null
  accessTokenExpiresAt: string | null
}

type Listener = () => void

const anonymousState: SessionState = {
  status: 'anonymous',
  user: null,
  accessToken: null,
  accessTokenExpiresAt: null,
}

let state: SessionState = { ...anonymousState, status: 'unknown' }
const listeners = new Set<Listener>()

function emit(next: SessionState) {
  state = next
  for (const listener of listeners) listener()
}

/**
 * In-memory session store. The access token never touches persistent storage;
 * the refresh token lives in an HttpOnly cookie managed by the API.
 */
export const session = {
  getState: (): SessionState => state,
  getAccessToken: (): string | null => state.accessToken,
  getUser: (): AuthUser | null => state.user,
  subscribe: (listener: Listener): (() => void) => {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },
  set: (response: AuthResponse) => {
    emit({
      status: 'authenticated',
      user: response.user,
      accessToken: response.accessToken,
      accessTokenExpiresAt: response.accessTokenExpiresAt,
    })
  },
  setUser: (user: AuthUser) => {
    if (state.status !== 'authenticated') return
    emit({ ...state, user })
  },
  clear: () => {
    emit(anonymousState)
  },
  /** Test helper: return to the initial, not-yet-bootstrapped state. */
  reset: () => {
    emit({ ...anonymousState, status: 'unknown' })
  },
}
