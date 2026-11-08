export interface AuditLogDto {
  id: number
  occurredAt: string
  userId: number | null
  username: string | null
  action: string
  entityType: string | null
  entityId: string | null
  ipAddress: string | null
  correlationId: string | null
  details: string | null
}

export interface AuditQueryParams {
  page: number
  pageSize: number
  action?: string | undefined
  username?: string | undefined
  from?: string | undefined
  to?: string | undefined
}
