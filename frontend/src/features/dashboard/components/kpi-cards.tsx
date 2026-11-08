import {
  Banknote,
  CarFront,
  DoorClosed,
  DoorOpen,
  Hourglass,
  Ticket,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency, formatDurationMinutes, formatNumber } from '@/lib/format'

import type { DashboardSummary } from '../types'

interface Kpi {
  label: string
  value: string
  hint?: string
  icon: LucideIcon
}

function toKpis(summary: DashboardSummary): Kpi[] {
  const { spaces } = summary
  const occupancyRate = spaces.total > 0 ? Math.round((spaces.occupied / spaces.total) * 100) : 0
  return [
    { label: 'Faturação', value: formatCurrency(summary.revenue), icon: Banknote },
    {
      label: 'Espaços livres',
      value: formatNumber(spaces.free),
      hint: `de ${formatNumber(spaces.total)} espaços`,
      icon: CarFront,
    },
    {
      label: 'Espaços ocupados',
      value: formatNumber(spaces.occupied),
      hint: `${occupancyRate}% de ocupação`,
      icon: Ticket,
    },
    {
      label: 'Indisponíveis',
      value: formatNumber(spaces.maintenance),
      icon: Wrench,
    },
    { label: 'Entradas', value: formatNumber(summary.entries), icon: DoorClosed },
    { label: 'Saídas', value: formatNumber(summary.exits), icon: DoorOpen },
    {
      label: 'Permanência média',
      value: formatDurationMinutes(summary.averageStayMinutes),
      icon: Hourglass,
    },
    { label: 'Tickets abertos', value: formatNumber(summary.openTickets), icon: Ticket },
  ]
}

export function KpiCards({ summary }: { summary: DashboardSummary | undefined }) {
  if (!summary) {
    return (
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-busy="true">
        {Array.from({ length: 8 }, (_, i) => (
          <Skeleton key={i} className="h-28 rounded-xl" />
        ))}
      </div>
    )
  }

  return (
    <ul className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Indicadores">
      {toKpis(summary).map((kpi) => (
        <li key={kpi.label}>
          <Card className="h-full gap-2">
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {kpi.label}
              </CardTitle>
              <kpi.icon className="size-4 text-muted-foreground" aria-hidden="true" />
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-semibold tabular-nums">{kpi.value}</p>
              {kpi.hint && <p className="text-xs text-muted-foreground">{kpi.hint}</p>}
            </CardContent>
          </Card>
        </li>
      ))}
    </ul>
  )
}
