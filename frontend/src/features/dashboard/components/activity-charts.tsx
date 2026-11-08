import { Area, AreaChart, Bar, BarChart, CartesianGrid, XAxis, YAxis } from 'recharts'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from '@/components/ui/chart'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency, formatNumber, parseDateOnly } from '@/lib/format'

import type { RevenueSeries } from '../types'

const revenueConfig = {
  revenue: { label: 'Receita', color: 'var(--chart-1)' },
} satisfies ChartConfig

const trafficConfig = {
  entries: { label: 'Entradas', color: 'var(--chart-2)' },
  exits: { label: 'Saídas', color: 'var(--chart-3)' },
} satisfies ChartConfig

const shortDate = new Intl.DateTimeFormat('pt-AO', { day: '2-digit', month: 'short' })

function labelFormatter(value: unknown) {
  return typeof value === 'string' ? dayLabel(value) : ''
}

function dayLabel(value: string) {
  const date = parseDateOnly(value)
  return date ? shortDate.format(date) : value
}

export function ActivityCharts({ series }: { series: RevenueSeries | undefined }) {
  if (!series) {
    return (
      <div className="grid gap-4 lg:grid-cols-2" aria-busy="true">
        <Skeleton className="h-80 rounded-xl" />
        <Skeleton className="h-80 rounded-xl" />
      </div>
    )
  }

  const totalRevenue = series.points.reduce((sum, point) => sum + point.revenue, 0)
  const totalEntries = series.points.reduce((sum, point) => sum + point.entries, 0)

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Receita diária</CardTitle>
          <CardDescription>
            {formatCurrency(totalRevenue)} nos últimos {series.days} dias
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ChartContainer
            config={revenueConfig}
            className="aspect-auto h-64 w-full"
            role="img"
            aria-label={`Gráfico de receita diária: total ${formatCurrency(totalRevenue)} nos últimos ${String(series.days)} dias`}
          >
            <AreaChart data={series.points} margin={{ left: 8, right: 8 }}>
              <CartesianGrid vertical={false} />
              <XAxis
                dataKey="date"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                minTickGap={24}
                tickFormatter={dayLabel}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                width={72}
                tickFormatter={(value: number) => formatNumber(value)}
              />
              <ChartTooltip
                content={
                  <ChartTooltipContent
                    labelFormatter={labelFormatter}
                    formatter={(value) => formatCurrency(Number(value))}
                  />
                }
              />
              <Area
                dataKey="revenue"
                type="monotone"
                fill="var(--color-revenue)"
                fillOpacity={0.2}
                stroke="var(--color-revenue)"
                strokeWidth={2}
              />
            </AreaChart>
          </ChartContainer>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Entradas e saídas</CardTitle>
          <CardDescription>
            {formatNumber(totalEntries)} entradas nos últimos {series.days} dias
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ChartContainer
            config={trafficConfig}
            className="aspect-auto h-64 w-full"
            role="img"
            aria-label={`Gráfico de entradas e saídas por dia nos últimos ${String(series.days)} dias`}
          >
            <BarChart data={series.points} margin={{ left: 8, right: 8 }}>
              <CartesianGrid vertical={false} />
              <XAxis
                dataKey="date"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                minTickGap={24}
                tickFormatter={dayLabel}
              />
              <YAxis tickLine={false} axisLine={false} allowDecimals={false} width={40} />
              <ChartTooltip content={<ChartTooltipContent labelFormatter={labelFormatter} />} />
              <ChartLegend content={<ChartLegendContent />} />
              <Bar dataKey="entries" fill="var(--color-entries)" radius={4} />
              <Bar dataKey="exits" fill="var(--color-exits)" radius={4} />
            </BarChart>
          </ChartContainer>
        </CardContent>
      </Card>
    </div>
  )
}
