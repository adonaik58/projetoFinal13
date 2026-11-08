import { useQuery } from '@tanstack/react-query'
import { Clock, Coins } from 'lucide-react'

import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { tariffQueryOptions } from '@/features/settings/api'
import { formatCurrency, formatDurationMinutes } from '@/lib/format'

export function TariffPage() {
  const tariff = useQuery(tariffQueryOptions())

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tarifário"
        description="Tarifa em vigor e exemplos de valores calculados pelo servidor."
      />
      {tariff.isError ? (
        <QueryError error={tariff.error} onRetry={() => void tariff.refetch()} />
      ) : !tariff.data ? (
        <Skeleton className="h-96 rounded-xl" />
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Renda por minuto
                </CardTitle>
                <Coins className="size-4 text-muted-foreground" aria-hidden="true" />
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold">
                  {formatCurrency(tariff.data.ratePerMinute)}
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Período grátis
                </CardTitle>
                <Clock className="size-4 text-muted-foreground" aria-hidden="true" />
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold">
                  {tariff.data.freeHours === 0
                    ? 'Sem período grátis'
                    : formatDurationMinutes(tariff.data.freeHours * 60)}
                </p>
              </CardContent>
            </Card>
          </div>
          <Card>
            <CardHeader>
              <CardTitle>Exemplos</CardTitle>
              <CardDescription>
                Estadias abaixo do período grátis não pagam; acima, paga-se cada minuto além do
                período grátis.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableCaption className="sr-only">Exemplos de valores por duração</TableCaption>
                <TableHeader>
                  <TableRow>
                    <TableHead>Duração da estadia</TableHead>
                    <TableHead className="text-right">Valor a pagar</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {tariff.data.examples.map((example) => (
                    <TableRow key={example.minutes}>
                      <TableCell>{formatDurationMinutes(example.minutes)}</TableCell>
                      <TableCell className="text-right font-medium tabular-nums">
                        {formatCurrency(example.amount)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  )
}
