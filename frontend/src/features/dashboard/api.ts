import { queryOptions } from '@tanstack/react-query'

import { http } from '@/lib/http'

import type { DashboardPeriod, DashboardSummary, RevenueSeries, SeriesDays } from './types'

export const dashboardKeys = {
  all: ['dashboard'] as const,
  summary: (period: DashboardPeriod) => [...dashboardKeys.all, 'summary', period] as const,
  series: (days: SeriesDays) => [...dashboardKeys.all, 'series', days] as const,
}

export const dashboardSummaryQueryOptions = (period: DashboardPeriod) =>
  queryOptions({
    queryKey: dashboardKeys.summary(period),
    queryFn: async ({ signal }) =>
      (await http.get<DashboardSummary>('/dashboard/summary', { params: { period }, signal })).data,
    refetchInterval: 30_000,
  })

export const revenueSeriesQueryOptions = (days: SeriesDays) =>
  queryOptions({
    queryKey: dashboardKeys.series(days),
    queryFn: async ({ signal }) =>
      (await http.get<RevenueSeries>('/dashboard/revenue-series', { params: { days }, signal }))
        .data,
    refetchInterval: 60_000,
  })
