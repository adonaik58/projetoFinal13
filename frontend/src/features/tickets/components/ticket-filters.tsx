import { useQuery } from '@tanstack/react-query'
import { Funnel, Search, X } from 'lucide-react'
import { useEffect, useId, useState, type SyntheticEvent } from 'react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { brandsQueryOptions, modelsQueryOptions } from '@/features/catalog/api'
import { useDebouncedValue } from '@/hooks/use-debounced-value'
import { DATE_PRESET_LABELS, DATE_PRESETS, type DatePreset } from '@/lib/date-range'

import { activeFilterCount, type TicketsSearch } from '../search'

type AdvancedFilters = Pick<
  TicketsSearch,
  | 'customerName'
  | 'nationalId'
  | 'plate'
  | 'spaceCode'
  | 'brandId'
  | 'modelId'
  | 'preset'
  | 'fromDate'
  | 'toDate'
>

const EMPTY_FILTERS: AdvancedFilters = {
  customerName: undefined,
  nationalId: undefined,
  plate: undefined,
  spaceCode: undefined,
  brandId: undefined,
  modelId: undefined,
  preset: undefined,
  fromDate: undefined,
  toDate: undefined,
}

const ANY = 'any'

function pickAdvanced(search: TicketsSearch): AdvancedFilters {
  return {
    customerName: search.customerName,
    nationalId: search.nationalId,
    plate: search.plate,
    spaceCode: search.spaceCode,
    brandId: search.brandId,
    modelId: search.modelId,
    preset: search.preset,
    fromDate: search.fromDate,
    toDate: search.toDate,
  }
}

const blankToUndefined = (value: string) => (value.trim() === '' ? undefined : value.trim())

export function TicketFilters({
  search,
  onChange,
}: {
  search: TicketsSearch
  /** Receives the changed keys; the page resets pagination. */
  onChange: (changes: Partial<TicketsSearch>) => void
}) {
  const panelId = useId()
  const [open, setOpen] = useState(() => activeFilterCount(search) > 0)
  const [quickSearch, setQuickSearch] = useState(search.search ?? '')
  const debouncedQuickSearch = useDebouncedValue(quickSearch)
  const [draft, setDraft] = useState<AdvancedFilters>(() => pickAdvanced(search))

  const brands = useQuery({ ...brandsQueryOptions(), enabled: open })
  const models = useQuery(modelsQueryOptions(open ? draft.brandId : undefined))

  useEffect(() => {
    const next = blankToUndefined(debouncedQuickSearch)
    if (next !== search.search) onChange({ search: next })
  }, [debouncedQuickSearch, search.search, onChange])

  const update = (changes: Partial<AdvancedFilters>) => {
    setDraft((current) => ({ ...current, ...changes }))
  }

  function apply(event: SyntheticEvent<HTMLFormElement>) {
    event.preventDefault()
    onChange({
      ...draft,
      customerName: blankToUndefined(draft.customerName ?? ''),
      nationalId: blankToUndefined(draft.nationalId ?? '')?.toUpperCase(),
      plate: blankToUndefined(draft.plate ?? '')?.toUpperCase(),
      spaceCode: blankToUndefined(draft.spaceCode ?? '')?.toUpperCase(),
    })
  }

  function clear() {
    setDraft(EMPTY_FILTERS)
    onChange(EMPTY_FILTERS)
  }

  const count = activeFilterCount(search)

  return (
    <div className="space-y-3">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center" role="search">
        <div className="relative flex-1">
          <Label htmlFor="ticket-quick-search" className="sr-only">
            Pesquisar tickets
          </Label>
          <Search
            className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden="true"
          />
          <Input
            id="ticket-quick-search"
            className="pl-8"
            placeholder="Pesquisar por cliente, BI, matrícula ou código…"
            value={quickSearch}
            onChange={(event) => {
              setQuickSearch(event.target.value)
            }}
          />
        </div>
        <Button
          variant="outline"
          aria-expanded={open}
          aria-controls={panelId}
          onClick={() => {
            setOpen((value) => !value)
          }}
        >
          <Funnel aria-hidden="true" />
          Filtros{count > 0 ? ` (${String(count)})` : ''}
        </Button>
      </div>

      {open && (
        <form
          id={panelId}
          onSubmit={apply}
          aria-label="Filtros avançados"
          className="grid gap-4 rounded-xl border bg-muted/30 p-4"
        >
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div className="grid gap-1.5">
              <Label htmlFor="f-customerName">Nome do cliente</Label>
              <Input
                id="f-customerName"
                value={draft.customerName ?? ''}
                onChange={(event) => {
                  update({ customerName: event.target.value })
                }}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-nationalId">Bilhete de Identidade</Label>
              <Input
                id="f-nationalId"
                className="font-mono uppercase"
                maxLength={14}
                value={draft.nationalId ?? ''}
                onChange={(event) => {
                  update({ nationalId: event.target.value })
                }}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-plate">Matrícula</Label>
              <Input
                id="f-plate"
                className="font-mono uppercase"
                value={draft.plate ?? ''}
                onChange={(event) => {
                  update({ plate: event.target.value })
                }}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-spaceCode">Código do espaço</Label>
              <Input
                id="f-spaceCode"
                className="uppercase"
                value={draft.spaceCode ?? ''}
                onChange={(event) => {
                  update({ spaceCode: event.target.value })
                }}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-brand">Marca</Label>
              <Select
                value={draft.brandId ? String(draft.brandId) : ANY}
                onValueChange={(value) => {
                  update({ brandId: value === ANY ? undefined : Number(value), modelId: undefined })
                }}
              >
                <SelectTrigger id="f-brand" className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={ANY}>Todas</SelectItem>
                  {brands.data?.map((brand) => (
                    <SelectItem key={brand.id} value={String(brand.id)}>
                      {brand.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-model">Modelo</Label>
              <Select
                value={draft.modelId ? String(draft.modelId) : ANY}
                disabled={!draft.brandId}
                onValueChange={(value) => {
                  update({ modelId: value === ANY ? undefined : Number(value) })
                }}
              >
                <SelectTrigger id="f-model" className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={ANY}>Todos</SelectItem>
                  {models.data?.map((model) => (
                    <SelectItem key={model.id} value={String(model.id)}>
                      {model.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-fromDate">Entrada desde</Label>
              <Input
                id="f-fromDate"
                type="date"
                value={draft.fromDate ?? ''}
                onChange={(event) => {
                  update({ fromDate: event.target.value || undefined, preset: undefined })
                }}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="f-toDate">Entrada até</Label>
              <Input
                id="f-toDate"
                type="date"
                min={draft.fromDate}
                value={draft.toDate ?? ''}
                onChange={(event) => {
                  update({ toDate: event.target.value || undefined, preset: undefined })
                }}
              />
            </div>
          </div>

          <fieldset className="grid gap-1.5">
            <legend className="mb-1.5 text-sm font-medium">Período de entrada</legend>
            <ToggleGroup
              type="single"
              variant="outline"
              className="flex-wrap"
              value={draft.preset ?? ''}
              onValueChange={(value) => {
                update({
                  preset: value === '' ? undefined : (value as DatePreset),
                  fromDate: undefined,
                  toDate: undefined,
                })
              }}
            >
              {DATE_PRESETS.map((preset) => (
                <ToggleGroupItem key={preset} value={preset}>
                  {DATE_PRESET_LABELS[preset]}
                </ToggleGroupItem>
              ))}
            </ToggleGroup>
          </fieldset>

          <div className="flex flex-wrap justify-end gap-2">
            <Button type="button" variant="ghost" onClick={clear}>
              <X aria-hidden="true" /> Limpar
            </Button>
            <Button type="submit">
              <Search aria-hidden="true" /> Aplicar filtros
            </Button>
          </div>
        </form>
      )}
    </div>
  )
}
