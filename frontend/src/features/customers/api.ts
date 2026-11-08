import { queryOptions } from '@tanstack/react-query'

import { http } from '@/lib/http'

import type { CustomerLookup } from './types'

export const customerKeys = {
  all: ['customers'] as const,
  lookup: (nationalId: string) => [...customerKeys.all, 'lookup', nationalId] as const,
}

/** Looks up a returning customer by BI. A 404 means "new customer" and resolves to `null`. */
export const customerLookupQueryOptions = (nationalId: string, enabled: boolean) =>
  queryOptions({
    queryKey: customerKeys.lookup(nationalId),
    queryFn: async ({ signal }): Promise<CustomerLookup | null> => {
      const response = await http.get<CustomerLookup>('/customers/lookup', {
        params: { nationalId },
        signal,
        validateStatus: (status) => (status >= 200 && status < 300) || status === 404,
      })
      return response.status === 404 ? null : response.data
    },
    enabled,
    staleTime: 60_000,
  })
