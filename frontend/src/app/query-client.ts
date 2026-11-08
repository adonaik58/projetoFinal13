import { MutationCache, QueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { type ApiError, toApiError } from '@/lib/api-error'

declare module '@tanstack/react-query' {
  interface Register {
    defaultError: ApiError
    mutationMeta: {
      /** Message shown on success. */
      successMessage?: string
      /** Set when the component renders the error itself (e.g. inline form errors). */
      skipGlobalErrorToast?: boolean
    }
  }
}

const MAX_RETRIES = 2

export function createQueryClient() {
  return new QueryClient({
    mutationCache: new MutationCache({
      onSuccess: (_data, _variables, _context, mutation) => {
        const message = mutation.meta?.successMessage
        if (message) toast.success(message)
      },
      onError: (error, _variables, _context, mutation) => {
        if (mutation.meta?.skipGlobalErrorToast) return
        toast.error(toApiError(error).userMessage)
      },
    }),
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        gcTime: 5 * 60_000,
        refetchOnWindowFocus: true,
        retry: (failureCount, error) => {
          const apiError = toApiError(error)
          // 4xx responses will not succeed on retry (except rate limiting).
          if (apiError.isClientError && apiError.status !== 429) return false
          return failureCount < MAX_RETRIES
        },
      },
      mutations: { retry: false },
    },
  })
}
