import { Printer, TicketX } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'

import { ConfirmDialog } from '@/components/common/confirm-dialog'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { useAuth } from '@/features/auth/use-auth'
import { useCloseTicket } from '@/features/tickets/api'
import { openTicketPrint } from '@/features/tickets/print'
import { formatCurrency, formatDate, formatDateTime, formatElapsedSeconds } from '@/lib/format'

import type { Occupancy } from '../types'
import { FreeTimeCountdown } from './free-time-countdown'

function Detail({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-start justify-between gap-4 py-1.5">
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="text-right text-sm font-medium">{children}</dd>
    </div>
  )
}

export function OccupancyDetails({
  spaceCode,
  occupancy,
  now,
  onClosed,
}: {
  spaceCode: string
  occupancy: Occupancy
  now: number
  onClosed: () => void
}) {
  const { can } = useAuth()
  const [confirmOpen, setConfirmOpen] = useState(false)
  const closeTicket = useCloseTicket()
  const elapsedSeconds = (now - Date.parse(occupancy.entryAt)) / 1000

  function handleClose() {
    closeTicket.mutate(occupancy.ticketId, {
      onSuccess: (ticket) => {
        setConfirmOpen(false)
        toast.success(`Ticket ${ticket.code} fechado.`, {
          description: `Total a pagar: ${formatCurrency(ticket.amount)}`,
          duration: 10_000,
          action: {
            label: 'Imprimir',
            onClick: () => openTicketPrint(ticket.id),
          },
        })
        onClosed()
      },
    })
  }

  return (
    <div className="flex flex-col gap-5">
      <div className="rounded-xl border bg-muted/40 p-4 text-center">
        <p className="text-sm text-muted-foreground">Valor acumulado</p>
        <p className="text-3xl font-semibold tabular-nums">
          {formatCurrency(occupancy.currentAmount)}
        </p>
        <p className="text-sm text-muted-foreground tabular-nums" aria-live="off">
          {formatElapsedSeconds(elapsedSeconds)} desde {formatDateTime(occupancy.entryAt)}
        </p>
      </div>

      <FreeTimeCountdown occupancy={occupancy} now={now} />

      <section aria-labelledby="occupancy-customer">
        <h3 id="occupancy-customer" className="mb-1 text-sm font-semibold">
          Cliente
        </h3>
        <dl className="divide-y">
          <Detail label="Nome">{occupancy.customer.fullName}</Detail>
          <Detail label="Idade">
            {occupancy.customer.age} anos ({formatDate(occupancy.customer.birthDate)})
          </Detail>
          <Detail label="Bilhete de Identidade">
            <span className="font-mono">{occupancy.customer.nationalId}</span>
          </Detail>
          <Detail label="Contacto">
            {occupancy.customer.phone ? (
              <a
                href={`tel:${occupancy.customer.phone}`}
                className="font-mono underline-offset-4 hover:underline"
              >
                {occupancy.customer.phone}
              </a>
            ) : (
              <span className="text-muted-foreground">Não indicado</span>
            )}
          </Detail>
        </dl>
      </section>

      <section aria-labelledby="occupancy-vehicle">
        <h3 id="occupancy-vehicle" className="mb-1 text-sm font-semibold">
          Viatura
        </h3>
        <dl className="divide-y">
          <Detail label="Matrícula">
            <span className="font-mono">{occupancy.vehicle.plate}</span>
          </Detail>
          <Detail label="Marca">{occupancy.vehicle.brandName}</Detail>
          <Detail label="Modelo">{occupancy.vehicle.modelName ?? 'Não indicado'}</Detail>
          <Detail label="Cor">
            <span className="inline-flex items-center gap-2">
              <span
                className="inline-block size-4 rounded border"
                style={{ backgroundColor: occupancy.vehicle.color }}
                aria-hidden="true"
              />
              <span className="font-mono uppercase">{occupancy.vehicle.color}</span>
            </span>
          </Detail>
          <Detail label="Ticket">
            <span className="font-mono">{occupancy.ticketCode}</span>
          </Detail>
          <Detail label="Entrada registada por">{occupancy.openedBy}</Detail>
        </dl>
      </section>

      <Separator />

      <div className="flex flex-col gap-2 sm:flex-row">
        <Button
          variant="outline"
          className="flex-1"
          onClick={() => {
            if (!openTicketPrint(occupancy.ticketId)) {
              toast.info('Permita janelas pop-up para imprimir o ticket.')
            }
          }}
        >
          <Printer aria-hidden="true" /> Reimprimir ticket
        </Button>
        {can('tickets.close') && (
          <Button
            variant="destructive"
            className="flex-1"
            onClick={() => {
              setConfirmOpen(true)
            }}
          >
            <TicketX aria-hidden="true" /> Fechar ticket
          </Button>
        )}
      </div>

      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={`Fechar o ticket do espaço ${spaceCode}?`}
        description={
          <>
            Valor acumulado neste momento:{' '}
            <strong>{formatCurrency(occupancy.currentAmount)}</strong>. O valor final é calculado
            pelo servidor no momento do fecho e o espaço fica livre.
          </>
        }
        confirmLabel="Fechar ticket"
        destructive
        pending={closeTicket.isPending}
        onConfirm={handleClose}
      />
    </div>
  )
}
