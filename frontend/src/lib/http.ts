import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'

import type { AuthResponse } from '@/features/auth/types'

import { toApiError } from './api-error'
import { session } from './session'

export const API_BASE_URL = 'https://localhost/api/v1'

/** Header required by the API on cookie-authenticated endpoints (CSRF defence). */
export const CSRF_HEADER = { 'X-Requested-With': 'XMLHttpRequest' } as const

const AUTH_ENDPOINTS_WITHOUT_RETRY = ['/auth/login', '/auth/refresh', '/auth/logout']

interface RetriableConfig extends InternalAxiosRequestConfig {
  _retried?: boolean
}

/** Bare client used only for the refresh call, so it never re-enters the interceptors. */
const authClient = axios.create({ baseURL: API_BASE_URL, withCredentials: true, timeout: 15_000 })

export const http = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  timeout: 30_000,
  headers: { Accept: 'application/json' },
})

let unauthenticatedHandler: (() => void) | null = null

/** Registered by the router so an expired session sends the user to the login page. */
export function setUnauthenticatedHandler(handler: (() => void) | null) {
  unauthenticatedHandler = handler
}

let refreshInFlight: Promise<boolean> | null = null

/**
 * Exchanges the HttpOnly refresh cookie for a new access token.
 * Concurrent callers share a single request (single-flight).
 */
export function refreshSession(): Promise<boolean> {
  refreshInFlight ??= authClient
    .post<AuthResponse>('/auth/refresh', null, { headers: CSRF_HEADER })
    .then((response) => {
      session.set(response.data)
      return true
    })
    .catch(() => {
      session.clear()
      return false
    })
    .finally(() => {
      refreshInFlight = null
    })
  return refreshInFlight
}

function isAuthEndpoint(url: string | undefined) {
  return !!url && AUTH_ENDPOINTS_WITHOUT_RETRY.some((endpoint) => url.endsWith(endpoint))
}

http.interceptors.request.use((config) => {
  const token = session.getAccessToken()
  if (token) config.headers.set('Authorization', `Bearer ${token}`)
  return config
})

http.interceptors.response.use(undefined, async (error: AxiosError) => {
  const config = error.config as RetriableConfig | undefined
  if (error.response?.status === 401 && config && !config._retried && !isAuthEndpoint(config.url)) {
    config._retried = true
    if (await refreshSession()) {
      config.headers.set('Authorization', `Bearer ${session.getAccessToken() ?? ''}`)
      return http.request(config)
    }
    unauthenticatedHandler?.()
  }
  throw toApiError(error)
})
