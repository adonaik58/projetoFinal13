import { createFileRoute } from '@tanstack/react-router'

import { requirePermission } from '@/features/auth/guards'
import { TicketsPage } from '@/features/tickets/pages/tickets-page'
import { ticketsSearchSchema } from '@/features/tickets/search'

export const Route = createFileRoute('/_authenticated/_shell/tickets')({
  validateSearch: ticketsSearchSchema,
  beforeLoad: requirePermission('tickets.view'),
  staticData: { title: 'Tickets' },
  component: TicketsPage,
})
