import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'
import { useCallback, useMemo, useState } from 'react'

import { DataTable } from '@/components/common/data-table'
import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ticketsQueryOptions } from '@/features/tickets/api'
import { ticketColumns } from '@/features/tickets/components/ticket-columns'
import { TicketDetailDialog } from '@/features/tickets/components/ticket-detail-dialog'
import { TicketFilters } from '@/features/tickets/components/ticket-filters'
import { buildTicketQuery, statusOf, type TicketsSearch } from '@/features/tickets/search'
import type { TicketSortField, TicketStatus } from '@/features/tickets/types'

const routeApi = getRouteApi('/_authenticated/_shell/tickets')

export function TicketsPage() {
  const search = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const status = statusOf(search)
  // Date presets are relative to "now"; recompute only when the search changes.
  const query = useMemo(() => buildTicketQuery(search), [search])
  const tickets = useQuery(ticketsQueryOptions(query))
  const [viewId, setViewId] = useState<number | null>(null)

  const updateSearch = useCallback(
    (changes: Partial<TicketsSearch>, resetPage = true) => {
      void navigate({
        search: (prev) => ({ ...prev, ...changes, ...(resetPage ? { page: undefined } : {}) }),
        replace: true,
      })
    },
    [navigate],
  )

  const columns = useMemo(
    () =>
      ticketColumns(status, (ticket) => {
        setViewId(ticket.id)
      }),
    [status],
  )

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tickets"
        description="Tickets ativos e histórico de saídas, com filtros e ordenação."
      />

      <Tabs
        value={status}
        onValueChange={(value) => {
          updateSearch({ status: value as TicketStatus, sortBy: undefined, sortDir: undefined })
        }}
      >
        <TabsList>
          <TabsTrigger value="Open">Ativos</TabsTrigger>
          <TabsTrigger value="Closed">Histórico</TabsTrigger>
        </TabsList>

        {(['Open', 'Closed'] as const).map((tab) => (
          <TabsContent key={tab} value={tab} className="space-y-6 pt-2">
            <TicketFilters search={search} onChange={updateSearch} />

            {tickets.isError && !tickets.data ? (
              <QueryError error={tickets.error} onRetry={() => void tickets.refetch()} />
            ) : (
              <DataTable
                caption={status === 'Open' ? 'Tickets ativos' : 'Histórico de tickets'}
                columns={columns}
                data={tickets.data?.items ?? []}
                isLoading={tickets.isPending}
                emptyMessage={
                  status === 'Open' ? 'Não há tickets ativos.' : 'Nenhum ticket encontrado.'
                }
                getRowId={(ticket) => String(ticket.id)}
                sorting={{ sortBy: query.sortBy, sortDir: query.sortDir }}
                onSortingChange={(sorting) => {
                  updateSearch(
                    { sortBy: sorting.sortBy as TicketSortField, sortDir: sorting.sortDir },
                    true,
                  )
                }}
                pagination={
                  tickets.data
                    ? {
                        page: tickets.data.page,
                        pageSize: tickets.data.pageSize,
                        totalCount: tickets.data.totalCount,
                        totalPages: tickets.data.totalPages,
                      }
                    : { page: query.page, pageSize: query.pageSize, totalCount: 0, totalPages: 0 }
                }
                onPageChange={(page) => {
                  updateSearch({ page }, false)
                }}
                onPageSizeChange={(pageSize) => {
                  updateSearch({ pageSize })
                }}
              />
            )}
          </TabsContent>
        ))}
      </Tabs>

      <TicketDetailDialog
        ticketId={viewId}
        onOpenChange={(open) => {
          if (!open) setViewId(null)
        }}
      />
    </div>
  )
}
