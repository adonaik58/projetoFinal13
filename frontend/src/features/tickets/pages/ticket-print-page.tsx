import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'
import { Printer, X } from 'lucide-react'
import { useEffect, useRef } from 'react'

import { QueryError } from '@/components/common/query-error'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ticketDetailQueryOptions } from '@/features/tickets/api'
import { formatCurrency, formatDateTime, formatDurationMinutes } from '@/lib/format'

const routeApi = getRouteApi('/_authenticated/tickets/$ticketId/print')

function Line({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex justify-between gap-4 border-b border-dashed py-1 last:border-b-0">
      <dt>{label}</dt>
      <dd className="text-right font-medium">{value}</dd>
    </div>
  )
}

export function PrintTicketPage() {
  const { ticketId } = routeApi.useParams()
  const ticket = useQuery(ticketDetailQueryOptions(ticketId))
  const printed = useRef(false)

  useEffect(() => {
    document.title = ticket.data ? `Ticket ${ticket.data.code}` : 'Ticket'
    if (ticket.data && !printed.current) {
      printed.current = true
      // Let the browser lay out the ticket before opening the print dialog.
      const timer = setTimeout(() => {
        window.print()
      }, 150)
      return () => {
        clearTimeout(timer)
      }
    }
    return undefined
  }, [ticket.data])

  return (
    <main className="mx-auto max-w-xs p-4 text-sm print:max-w-none print:p-0">
      <div className="mb-4 flex justify-end gap-2 print:hidden">
        <Button
          size="sm"
          variant="outline"
          onClick={() => {
            window.print()
          }}
          disabled={!ticket.data}
        >
          <Printer aria-hidden="true" /> Imprimir
        </Button>
        <Button
          size="sm"
          variant="ghost"
          onClick={() => {
            window.close()
          }}
        >
          <X aria-hidden="true" /> Fechar
        </Button>
      </div>

      {ticket.isError ? (
        <QueryError error={ticket.error} onRetry={() => void ticket.refetch()} />
      ) : !ticket.data ? (
        <Skeleton className="h-96 w-full" />
      ) : (
        <article
          aria-label={`Ticket ${ticket.data.code}`}
          className="rounded-lg border bg-white p-4 text-black print:rounded-none print:border-0"
        >
          <header className="mb-3 text-center">
            <p className="text-xs tracking-widest uppercase">Xyami.Park</p>
            <p className="text-5xl font-bold">{ticket.data.spaceCode}</p>
            <p className="font-mono text-base tracking-wider">{ticket.data.code}</p>
          </header>
          <dl>
            <Line label="Cliente" value={ticket.data.customerName.toUpperCase()} />
            <Line
              label="Viatura"
              value={`${ticket.data.brandName}${ticket.data.modelName ? ` ${ticket.data.modelName}` : ''}`}
            />
            <Line
              label="Matrícula"
              value={<span className="font-mono">{ticket.data.plate}</span>}
            />
            <Line label="Entrada" value={formatDateTime(ticket.data.entryAt)} />
            {ticket.data.exitAt && (
              <>
                <Line label="Saída" value={formatDateTime(ticket.data.exitAt)} />
                <Line label="Duração" value={formatDurationMinutes(ticket.data.durationMinutes)} />
                <Line label="Total" value={formatCurrency(ticket.data.amount)} />
              </>
            )}
            <Line label="Operador" value={ticket.data.closedBy ?? ticket.data.openedBy} />
          </dl>
          <footer className="mt-3 text-center text-xs">
            Tarifa: {formatCurrency(ticket.data.ratePerMinute)}/min ·{' '}
            {formatDurationMinutes(ticket.data.freeMinutes)} grátis
          </footer>
        </article>
      )}
    </main>
  )
}
