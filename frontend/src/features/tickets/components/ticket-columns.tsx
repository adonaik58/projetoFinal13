import type { ColumnDef } from '@tanstack/react-table'
import { Eye } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { formatCurrency, formatDateTime, formatDurationMinutes } from '@/lib/format'

import type { TicketDto, TicketStatus } from '../types'

export function ticketColumns(
  status: TicketStatus,
  onView: (ticket: TicketDto) => void,
): ColumnDef<TicketDto>[] {
  const columns: (ColumnDef<TicketDto> | false)[] = [
    {
      id: 'code',
      header: 'Ticket',
      enableSorting: false,
      cell: ({ row }) => <span className="font-mono text-xs">{row.original.code}</span>,
    },
    {
      id: 'customerName',
      header: 'Cliente',
      cell: ({ row }) => (
        <div className="min-w-36">
          <div className="font-medium">{row.original.customerName}</div>
          <div className="font-mono text-xs text-muted-foreground">{row.original.nationalId}</div>
        </div>
      ),
    },
    {
      id: 'spaceCode',
      header: 'Espaço',
      cell: ({ row }) => <span className="font-semibold">{row.original.spaceCode}</span>,
    },
    {
      id: 'brand',
      header: 'Marca',
      cell: ({ row }) => row.original.brandName,
    },
    {
      id: 'model',
      header: 'Modelo',
      cell: ({ row }) => row.original.modelName ?? '—',
    },
    {
      id: 'plate',
      header: 'Matrícula',
      cell: ({ row }) => (
        <span className="inline-flex items-center gap-2 font-mono">
          <span
            className="inline-block size-3 rounded-full border"
            style={{ backgroundColor: row.original.color }}
            aria-hidden="true"
          />
          {row.original.plate}
        </span>
      ),
    },
    {
      id: 'entryAt',
      header: 'Entrada',
      cell: ({ row }) => (
        <span className="whitespace-nowrap">{formatDateTime(row.original.entryAt)}</span>
      ),
    },
    status === 'Closed' && {
      id: 'exitAt',
      header: 'Saída',
      cell: ({ row }) => (
        <span className="whitespace-nowrap">{formatDateTime(row.original.exitAt)}</span>
      ),
    },
    {
      id: 'duration',
      header: status === 'Open' ? 'Tempo decorrido' : 'Tempo ocupado',
      cell: ({ row }) => (
        <span className="whitespace-nowrap">
          {formatDurationMinutes(row.original.durationMinutes)}
        </span>
      ),
    },
    {
      id: 'amount',
      header: status === 'Open' ? 'Acumulado' : 'Total',
      cell: ({ row }) => (
        <span className="font-semibold whitespace-nowrap tabular-nums">
          {formatCurrency(row.original.amount)}
        </span>
      ),
    },
    {
      id: 'actions',
      header: () => <span className="sr-only">Ações</span>,
      enableSorting: false,
      cell: ({ row }) => (
        <Button
          variant="ghost"
          size="icon-sm"
          aria-label={`Ver detalhes do ticket ${row.original.code}`}
          onClick={() => {
            onView(row.original)
          }}
        >
          <Eye aria-hidden="true" />
        </Button>
      ),
    },
  ]
  return columns.filter((column): column is ColumnDef<TicketDto> => column !== false)
}
