import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios'
import { describe, expect, it, vi } from 'vitest'

import { ApiError, applyFieldErrors, normalizeFieldPath, toApiError } from './api-error'

function axiosErrorWith(status: number, data: unknown, headers: Record<string, string> = {}) {
  const config = { headers: new AxiosHeaders() }
  const response = {
    status,
    statusText: '',
    data,
    headers,
    config,
  } as unknown as AxiosResponse
  return new AxiosError('Request failed', String(status), config, null, response)
}

describe('normalizeFieldPath', () => {
  it.each([
    ['Customer.NationalId', 'customer.nationalId'],
    ['vehicle.plate', 'vehicle.plate'],
    ['Items[0].Name', 'items.0.name'],
    ['Password', 'password'],
  ])('%s → %s', (input, expected) => {
    expect(normalizeFieldPath(input)).toBe(expected)
  })
})

describe('toApiError', () => {
  it('parses RFC 7807 problem details', () => {
    const error = toApiError(
      axiosErrorWith(
        409,
        {
          type: 'https://httpstatuses.io/409',
          title: 'Conflict',
          status: 409,
          detail: 'O espaço já está ocupado.',
          code: 'ticket.space_not_free',
          traceId: 'trace-1',
          correlationId: 'corr-1',
        },
        { 'x-correlation-id': 'header-corr' },
      ),
    )

    expect(error).toBeInstanceOf(ApiError)
    expect(error.status).toBe(409)
    expect(error.code).toBe('ticket.space_not_free')
    expect(error.detail).toBe('O espaço já está ocupado.')
    expect(error.correlationId).toBe('corr-1')
    expect(error.userMessage).toBe('O espaço já está ocupado.')
  })

  it('normalises validation errors and exposes the first message', () => {
    const error = toApiError(
      axiosErrorWith(400, {
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: { 'Customer.NationalId': ['BI inválido.'], Plate: 'Obrigatório.' },
      }),
    )

    expect(error.errors).toEqual({
      'customer.nationalId': ['BI inválido.'],
      plate: ['Obrigatório.'],
    })
    expect(error.userMessage).toBe('BI inválido.')
  })

  it('falls back to status-based titles and reads Retry-After for 429', () => {
    const error = toApiError(axiosErrorWith(429, '', { 'retry-after': '30' }))

    expect(error.title).toBe('Demasiados pedidos')
    expect(error.retryAfterSeconds).toBe(30)
    expect(error.userMessage).toContain('30 s')
  })

  it('hides server internals for 5xx responses', () => {
    const error = toApiError(axiosErrorWith(500, { title: 'NullReferenceException at …' }))
    expect(error.userMessage).toBe('Ocorreu um erro no servidor. Tente novamente mais tarde.')
  })

  it('wraps unknown errors', () => {
    expect(toApiError(new Error('boom')).detail).toBe('boom')
    expect(toApiError('weird').status).toBe(-1)
  })
})

describe('applyFieldErrors', () => {
  it('sets known fields and returns the rest', () => {
    const setError = vi.fn()
    const error = new ApiError({
      status: 400,
      title: 'Validation',
      errors: { 'customer.nationalId': ['Inválido.'], unknownField: ['Outro erro.'] },
    })

    const unmatched = applyFieldErrors(error, setError, ['customer.nationalId'] as const)

    expect(setError).toHaveBeenCalledWith('customer.nationalId', {
      type: 'server',
      message: 'Inválido.',
    })
    expect(unmatched).toEqual(['Outro erro.'])
  })
})
