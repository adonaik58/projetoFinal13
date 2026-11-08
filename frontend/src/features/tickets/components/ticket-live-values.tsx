import { useNow } from '@/lib/clock'
import { formatCurrency, formatDurationMinutes } from '@/lib/format'
import { amountFor, elapsedMinutes } from '@/lib/pricing'

import type { TicketDto } from '../types'

/** Open tickets keep counting on screen; closed tickets show the values stored by the server. */
export function TicketDuration({ ticket }: { ticket: TicketDto }) {
  const now = useNow()
  const minutes =
    ticket.status === 'Open' ? elapsedMinutes(ticket.entryAt, now) : ticket.durationMinutes
  return <>{formatDurationMinutes(minutes)}</>
}

export function TicketAmount({ ticket }: { ticket: TicketDto }) {
  const now = useNow()
  const amount =
    ticket.status === 'Open'
      ? amountFor(elapsedMinutes(ticket.entryAt, now), ticket.ratePerMinute, ticket.freeMinutes)
      : ticket.amount
  return <>{formatCurrency(amount)}</>
}
