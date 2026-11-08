import type { QueryClient, QueryKey, UseQueryOptions } from '@tanstack/react-query'

import type { ApiError } from '@/lib/api-error'

/**
 * Warms the cache from a route loader without blocking navigation; errors are left for the
 * component's own query to surface. Observer-only options (refetchInterval, placeholderData…)
 * are not forwarded.
 */
export function prefetch<TData, TKey extends QueryKey>(
  queryClient: QueryClient,
  options: Pick<
    UseQueryOptions<TData, ApiError, TData, TKey>,
    'queryKey' | 'queryFn' | 'staleTime'
  >,
): void {
  const { queryKey, queryFn, staleTime } = options
  queryClient
    .query({
      queryKey,
      ...(queryFn ? { queryFn } : {}),
      ...(staleTime !== undefined ? { staleTime } : {}),
    })
    .catch(() => undefined)
}
