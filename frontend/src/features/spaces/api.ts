import { queryOptions, useMutation, useQueryClient } from '@tanstack/react-query'

import { dashboardKeys } from '@/features/dashboard/api'
import { http } from '@/lib/http'
import { compactParams } from '@/lib/paged'
import { whenRealtimeDown } from '@/lib/realtime'

import type {
  BulkCreateSpacesRequest,
  BulkCreateSpacesResult,
  SpaceBoard,
  SpaceDto,
  SpaceFilters,
  SpaceStatus,
} from './types'

export const spaceKeys = {
  all: ['spaces'] as const,
  board: (filters: SpaceFilters) => [...spaceKeys.all, 'board', filters] as const,
}

/** Live board: refreshed by server push; polls every 10 s only while the realtime connection is down. */
export const spaceBoardQueryOptions = (filters: SpaceFilters) =>
  queryOptions({
    queryKey: spaceKeys.board(filters),
    queryFn: async ({ signal }) =>
      (await http.get<SpaceBoard>('/spaces', { params: compactParams(filters), signal })).data,
    refetchInterval: whenRealtimeDown(10_000),
    staleTime: 5_000,
  })

function useInvalidateSpaces() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: spaceKeys.all }),
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
    ])
}

export function useCreateSpace() {
  const invalidate = useInvalidateSpaces()
  return useMutation({
    mutationFn: async (code: string) => (await http.post<SpaceDto>('/spaces', { code })).data,
    meta: { successMessage: 'Espaço criado.', skipGlobalErrorToast: true },
    onSuccess: invalidate,
  })
}

export function useBulkCreateSpaces() {
  const invalidate = useInvalidateSpaces()
  return useMutation({
    mutationFn: async (request: BulkCreateSpacesRequest) =>
      (await http.post<BulkCreateSpacesResult>('/spaces/bulk', request)).data,
    meta: { skipGlobalErrorToast: true },
    onSuccess: invalidate,
  })
}

export function useUpdateSpaceStatus() {
  const invalidate = useInvalidateSpaces()
  return useMutation({
    mutationFn: async ({ id, status }: { id: number; status: Exclude<SpaceStatus, 'Occupied'> }) =>
      (await http.patch<SpaceDto>(`/spaces/${String(id)}/status`, { status })).data,
    meta: { successMessage: 'Estado do espaço atualizado.' },
    onSuccess: invalidate,
  })
}

export function useDeleteSpace() {
  const invalidate = useInvalidateSpaces()
  return useMutation({
    mutationFn: async (id: number) => {
      await http.delete(`/spaces/${String(id)}`)
    },
    meta: { successMessage: 'Espaço eliminado.' },
    onSuccess: invalidate,
  })
}
