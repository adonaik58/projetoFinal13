import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'
import { RefreshCw } from 'lucide-react'
import { useCallback, useState } from 'react'

import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { spaceBoardQueryOptions } from '@/features/spaces/api'
import { SpaceFilters } from '@/features/spaces/components/space-filters'
import { SpaceGrid } from '@/features/spaces/components/space-grid'
import { SpaceSheet } from '@/features/spaces/components/space-sheet'
import { SpaceSummary } from '@/features/spaces/components/space-summary'
import type { SpaceFilters as Filters } from '@/features/spaces/types'
import { useNow } from '@/lib/clock'
import { cn } from '@/lib/utils'

const routeApi = getRouteApi('/_authenticated/_shell/spaces')

export function SpacesPage() {
  const filters = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const board = useQuery(spaceBoardQueryOptions(filters))
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const clientNow = useNow()

  // Server clock offset so elapsed timers agree with the amounts computed by the API.
  const offset = board.data ? Date.parse(board.data.serverTime) - board.dataUpdatedAt : 0
  const now = clientNow + offset

  const selected = board.data?.items.find((space) => space.id === selectedId) ?? null

  const onFiltersChange = useCallback(
    (next: Filters) => {
      void navigate({ search: next, replace: true })
    },
    [navigate],
  )

  return (
    <div className="space-y-6">
      <PageHeader
        title="Espaços"
        description="Toque num espaço livre para registar uma entrada, ou num ocupado para fechar o ticket."
        actions={
          <Button
            variant="outline"
            onClick={() => void board.refetch()}
            disabled={board.isFetching}
          >
            <RefreshCw className={cn(board.isFetching && 'animate-spin')} aria-hidden="true" />
            Atualizar
          </Button>
        }
      />

      <SpaceFilters value={filters} onChange={onFiltersChange} />

      {board.isError && !board.data ? (
        <QueryError error={board.error} onRetry={() => void board.refetch()} />
      ) : !board.data ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
          {Array.from({ length: 12 }, (_, i) => (
            <Skeleton key={i} className="h-28 rounded-xl" />
          ))}
        </div>
      ) : (
        <>
          <div className="flex flex-wrap items-center justify-between gap-3">
            <SpaceSummary summary={board.data.summary} />
            <p className="text-sm text-muted-foreground" aria-live="polite">
              Resultados: {board.data.items.length}
            </p>
          </div>
          <SpaceGrid
            spaces={board.data.items}
            now={now}
            onSelect={(space) => {
              setSelectedId(space.id)
            }}
          />
        </>
      )}

      <SpaceSheet
        space={selected}
        now={now}
        onOpenChange={(open) => {
          if (!open) setSelectedId(null)
        }}
      />
    </div>
  )
}
