import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'

import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { dashboardSummaryQueryOptions, revenueSeriesQueryOptions } from '@/features/dashboard/api'
import { ActivityCharts } from '@/features/dashboard/components/activity-charts'
import { KpiCards } from '@/features/dashboard/components/kpi-cards'
import type { DashboardPeriod, SeriesDays } from '@/features/dashboard/types'

const routeApi = getRouteApi('/_authenticated/_shell/')

const PERIODS: { value: DashboardPeriod; label: string }[] = [
  { value: 'today', label: 'Hoje' },
  { value: '7d', label: '7 dias' },
  { value: '30d', label: '30 dias' },
  { value: 'all', label: 'Tudo' },
]

const SERIES_DAYS: SeriesDays[] = [7, 30, 90]

export function DashboardPage() {
  const search = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const period = search.period ?? 'today'
  const days = search.days ?? 30

  const summary = useQuery(dashboardSummaryQueryOptions(period))
  const series = useQuery(revenueSeriesQueryOptions(days))

  return (
    <div className="space-y-6">
      <PageHeader
        title="Dashboard"
        description="Faturação, ocupação e movimento do parque."
        actions={
          <ToggleGroup
            type="single"
            variant="outline"
            aria-label="Período dos indicadores"
            value={period}
            onValueChange={(value) => {
              if (value) {
                void navigate({
                  search: (prev) => ({ ...prev, period: value as DashboardPeriod }),
                  replace: true,
                })
              }
            }}
          >
            {PERIODS.map((p) => (
              <ToggleGroupItem key={p.value} value={p.value}>
                {p.label}
              </ToggleGroupItem>
            ))}
          </ToggleGroup>
        }
      />

      {summary.isError ? (
        <QueryError error={summary.error} onRetry={() => void summary.refetch()} />
      ) : (
        <KpiCards summary={summary.data} />
      )}

      <section aria-labelledby="activity-heading" className="space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 id="activity-heading" className="text-lg font-semibold">
            Atividade
          </h2>
          <div className="flex items-center gap-2">
            <Label htmlFor="series-days" className="text-sm text-muted-foreground">
              Intervalo
            </Label>
            <Select
              value={String(days)}
              onValueChange={(value) => {
                void navigate({
                  search: (prev) => ({ ...prev, days: Number(value) as SeriesDays }),
                  replace: true,
                })
              }}
            >
              <SelectTrigger id="series-days" className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SERIES_DAYS.map((d) => (
                  <SelectItem key={d} value={String(d)}>
                    Últimos {d} dias
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
        {series.isError ? (
          <QueryError error={series.error} onRetry={() => void series.refetch()} />
        ) : (
          <ActivityCharts series={series.data} />
        )}
      </section>
    </div>
  )
}
