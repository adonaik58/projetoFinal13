import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react'
import { useId } from 'react'

import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { formatNumber } from '@/lib/format'
import { PAGE_SIZES } from '@/lib/paged'

export interface PaginationState {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function Pagination({
  state,
  onPageChange,
  onPageSizeChange,
}: {
  state: PaginationState
  onPageChange: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
}) {
  const pageSizeId = useId()
  const { page, pageSize, totalCount } = state
  const totalPages = Math.max(1, state.totalPages)

  return (
    <nav
      aria-label="Paginação"
      className="flex flex-col items-center justify-between gap-3 text-sm sm:flex-row"
    >
      <p className="text-muted-foreground" aria-live="polite">
        {formatNumber(totalCount)} {totalCount === 1 ? 'registo' : 'registos'} · Página {page} de{' '}
        {totalPages}
      </p>
      <div className="flex items-center gap-2">
        {onPageSizeChange && (
          <div className="flex items-center gap-2">
            <label htmlFor={pageSizeId} className="text-muted-foreground">
              Por página
            </label>
            <Select
              value={String(pageSize)}
              onValueChange={(value) => {
                onPageSizeChange(Number(value))
              }}
            >
              <SelectTrigger id={pageSizeId} size="sm" className="w-20">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {PAGE_SIZES.map((size) => (
                  <SelectItem key={size} value={String(size)}>
                    {size}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        )}
        <Button
          variant="outline"
          size="icon-sm"
          aria-label="Primeira página"
          disabled={page <= 1}
          onClick={() => {
            onPageChange(1)
          }}
        >
          <ChevronsLeft aria-hidden="true" />
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label="Página anterior"
          disabled={page <= 1}
          onClick={() => {
            onPageChange(page - 1)
          }}
        >
          <ChevronLeft aria-hidden="true" />
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label="Página seguinte"
          disabled={page >= totalPages}
          onClick={() => {
            onPageChange(page + 1)
          }}
        >
          <ChevronRight aria-hidden="true" />
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label="Última página"
          disabled={page >= totalPages}
          onClick={() => {
            onPageChange(totalPages)
          }}
        >
          <ChevronsRight aria-hidden="true" />
        </Button>
      </div>
    </nav>
  )
}
