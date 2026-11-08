import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'
import type { ColumnDef } from '@tanstack/react-table'
import { FileJson, Search } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'

import { DataTable } from '@/components/common/data-table'
import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { auditLogsQueryOptions } from '@/features/audit/api'
import type { AuditLogDto } from '@/features/audit/types'
import { useDebouncedValue } from '@/hooks/use-debounced-value'
import { customRange } from '@/lib/date-range'
import { formatDateTime } from '@/lib/format'

const routeApi = getRouteApi('/_authenticated/_shell/management/audit')

function prettyDetails(details: string): string {
  try {
    return JSON.stringify(JSON.parse(details), null, 2)
  } catch {
    return details
  }
}

const columns: ColumnDef<AuditLogDto>[] = [
  {
    id: 'occurredAt',
    header: 'Data',
    cell: ({ row }) => (
      <span className="whitespace-nowrap">{formatDateTime(row.original.occurredAt)}</span>
    ),
  },
  {
    id: 'username',
    header: 'Utilizador',
    cell: ({ row }) => row.original.username ?? <span className="text-muted-foreground">—</span>,
  },
  {
    id: 'action',
    header: 'Ação',
    cell: ({ row }) => <code className="text-xs">{row.original.action}</code>,
  },
  {
    id: 'entity',
    header: 'Entidade',
    cell: ({ row }) =>
      row.original.entityType ? (
        <span className="text-xs">
          {row.original.entityType}
          {row.original.entityId ? ` #${row.original.entityId}` : ''}
        </span>
      ) : (
        <span className="text-muted-foreground">—</span>
      ),
  },
  {
    id: 'ipAddress',
    header: 'IP',
    cell: ({ row }) => <span className="font-mono text-xs">{row.original.ipAddress ?? '—'}</span>,
  },
  {
    id: 'details',
    header: () => <span className="sr-only">Detalhes</span>,
    cell: ({ row }) =>
      row.original.details ? (
        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon-sm" aria-label="Ver detalhes do registo">
              <FileJson aria-hidden="true" />
            </Button>
          </PopoverTrigger>
          <PopoverContent align="end" className="w-96 max-w-[90vw]">
            <pre className="max-h-80 overflow-auto text-xs whitespace-pre-wrap">
              {prettyDetails(row.original.details)}
            </pre>
            {row.original.correlationId && (
              <p className="mt-2 text-xs text-muted-foreground">
                Correlation ID: <code>{row.original.correlationId}</code>
              </p>
            )}
          </PopoverContent>
        </Popover>
      ) : null,
  },
]

export function AuditPage() {
  const search = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const [action, setAction] = useState(search.action ?? '')
  const [username, setUsername] = useState(search.username ?? '')
  const debouncedAction = useDebouncedValue(action)
  const debouncedUsername = useDebouncedValue(username)

  const params = useMemo(() => {
    const range = customRange(search.fromDate, search.toDate)
    return {
      page: search.page ?? 1,
      pageSize: search.pageSize ?? 20,
      action: search.action,
      username: search.username,
      from: range.from,
      to: range.to,
    }
  }, [search])
  const logs = useQuery(auditLogsQueryOptions(params))

  useEffect(() => {
    const nextAction = debouncedAction.trim() || undefined
    const nextUsername = debouncedUsername.trim() || undefined
    if (nextAction !== search.action || nextUsername !== search.username) {
      void navigate({
        search: (prev) => ({
          ...prev,
          action: nextAction,
          username: nextUsername,
          page: undefined,
        }),
        replace: true,
      })
    }
  }, [debouncedAction, debouncedUsername, search.action, search.username, navigate])

  return (
    <div className="space-y-6">
      <PageHeader
        title="Auditoria"
        description="Registo de operações sensíveis: autenticação, utilizadores, configuração e tickets."
      />
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4" role="search">
        <div className="grid gap-1.5">
          <Label htmlFor="audit-action">Ação</Label>
          <div className="relative">
            <Search
              className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <Input
              id="audit-action"
              className="pl-8"
              placeholder="ex.: auth.login"
              value={action}
              onChange={(event) => {
                setAction(event.target.value)
              }}
            />
          </div>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="audit-username">Utilizador</Label>
          <Input
            id="audit-username"
            value={username}
            onChange={(event) => {
              setUsername(event.target.value)
            }}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="audit-from">Desde</Label>
          <Input
            id="audit-from"
            type="date"
            value={search.fromDate ?? ''}
            onChange={(event) => {
              void navigate({
                search: (prev) => ({
                  ...prev,
                  fromDate: event.target.value || undefined,
                  page: undefined,
                }),
                replace: true,
              })
            }}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="audit-to">Até</Label>
          <Input
            id="audit-to"
            type="date"
            min={search.fromDate}
            value={search.toDate ?? ''}
            onChange={(event) => {
              void navigate({
                search: (prev) => ({
                  ...prev,
                  toDate: event.target.value || undefined,
                  page: undefined,
                }),
                replace: true,
              })
            }}
          />
        </div>
      </div>
      {logs.isError && !logs.data ? (
        <QueryError error={logs.error} onRetry={() => void logs.refetch()} />
      ) : (
        <DataTable
          caption="Registos de auditoria"
          columns={columns}
          data={logs.data?.items ?? []}
          isLoading={logs.isPending}
          emptyMessage="Sem registos para os filtros escolhidos."
          getRowId={(log) => String(log.id)}
          pagination={
            logs.data ?? {
              page: params.page,
              pageSize: params.pageSize,
              totalCount: 0,
              totalPages: 0,
            }
          }
          onPageChange={(page) => {
            void navigate({ search: (prev) => ({ ...prev, page }), replace: true })
          }}
          onPageSizeChange={(pageSize) => {
            void navigate({
              search: (prev) => ({ ...prev, pageSize, page: undefined }),
              replace: true,
            })
          }}
        />
      )}
    </div>
  )
}
