import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'

import { requirePermission } from '@/features/auth/guards'
import { PrintTicketPage } from '@/features/tickets/pages/ticket-print-page'

export const Route = createFileRoute('/_authenticated/tickets/$ticketId/print')({
  params: {
    parse: (params) => ({ ticketId: z.coerce.number().int().positive().parse(params.ticketId) }),
    stringify: (params) => ({ ticketId: String(params.ticketId) }),
  },
  beforeLoad: requirePermission('tickets.view'),
  staticData: { title: 'Imprimir ticket' },
  component: PrintTicketPage,
})
