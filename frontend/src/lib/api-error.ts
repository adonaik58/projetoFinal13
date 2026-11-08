import { isAxiosError } from 'axios'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'

/** RFC 7807 problem details as produced by the API. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  code?: string
  errors?: Record<string, string[]>
  traceId?: string
  correlationId?: string
}

export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly detail: string | undefined
  readonly code: string | undefined
  readonly errors: Record<string, string[]>
  readonly traceId: string | undefined
  readonly correlationId: string | undefined
  readonly retryAfterSeconds: number | undefined

  constructor(init: {
    status: number
    title: string
    detail?: string | undefined
    code?: string | undefined
    errors?: Record<string, string[]> | undefined
    traceId?: string | undefined
    correlationId?: string | undefined
    retryAfterSeconds?: number | undefined
  }) {
    super(init.detail ?? init.title)
    this.name = 'ApiError'
    this.status = init.status
    this.title = init.title
    this.detail = init.detail
    this.code = init.code
    this.errors = init.errors ?? {}
    this.traceId = init.traceId
    this.correlationId = init.correlationId
    this.retryAfterSeconds = init.retryAfterSeconds
  }

  get isNetworkError() {
    return this.status === 0
  }

  get isClientError() {
    return this.status >= 400 && this.status < 500
  }

  /** The best message to show to a person. */
  get userMessage(): string {
    if (this.isNetworkError)
      return 'Sem ligação ao servidor. Verifique a sua rede e tente novamente.'
    if (this.status === 429) {
      return this.retryAfterSeconds
        ? `Demasiados pedidos. Tente novamente dentro de ${this.retryAfterSeconds} s.`
        : 'Demasiados pedidos. Aguarde um momento e tente novamente.'
    }
    if (this.status >= 500) return 'Ocorreu um erro no servidor. Tente novamente mais tarde.'
    const firstFieldError = Object.values(this.errors)[0]?.[0]
    return this.detail ?? firstFieldError ?? this.title
  }
}

const defaultTitles: Record<number, string> = {
  400: 'Pedido inválido',
  401: 'Sessão expirada ou credenciais inválidas',
  403: 'Sem permissão para esta operação',
  404: 'Recurso não encontrado',
  409: 'Conflito com o estado atual',
  422: 'Dados inválidos',
  429: 'Demasiados pedidos',
}

/** `Customer.NationalId` → `customer.nationalId`; `items[0].Name` → `items.0.name`. */
export function normalizeFieldPath(path: string): string {
  return path
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .filter(Boolean)
    .map((segment) => segment.charAt(0).toLowerCase() + segment.slice(1))
    .join('.')
}

function normalizeErrors(errors: unknown): Record<string, string[]> {
  if (!errors || typeof errors !== 'object') return {}
  const result: Record<string, string[]> = {}
  for (const [key, value] of Object.entries(errors as Record<string, unknown>)) {
    const messages = Array.isArray(value)
      ? value.filter((m): m is string => typeof m === 'string')
      : typeof value === 'string'
        ? [value]
        : []
    if (messages.length > 0) result[normalizeFieldPath(key)] = messages
  }
  return result
}

function parseRetryAfter(header: unknown): number | undefined {
  if (typeof header !== 'string' && typeof header !== 'number') return undefined
  const seconds = Number(header)
  return Number.isFinite(seconds) && seconds > 0 ? Math.ceil(seconds) : undefined
}

function asProblem(data: unknown): ProblemDetails | undefined {
  if (!data || typeof data !== 'object') return undefined
  return data
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error

  if (isAxiosError(error)) {
    const response = error.response
    if (!response) {
      return new ApiError({ status: 0, title: 'Sem ligação ao servidor', detail: undefined })
    }
    const problem = asProblem(response.data)
    const status = problem?.status ?? response.status
    const headers = response.headers as Record<string, unknown>
    return new ApiError({
      status,
      title: problem?.title ?? defaultTitles[status] ?? 'Erro inesperado',
      detail: problem?.detail,
      code: problem?.code,
      errors: normalizeErrors(problem?.errors),
      traceId: problem?.traceId,
      correlationId:
        problem?.correlationId ??
        (typeof headers['x-correlation-id'] === 'string' ? headers['x-correlation-id'] : undefined),
      retryAfterSeconds: parseRetryAfter(headers['retry-after']),
    })
  }

  if (error instanceof Error) {
    return new ApiError({ status: -1, title: 'Erro inesperado', detail: error.message })
  }
  return new ApiError({ status: -1, title: 'Erro inesperado' })
}

/**
 * Copies server-side validation errors onto React Hook Form fields.
 * Returns the messages that could not be attached to a known field.
 */
export function applyFieldErrors<T extends FieldValues>(
  error: ApiError,
  setError: UseFormSetError<T>,
  knownFields: readonly Path<T>[],
): string[] {
  const unmatched: string[] = []
  for (const [field, messages] of Object.entries(error.errors)) {
    const message = messages.join(' ')
    if ((knownFields as readonly string[]).includes(field)) {
      setError(field as Path<T>, { type: 'server', message })
    } else {
      unmatched.push(message)
    }
  }
  return unmatched
}
