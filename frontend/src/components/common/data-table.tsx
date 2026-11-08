import {
  type ColumnDef,
  flexRender,
  getCoreRowModel,
  type SortingState,
  useReactTable,
} from '@tanstack/react-table'
import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

import { Pagination, type PaginationState } from './pagination'

export interface ServerSorting {
  sortBy: string
  sortDir: 'asc' | 'desc'
}

/**
 * Table for server-paginated, server-sorted lists (manualPagination / manualSorting).
 * Columns with `enableSorting: true` and an `id` matching an API `sortBy` value become sortable.
 */
export function DataTable<TData>({
  columns,
  data,
  caption,
  isLoading,
  emptyMessage = 'Sem resultados.',
  sorting,
  onSortingChange,
  pagination,
  onPageChange,
  onPageSizeChange,
  getRowId,
}: {
  columns: ColumnDef<TData>[]
  data: TData[]
  caption: string
  isLoading?: boolean
  emptyMessage?: string
  sorting?: ServerSorting
  onSortingChange?: (sorting: ServerSorting) => void
  pagination?: PaginationState
  onPageChange?: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
  getRowId?: (row: TData) => string
}) {
  const sortingState: SortingState = sorting
    ? [{ id: sorting.sortBy, desc: sorting.sortDir === 'desc' }]
    : []

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    manualSorting: true,
    enableSortingRemoval: false,
    enableSorting: !!onSortingChange,
    state: { sorting: sortingState },
    onSortingChange: (updater) => {
      const next = typeof updater === 'function' ? updater(sortingState) : updater
      const first = next[0]
      if (first && onSortingChange) {
        onSortingChange({ sortBy: first.id, sortDir: first.desc ? 'desc' : 'asc' })
      }
    },
    ...(getRowId ? { getRowId: (row: TData) => getRowId(row) } : {}),
  })

  return (
    <div className="space-y-3">
      <div className="overflow-hidden rounded-xl border">
        <Table>
          <TableCaption className="sr-only">{caption}</TableCaption>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => {
                  const canSort = header.column.getCanSort()
                  const direction = header.column.getIsSorted()
                  const label = flexRender(header.column.columnDef.header, header.getContext())
                  return (
                    <TableHead
                      key={header.id}
                      aria-sort={
                        direction === 'asc'
                          ? 'ascending'
                          : direction === 'desc'
                            ? 'descending'
                            : canSort
                              ? 'none'
                              : undefined
                      }
                    >
                      {header.isPlaceholder ? null : canSort ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          className="-ml-2"
                          onClick={header.column.getToggleSortingHandler()}
                        >
                          {label}
                          {direction === 'asc' ? (
                            <ArrowUp aria-hidden="true" />
                          ) : direction === 'desc' ? (
                            <ArrowDown aria-hidden="true" />
                          ) : (
                            <ArrowUpDown aria-hidden="true" className="opacity-50" />
                          )}
                        </Button>
                      ) : (
                        label
                      )}
                    </TableHead>
                  )
                })}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }, (_, i) => (
                <TableRow key={`skeleton-${i}`}>
                  {columns.map((_column, j) => (
                    <TableCell key={j}>
                      <Skeleton className="h-4 w-full" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : table.getRowModel().rows.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center text-muted-foreground"
                >
                  {emptyMessage}
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
      {pagination && onPageChange && (
        <Pagination
          state={pagination}
          onPageChange={onPageChange}
          {...(onPageSizeChange ? { onPageSizeChange } : {})}
        />
      )}
    </div>
  )
}
