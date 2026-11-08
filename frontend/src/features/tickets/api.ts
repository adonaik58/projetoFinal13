import { keepPreviousData, queryOptions, useMutation, useQueryClient } from '@tanstack/react-query'

import { dashboardKeys } from '@/features/dashboard/api'
import { spaceKeys } from '@/features/spaces/api'
import { http } from '@/lib/http'
import { compactParams, type Paged } from '@/lib/paged'

import type { CreateTicketRequest, TicketDetailDto, TicketDto, TicketQueryParams } from './types'

export const ticketKeys = {
  all: ['tickets'] as const,
  lists: () => [...ticketKeys.all, 'list'] as const,
  list: (params: TicketQueryParams) => [...ticketKeys.lists(), params] as const,
  detail: (id: number) => [...ticketKeys.all, 'detail', id] as const,
}

export const ticketsQueryOptions = (params: TicketQueryParams) =>
  queryOptions({
    queryKey: ticketKeys.list(params),
    queryFn: async ({ signal }) =>
      (await http.get<Paged<TicketDto>>('/tickets', { params: compactParams(params), signal }))
        .data,
    placeholderData: keepPreviousData,
    // Open tickets show amounts that grow with time.
    refetchInterval: params.status === 'Open' ? 30_000 : false,
  })

export const ticketDetailQueryOptions = (id: number) =>
  queryOptions({
    queryKey: ticketKeys.detail(id),
    queryFn: async ({ signal }) =>
      (await http.get<TicketDetailDto>(`/tickets/${String(id)}`, { signal })).data,
  })

function useInvalidateParking() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: spaceKeys.all }),
      queryClient.invalidateQueries({ queryKey: ticketKeys.all }),
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
    ])
}

/** Check-in. A fresh Idempotency-Key per submission lets the API discard duplicate clicks/retries. */
export function useCreateTicket() {
  const invalidate = useInvalidateParking()
  return useMutation({
    mutationFn: async (request: CreateTicketRequest) =>
      (
        await http.post<TicketDetailDto>('/tickets', request, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data,
    meta: { skipGlobalErrorToast: true },
    onSuccess: invalidate,
  })
}

/** Check-out. */
export function useCloseTicket() {
  const invalidate = useInvalidateParking()
  return useMutation({
    mutationFn: async (ticketId: number) =>
      (
        await http.post<TicketDetailDto>(`/tickets/${String(ticketId)}/close`, null, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data,
    onSuccess: invalidate,
  })
}
