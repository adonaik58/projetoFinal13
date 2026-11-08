import { useQuery } from '@tanstack/react-query'
import { Printer } from 'lucide-react'
import { toast } from 'sonner'

import { QueryError } from '@/components/common/query-error'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency, formatDate, formatDateTime, formatDurationMinutes } from '@/lib/format'

import { ticketDetailQueryOptions } from '../api'
import { openTicketPrint } from '../print'

function Row({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex justify-between gap-4 py-1.5 text-sm">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="text-right font-medium">{value}</dd>
    </div>
  )
}

export function TicketDetailDialog({
  ticketId,
  onOpenChange,
}: {
  ticketId: number | null
  onOpenChange: (open: boolean) => void
}) {
  const detail = useQuery({
    ...ticketDetailQueryOptions(ticketId ?? 0),
    enabled: ticketId !== null,
  })
  const ticket = detail.data

  return (
    <Dialog open={ticketId !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Ticket {ticket?.code ?? ''}
            {ticket && (
              <Badge variant={ticket.status === 'Open' ? 'default' : 'secondary'}>
                {ticket.status === 'Open' ? 'Ativo' : 'Fechado'}
              </Badge>
            )}
          </DialogTitle>
          <DialogDescription>
            {ticket ? `Espaço ${ticket.spaceCode}` : 'A carregar os detalhes do ticket…'}
          </DialogDescription>
        </DialogHeader>
        {detail.isError ? (
          <QueryError error={detail.error} onRetry={() => void detail.refetch()} />
        ) : !ticket ? (
          <div className="space-y-2">
            {Array.from({ length: 8 }, (_, i) => (
              <Skeleton key={i} className="h-5 w-full" />
            ))}
          </div>
        ) : (
          <dl className="divide-y">
            <Row label="Cliente" value={ticket.customerName} />
            <Row
              label="Idade"
              value={`${String(ticket.customerAge)} anos (${formatDate(ticket.customerBirthDate)})`}
            />
            <Row
              label="Bilhete de Identidade"
              value={<span className="font-mono">{ticket.nationalId}</span>}
            />
            <Row label="Marca" value={ticket.brandName} />
            <Row label="Modelo" value={ticket.modelName ?? 'Não indicado'} />
            <Row
              label="Cor"
              value={
                <span className="inline-flex items-center gap-2">
                  <span
                    className="inline-block size-3 rounded-full border"
                    style={{ backgroundColor: ticket.color }}
                    aria-hidden="true"
                  />
                  <span className="font-mono uppercase">{ticket.color}</span>
                </span>
              }
            />
            <Row label="Matrícula" value={<span className="font-mono">{ticket.plate}</span>} />
            <Row label="Entrada" value={formatDateTime(ticket.entryAt)} />
            <Row label="Saída" value={formatDateTime(ticket.exitAt)} />
            <Row label="Tempo ocupado" value={formatDurationMinutes(ticket.durationMinutes)} />
            <Row
              label="Tarifa"
              value={`${formatCurrency(ticket.ratePerMinute)}/min · ${formatDurationMinutes(ticket.freeMinutes)} grátis`}
            />
            <Row label="Aberto por" value={ticket.openedBy} />
            {ticket.closedBy && <Row label="Fechado por" value={ticket.closedBy} />}
            <div className="flex justify-between gap-4 pt-3">
              <dt className="text-base font-semibold">
                {ticket.status === 'Open' ? 'Acumulado' : 'Total'}
              </dt>
              <dd className="text-xl font-semibold tabular-nums">
                {formatCurrency(ticket.amount)}
              </dd>
            </div>
          </dl>
        )}
        <DialogFooter>
          <Button
            variant="outline"
            disabled={!ticket}
            onClick={() => {
              if (ticket && !openTicketPrint(ticket.id)) {
                toast.info('Permita janelas pop-up para imprimir o ticket.')
              }
            }}
          >
            <Printer aria-hidden="true" /> Imprimir
          </Button>
          <Button
            onClick={() => {
              onOpenChange(false)
            }}
          >
            OK
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
