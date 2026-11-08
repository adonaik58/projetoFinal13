import { z } from 'zod'

import { customRange, DATE_PRESETS, presetRange } from '@/lib/date-range'
import { optionalDateOnly, optionalPage, optionalPageSize, optionalText } from '@/lib/search-params'

import { TICKET_SORT_FIELDS, type TicketQueryParams, type TicketStatus } from './types'

const optionalId = z.coerce.number().int().positive().optional().catch(undefined)

/** URL search params of the /tickets page. Invalid values are dropped instead of failing. */
export const ticketsSearchSchema = z.object({
  status: z.enum(['Open', 'Closed']).optional().catch(undefined),
  page: optionalPage(),
  pageSize: optionalPageSize(),
  search: optionalText(),
  customerName: optionalText(),
  nationalId: optionalText(14),
  plate: optionalText(20),
  brandId: optionalId,
  modelId: optionalId,
  spaceCode: optionalText(10),
  preset: z.enum(DATE_PRESETS).optional().catch(undefined),
  fromDate: optionalDateOnly(),
  toDate: optionalDateOnly(),
  sortBy: z.enum(TICKET_SORT_FIELDS).optional().catch(undefined),
  sortDir: z.enum(['asc', 'desc']).optional().catch(undefined),
})
export type TicketsSearch = z.infer<typeof ticketsSearchSchema>

export const DEFAULT_PAGE_SIZE = 20

export function statusOf(search: TicketsSearch): TicketStatus {
  return search.status ?? 'Open'
}

export function defaultSortFor(status: TicketStatus): TicketQueryParams['sortBy'] {
  return status === 'Closed' ? 'exitAt' : 'entryAt'
}

/** Translates the page's URL state into `GET /tickets` query parameters. */
export function buildTicketQuery(search: TicketsSearch, now: Date = new Date()): TicketQueryParams {
  const status = statusOf(search)
  const range = search.preset
    ? presetRange(search.preset, now)
    : customRange(search.fromDate, search.toDate)
  return {
    page: search.page ?? 1,
    pageSize: search.pageSize ?? DEFAULT_PAGE_SIZE,
    status,
    search: search.search,
    customerName: search.customerName,
    nationalId: search.nationalId?.toUpperCase(),
    plate: search.plate?.toUpperCase(),
    brandId: search.brandId,
    modelId: search.modelId,
    spaceCode: search.spaceCode?.toUpperCase(),
    from: range.from,
    to: range.to,
    sortBy: search.sortBy ?? defaultSortFor(status),
    sortDir: search.sortDir ?? 'desc',
  }
}

/** Number of active advanced filters (for the "Filtros (n)" button). */
export function activeFilterCount(search: TicketsSearch): number {
  return [
    search.customerName,
    search.nationalId,
    search.plate,
    search.brandId,
    search.modelId,
    search.spaceCode,
    search.preset ?? search.fromDate ?? search.toDate,
  ].filter((value) => value !== undefined && value !== '').length
}
