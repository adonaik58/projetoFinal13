import { screen } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it, vi } from 'vitest'

import { api, server } from '../../../../tests/msw-server'
import { renderWithQuery, setupUser } from '../../../../tests/render'
import { CheckInForm } from './check-in-form'

function useCatalogHandlers() {
  server.use(
    http.get(api('/catalog/brands'), () => HttpResponse.json([{ id: 3, name: 'Toyota' }])),
    http.get(api('/catalog/brands/:brandId/models'), () =>
      HttpResponse.json([{ id: 9, brandId: 3, name: 'Hilux' }]),
    ),
  )
}

describe('CheckInForm', () => {
  it('validates the form client-side and does not create a ticket', async () => {
    const user = setupUser()
    useCatalogHandlers()
    const createTicket = vi.fn()
    server.use(
      http.get(api('/customers/lookup'), () => new HttpResponse(null, { status: 404 })),
      http.post(api('/tickets'), () => {
        createTicket()
        return HttpResponse.json({})
      }),
    )
    const onSuccess = vi.fn()
    renderWithQuery(<CheckInForm spaceId={1} onSuccess={onSuccess} />)

    await user.click(screen.getByRole('button', { name: 'Registar entrada' }))

    expect(
      await screen.findByText('O Bilhete de Identidade contém 14 caracteres.'),
    ).toBeInTheDocument()
    expect(screen.getByText('Indique o nome completo.')).toBeInTheDocument()
    expect(screen.getByText('Indique a data de nascimento.')).toBeInTheDocument()
    expect(screen.getByText('Indique a matrícula.')).toBeInTheDocument()
    expect(screen.getByText('Escolha a marca.')).toBeInTheDocument()
    expect(createTicket).not.toHaveBeenCalled()
    expect(onSuccess).not.toHaveBeenCalled()
  })

  it('rejects a malformed BI with the legacy rule', async () => {
    const user = setupUser()
    useCatalogHandlers()
    renderWithQuery(<CheckInForm spaceId={1} onSuccess={vi.fn()} />)

    await user.type(screen.getByLabelText('Bilhete de Identidade'), '1234567890A123')
    await user.click(screen.getByRole('button', { name: 'Registar entrada' }))

    expect(
      await screen.findByText('Bilhete de Identidade inválido (ex.: 123456789LA123).'),
    ).toBeInTheDocument()
  })

  it('prefills a returning customer found by BI', async () => {
    const user = setupUser()
    useCatalogHandlers()
    let lookedUp: string | null = null
    server.use(
      http.get(api('/customers/lookup'), ({ request }) => {
        lookedUp = new URL(request.url).searchParams.get('nationalId')
        return HttpResponse.json({
          id: 5,
          fullName: 'Ana Kambu',
          nationalId: '123456789LA123',
          birthDate: '1990-05-17',
          phone: '923456789',
          lastVehicle: { plate: 'LD-12-34-AB', brandId: 3, modelId: 9, color: '#aa0000' },
          source: 'Customer',
        })
      }),
    )
    renderWithQuery(<CheckInForm spaceId={1} onSuccess={vi.fn()} />)

    await user.type(screen.getByLabelText('Bilhete de Identidade'), '123456789la123')

    expect(await screen.findByDisplayValue('Ana Kambu')).toBeInTheDocument()
    expect(lookedUp).toBe('123456789LA123')
    expect(screen.getByLabelText('Data de nascimento')).toHaveValue('1990-05-17')
    expect(screen.getByLabelText('Matrícula')).toHaveValue('LD-12-34-AB')
    expect(screen.getByLabelText('Contacto (opcional)')).toHaveValue('923456789')
    expect(screen.getByText(/Cliente registado/)).toBeInTheDocument()
  })

  it('prefills a new customer from the BI registry lookup', async () => {
    const user = setupUser()
    useCatalogHandlers()
    server.use(
      http.get(api('/customers/lookup'), () =>
        HttpResponse.json({
          id: null,
          fullName: 'Maria da Silva',
          nationalId: '006921020LA044',
          birthDate: '1995-03-02',
          phone: null,
          lastVehicle: null,
          source: 'Registry',
        }),
      ),
    )
    renderWithQuery(<CheckInForm spaceId={1} onSuccess={vi.fn()} />)

    await user.type(screen.getByLabelText('Bilhete de Identidade'), '006921020LA044')

    expect(await screen.findByDisplayValue('Maria da Silva')).toBeInTheDocument()
    expect(screen.getByLabelText('Data de nascimento')).toHaveValue('1995-03-02')
    expect(screen.getByText(/Dados obtidos da consulta do BI/)).toBeInTheDocument()
  })

  it('shows server conflicts (e.g. space taken meanwhile) as a form error', async () => {
    const user = setupUser()
    useCatalogHandlers()
    server.use(
      http.get(api('/customers/lookup'), () =>
        HttpResponse.json({
          id: 5,
          fullName: 'Ana Kambu',
          nationalId: '123456789LA123',
          birthDate: '1990-05-17',
          phone: null,
          lastVehicle: { plate: 'LD-12-34-AB', brandId: 3, modelId: null, color: '#aa0000' },
          source: 'Customer',
        }),
      ),
      http.post(api('/tickets'), ({ request }) => {
        expect(request.headers.get('Idempotency-Key')).toMatch(/^[0-9a-f-]{36}$/)
        return HttpResponse.json(
          {
            status: 409,
            title: 'Conflict',
            detail: 'O espaço A1 já não está livre.',
            code: 'ticket.space_not_free',
          },
          { status: 409 },
        )
      }),
    )
    const onSuccess = vi.fn()
    renderWithQuery(<CheckInForm spaceId={1} onSuccess={onSuccess} />)

    await user.type(screen.getByLabelText('Bilhete de Identidade'), '123456789LA123')
    await screen.findByDisplayValue('Ana Kambu')
    await user.click(screen.getByRole('button', { name: 'Registar entrada' }))

    expect(await screen.findByText('O espaço A1 já não está livre.')).toBeInTheDocument()
    expect(onSuccess).not.toHaveBeenCalled()
  })
})
