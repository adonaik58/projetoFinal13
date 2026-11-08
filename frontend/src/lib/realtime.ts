import { HttpTransportType, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

import { refreshSession } from './http'
import { session } from './session'

export const REALTIME_HUB_URL = '/hubs/parking'

export type RealtimeTopic = 'parking' | 'users'
export type RealtimeStatus = 'offline' | 'connecting' | 'connected' | 'reconnecting'

const listeners = new Set<() => void>()
let status: RealtimeStatus = 'offline'

function setStatus(next: RealtimeStatus) {
  if (status === next) return
  status = next
  for (const listener of listeners) listener()
}

export const realtimeStatus = {
  get: (): RealtimeStatus => status,
  subscribe: (listener: () => void): (() => void) => {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },
}

/**
 * `refetchInterval` for queries kept fresh by the server push: no polling while the socket is
 * connected, fall back to polling every `intervalMs` while it is down.
 */
export function whenRealtimeDown(intervalMs: number) {
  return () => (status === 'connected' ? false : intervalMs)
}

const TOKEN_REFRESH_MARGIN_MS = 30_000

/** The WebSocket sends the token once, at connect time: make sure it is not about to expire. */
async function freshAccessToken(): Promise<string> {
  const { accessToken, accessTokenExpiresAt } = session.getState()
  const expiresAt = accessTokenExpiresAt ? Date.parse(accessTokenExpiresAt) : 0
  if (!accessToken || expiresAt - Date.now() < TOKEN_REFRESH_MARGIN_MS) {
    await refreshSession()
  }
  return session.getAccessToken() ?? ''
}

const retryDelay = (attempt: number) => Math.min(30_000, 1_000 * 2 ** attempt)

/**
 * Connects to the realtime hub and keeps the connection alive: reconnects forever with backoff
 * and a fresh token (the server closes the socket when the access token expires).
 * `onChanged` receives every change topic; `onResync` runs whenever events may have been missed
 * (connection lost or restored), so views refresh and the polling fallback is re-evaluated.
 * Returns a function that closes the connection.
 */
export function connectRealtime(handlers: {
  onChanged: (topic: RealtimeTopic) => void
  onResync: () => void
}): () => void {
  let stopped = false
  let attempt = 0
  let timer: ReturnType<typeof setTimeout> | undefined

  const connection = new HubConnectionBuilder()
    .withUrl(REALTIME_HUB_URL, {
      accessTokenFactory: freshAccessToken,
      // WebSockets only, no negotiate round-trip: any API replica can take the connection.
      transport: HttpTransportType.WebSockets,
      skipNegotiation: true,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (context) => retryDelay(context.previousRetryCount),
    })
    .configureLogging(LogLevel.None)
    .build()

  connection.on('changed', (topic: RealtimeTopic) => {
    handlers.onChanged(topic)
  })
  connection.onreconnecting(() => {
    setStatus('reconnecting')
    handlers.onResync()
  })
  connection.onreconnected(() => {
    setStatus('connected')
    handlers.onResync()
  })
  // Closed by the server (e.g. token expired) or reconnection gave up: start over.
  connection.onclose(() => {
    if (stopped) return
    setStatus('reconnecting')
    handlers.onResync()
    schedule()
  })

  function schedule() {
    if (stopped) return
    clearTimeout(timer)
    timer = setTimeout(() => void start(), retryDelay(attempt++))
  }

  async function start() {
    if (stopped) return
    setStatus(attempt === 0 ? 'connecting' : 'reconnecting')
    try {
      await connection.start()
      const resumed = attempt > 0
      attempt = 0
      setStatus('connected')
      if (resumed) handlers.onResync()
    } catch {
      schedule()
    }
  }

  void start()

  return () => {
    stopped = true
    clearTimeout(timer)
    setStatus('offline')
    void connection.stop()
  }
}
