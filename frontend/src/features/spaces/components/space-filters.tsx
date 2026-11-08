import { Search } from 'lucide-react'
import { useEffect, useState } from 'react'

import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useDebouncedValue } from '@/hooks/use-debounced-value'

import { SPACE_STATUS_META, SPACE_STATUSES } from '../status'
import type { SpaceFilters as Filters, SpaceStatus } from '../types'

const ALL = 'all'

export function SpaceFilters({
  value,
  onChange,
}: {
  value: Filters
  onChange: (filters: Filters) => void
}) {
  const [search, setSearch] = useState(value.search ?? '')
  const [plate, setPlate] = useState(value.plate ?? '')
  const debouncedSearch = useDebouncedValue(search)
  const debouncedPlate = useDebouncedValue(plate)

  // Push debounced text filters to the URL.
  useEffect(() => {
    const nextSearch = debouncedSearch.trim() || undefined
    const nextPlate = debouncedPlate.trim() || undefined
    if (nextSearch !== value.search || nextPlate !== value.plate) {
      onChange({ ...value, search: nextSearch, plate: nextPlate })
    }
  }, [debouncedSearch, debouncedPlate, value, onChange])

  return (
    <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4" role="search">
      <div className="grid gap-1.5">
        <Label htmlFor="space-search">Código do espaço</Label>
        <div className="relative">
          <Search
            className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden="true"
          />
          <Input
            id="space-search"
            className="pl-8 uppercase"
            placeholder="ex.: A1"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
            }}
          />
        </div>
      </div>
      <div className="grid gap-1.5">
        <Label htmlFor="space-plate">Matrícula</Label>
        <Input
          id="space-plate"
          className="font-mono uppercase"
          placeholder="ex.: LD-12-34-AB"
          value={plate}
          onChange={(event) => {
            setPlate(event.target.value)
          }}
        />
      </div>
      <div className="grid gap-1.5">
        <Label htmlFor="space-status">Estado</Label>
        <Select
          value={value.status ?? ALL}
          onValueChange={(status) => {
            onChange({ ...value, status: status === ALL ? undefined : (status as SpaceStatus) })
          }}
        >
          <SelectTrigger id="space-status" className="w-full">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL}>Todos</SelectItem>
            {SPACE_STATUSES.map((status) => (
              <SelectItem key={status} value={status}>
                {SPACE_STATUS_META[status].label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="grid gap-1.5">
        <Label htmlFor="space-sort">Ordem</Label>
        <Select
          value={value.sortDir ?? 'asc'}
          onValueChange={(sortDir) => {
            onChange({ ...value, sortDir: sortDir as 'asc' | 'desc' })
          }}
        >
          <SelectTrigger id="space-sort" className="w-full">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="asc">A → Z</SelectItem>
            <SelectItem value="desc">Z → A</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  )
}
