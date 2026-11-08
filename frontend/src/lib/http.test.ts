import { http as mswHttp, HttpResponse } from 'msw'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api, server } from '../../tests/msw-server'
import { authResponse } from '../../tests/fixtures'
import { ApiError } from './api-error'
import { http, setUnauthenticatedHandler } from './http'
import { session } from './session'

describe('http client', () => {
  const onUnauthenticated = vi.fn()

  beforeEach(() => {
    session.reset()
    onUnauthenticated.mockReset()
    setUnauthenticatedHandler(onUnauthenticated)
  })

  it('sends the in-memory access token as a Bearer header', async () => {
    session.set(authResponse('token-1'))
    let authorization: string | null = null
    server.use(
      mswHttp.get(api('/spaces'), ({ request }) => {
        authorization = request.headers.get('Authorization')
        return HttpResponse.json({ ok: true })
      }),
    )

    await http.get('/spaces')

    expect(authorization).toBe('Bearer token-1')
  })

  it('refreshes the session once on 401 and retries the request', async () => {
    session.set(authResponse('expired'))
    const refreshCalls = vi.fn()
    server.use(
      mswHttp.get(api('/spaces'), ({ request }) =>
        request.headers.get('Authorization') === 'Bearer fresh'
          ? HttpResponse.json({ items: [] })
          : HttpResponse.json({ status: 401, title: 'Unauthorized' }, { status: 401 }),
      ),
      mswHttp.post(api('/auth/refresh'), ({ request }) => {
        refreshCalls(request.headers.get('X-Requested-With'))
        return HttpResponse.json(authResponse('fresh'))
      }),
    )

    const response = await http.get<{ items: unknown[] }>('/spaces')

    expect(response.data).toEqual({ items: [] })
    expect(refreshCalls).toHaveBeenCalledTimes(1)
    expect(refreshCalls).toHaveBeenCalledWith('XMLHttpRequest')
    expect(session.getAccessToken()).toBe('fresh')
    expect(onUnauthenticated).not.toHaveBeenCalled()
  })

  it('shares a single refresh between concurrent 401 responses', async () => {
    session.set(authResponse('expired'))
    const refreshCalls = vi.fn()
    server.use(
      mswHttp.get(api('/spaces'), ({ request }) =>
        request.headers.get('Authorization') === 'Bearer fresh'
          ? HttpResponse.json({ ok: 'spaces' })
          : new HttpResponse(null, { status: 401 }),
      ),
      mswHttp.get(api('/tickets'), ({ request }) =>
        request.headers.get('Authorization') === 'Bearer fresh'
          ? HttpResponse.json({ ok: 'tickets' })
          : new HttpResponse(null, { status: 401 }),
      ),
      mswHttp.post(api('/auth/refresh'), async () => {
        refreshCalls()
        await new Promise((resolve) => setTimeout(resolve, 20))
        return HttpResponse.json(authResponse('fresh'))
      }),
    )

    const [spaces, tickets] = await Promise.all([http.get('/spaces'), http.get('/tickets')])

    expect(spaces.data).toEqual({ ok: 'spaces' })
    expect(tickets.data).toEqual({ ok: 'tickets' })
    expect(refreshCalls).toHaveBeenCalledTimes(1)
  })

  it('clears the session and notifies when the refresh fails', async () => {
    session.set(authResponse('expired'))
    server.use(
      mswHttp.get(api('/spaces'), () => new HttpResponse(null, { status: 401 })),
      mswHttp.post(api('/auth/refresh'), () => new HttpResponse(null, { status: 401 })),
    )

    const error: unknown = await http.get('/spaces').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(401)
    expect(session.getState().status).toBe('anonymous')
    expect(onUnauthenticated).toHaveBeenCalledTimes(1)
  })

  it('does not try to refresh when the login itself is rejected', async () => {
    const refreshCalls = vi.fn()
    server.use(
      mswHttp.post(api('/auth/login'), () =>
        HttpResponse.json(
          { status: 401, title: 'Credenciais inválidas', code: 'auth.invalid_credentials' },
          { status: 401 },
        ),
      ),
      mswHttp.post(api('/auth/refresh'), () => {
        refreshCalls()
        return HttpResponse.json(authResponse('x'))
      }),
    )

    const error: unknown = await http
      .post('/auth/login', { username: 'a', password: 'b' })
      .catch((e: unknown) => e)

    expect((error as ApiError).code).toBe('auth.invalid_credentials')
    expect(refreshCalls).not.toHaveBeenCalled()
    expect(onUnauthenticated).not.toHaveBeenCalled()
  })

  it('maps network failures to an ApiError with status 0', async () => {
    server.use(mswHttp.get(api('/spaces'), () => HttpResponse.error()))

    const error: unknown = await http.get('/spaces').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).isNetworkError).toBe(true)
    expect((error as ApiError).userMessage).toMatch(/Sem ligação/)
  })
})
