import { render, screen } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { session } from '@/lib/session'

import { adminUser, authResponse } from '../../../../tests/fixtures'
import { api, server } from '../../../../tests/msw-server'
import { setupUser } from '../../../../tests/render'
import { LoginForm } from './login-form'

describe('LoginForm', () => {
  beforeEach(() => {
    session.reset()
  })

  it('validates required fields without calling the API', async () => {
    const user = setupUser()
    const onSuccess = vi.fn()
    render(<LoginForm onSuccess={onSuccess} />)

    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByText('Indique o nome de utilizador.')).toBeInTheDocument()
    expect(screen.getByText('Indique a senha.')).toBeInTheDocument()
    expect(screen.getByLabelText('Nome de utilizador')).toHaveAttribute('aria-invalid', 'true')
    expect(onSuccess).not.toHaveBeenCalled()
  })

  it('signs in and stores the session in memory', async () => {
    const user = setupUser()
    let body: unknown
    server.use(
      http.post(api('/auth/login'), async ({ request }) => {
        body = await request.json()
        return HttpResponse.json(authResponse('access-1'))
      }),
    )
    const onSuccess = vi.fn()
    render(<LoginForm onSuccess={onSuccess} />)

    await user.type(screen.getByLabelText('Nome de utilizador'), 'admin')
    await user.type(screen.getByLabelText('Senha'), 'secret123')
    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    await vi.waitFor(() => {
      expect(onSuccess).toHaveBeenCalledWith(adminUser)
    })
    expect(body).toEqual({ username: 'admin', password: 'secret123' })
    expect(session.getAccessToken()).toBe('access-1')
    expect(window.localStorage.length).toBe(0)
  })

  it('shows a friendly message for invalid credentials', async () => {
    const user = setupUser()
    server.use(
      http.post(api('/auth/login'), () =>
        HttpResponse.json({ status: 401, title: 'Unauthorized' }, { status: 401 }),
      ),
    )
    render(<LoginForm onSuccess={vi.fn()} />)

    await user.type(screen.getByLabelText('Nome de utilizador'), 'admin')
    await user.type(screen.getByLabelText('Senha'), 'wrong')
    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByText('Nome de utilizador ou senha incorretos.')).toBeInTheDocument()
  })

  it('shows the lockout message when rate limited', async () => {
    const user = setupUser()
    server.use(
      http.post(
        api('/auth/login'),
        () =>
          new HttpResponse(JSON.stringify({ status: 429, title: 'Too Many Requests' }), {
            status: 429,
            headers: { 'Content-Type': 'application/problem+json', 'Retry-After': '60' },
          }),
      ),
    )
    render(<LoginForm onSuccess={vi.fn()} />)

    await user.type(screen.getByLabelText('Nome de utilizador'), 'admin')
    await user.type(screen.getByLabelText('Senha'), 'x')
    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByText(/Tente novamente dentro de 60 s/)).toBeInTheDocument()
  })

  it('toggles password visibility', async () => {
    const user = setupUser()
    render(<LoginForm onSuccess={vi.fn()} />)
    const password = screen.getByLabelText('Senha')
    expect(password).toHaveAttribute('type', 'password')

    await user.click(screen.getByRole('button', { name: 'Mostrar senha' }))

    expect(password).toHaveAttribute('type', 'text')
    expect(screen.getByRole('button', { name: 'Ocultar senha' })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
  })
})
