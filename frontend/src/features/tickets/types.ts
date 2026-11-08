export type TicketStatus = 'Open' | 'Closed'

export interface TicketDto {
  id: number
  code: string
  status: TicketStatus
  spaceId: number
  spaceCode: string
  customerName: string
  nationalId: string
  customerAge: number
  plate: string
  brandName: string
  modelName: string | null
  color: string
  entryAt: string
  exitAt: string | null
  durationMinutes: number
  amount: number
  openedBy: string
  closedBy: string | null
}

export interface TicketDetailDto extends TicketDto {
  customerBirthDate: string
  ratePerMinute: number
  freeMinutes: number
}

export interface CreateTicketRequest {
  spaceId: number
  customer: { fullName: string; nationalId: string; birthDate: string; phone: string | null }
  vehicle: { plate: string; brandId: number; modelId: number | null; color: string }
}

export const TICKET_SORT_FIELDS = [
  'entryAt',
  'exitAt',
  'amount',
  'duration',
  'customerName',
  'spaceCode',
  'plate',
  'brand',
  'model',
] as const
export type TicketSortField = (typeof TICKET_SORT_FIELDS)[number]

/** Query parameters accepted by `GET /tickets`. */
export interface TicketQueryParams {
  page: number
  pageSize: number
  status?: TicketStatus | undefined
  search?: string | undefined
  customerName?: string | undefined
  nationalId?: string | undefined
  plate?: string | undefined
  brandId?: number | undefined
  modelId?: number | undefined
  spaceCode?: string | undefined
  from?: string | undefined
  to?: string | undefined
  sortBy: TicketSortField
  sortDir: 'asc' | 'desc'
}
