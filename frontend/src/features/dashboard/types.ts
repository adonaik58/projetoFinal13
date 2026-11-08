export type DashboardPeriod = 'today' | '7d' | '30d' | 'all'

export interface DashboardSummary {
  period: DashboardPeriod
  from: string | null
  to: string
  revenue: number
  entries: number
  exits: number
  averageStayMinutes: number
  openTickets: number
  spaces: { total: number; free: number; occupied: number; maintenance: number }
}

export type SeriesDays = 7 | 30 | 90

export interface RevenueSeries {
  days: number
  points: { date: string; revenue: number; entries: number; exits: number }[]
}
