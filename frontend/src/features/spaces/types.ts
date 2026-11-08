export type SpaceStatus = 'Free' | 'Occupied' | 'Maintenance'

export interface Occupancy {
  ticketId: number
  ticketCode: string
  entryAt: string
  elapsedMinutes: number
  currentAmount: number
  customer: {
    id: number
    fullName: string
    nationalId: string
    birthDate: string
    age: number
    phone: string | null
  }
  vehicle: {
    id: number
    plate: string
    color: string
    brandId: number
    brandName: string
    modelId: number | null
    modelName: string | null
  }
  /** Free period from the tariff snapshot taken at entry (0 = none). */
  freeMinutes: number
  /** End of the free period (UTC ISO); equals `entryAt` when there is none. */
  freeUntil: string
  /** Operator who registered the entry. */
  openedBy: string
  /** Tariff snapshot taken at entry, used to keep the amount live on screen. */
  ratePerMinute: number
}

export interface SpaceDto {
  id: number
  code: string
  section: string
  number: number
  status: SpaceStatus
  occupancy: Occupancy | null
  /** Who added the space; `null` when unknown (imported or older spaces). */
  createdBy: string | null
  createdAt: string
}

export interface SpaceSummary {
  total: number
  free: number
  occupied: number
  maintenance: number
}

export interface SpaceBoard {
  items: SpaceDto[]
  summary: SpaceSummary
  serverTime: string
}

export interface SpaceFilters {
  search?: string | undefined
  status?: SpaceStatus | undefined
  plate?: string | undefined
  sortDir?: 'asc' | 'desc' | undefined
}

export interface BulkCreateSpacesRequest {
  sections: string[]
  fromNumber: number
  toNumber: number
}

export interface BulkCreateSpacesResult {
  created: number
  skipped: number
}
