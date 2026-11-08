import { keepPreviousData, queryOptions } from '@tanstack/react-query'

import { http } from '@/lib/http'
import { compactParams, type Paged } from '@/lib/paged'

import type { AuditLogDto, AuditQueryParams } from './types'

export const auditKeys = {
  all: ['audit-logs'] as const,
  list: (params: AuditQueryParams) => [...auditKeys.all, params] as const,
}

export const auditLogsQueryOptions = (params: AuditQueryParams) =>
  queryOptions({
    queryKey: auditKeys.list(params),
    queryFn: async ({ signal }) =>
      (await http.get<Paged<AuditLogDto>>('/audit-logs', { params: compactParams(params), signal }))
        .data,
    placeholderData: keepPreviousData,
  })
