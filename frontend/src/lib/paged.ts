/** Server-side paginated list, as returned by every list endpoint of the API. */
export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export const PAGE_SIZES = [10, 20, 50, 100] as const

/** Drops empty values so they are not sent as `?key=` query parameters. */
export function compactParams<T extends object>(params: T): Partial<T> {
  return Object.fromEntries(
    Object.entries(params).filter(
      ([, value]) => value !== undefined && value !== null && value !== '',
    ),
  ) as Partial<T>
}
