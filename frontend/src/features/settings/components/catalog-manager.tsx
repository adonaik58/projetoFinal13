import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'

import { QueryError } from '@/components/common/query-error'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldError } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Skeleton } from '@/components/ui/skeleton'
import {
  brandFormSchema,
  brandsQueryOptions,
  modelsQueryOptions,
  useCreateBrand,
  useCreateModel,
  type BrandFormValues,
} from '@/features/catalog/api'
import { toApiError } from '@/lib/api-error'
import { cn } from '@/lib/utils'

function AddNameForm({
  id,
  label,
  submitLabel,
  onCreate,
}: {
  id: string
  label: string
  submitLabel: string
  onCreate: (values: BrandFormValues) => Promise<unknown>
}) {
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<BrandFormValues>({
    resolver: zodResolver(brandFormSchema),
    defaultValues: { name: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      await onCreate(values)
      reset()
    } catch (error) {
      setError('name', { message: toApiError(error).userMessage })
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <Field data-invalid={!!errors.name}>
        <Label htmlFor={id}>{label}</Label>
        <div className="flex gap-2">
          <Input
            id={id}
            autoComplete="off"
            aria-invalid={!!errors.name}
            aria-describedby={errors.name ? `${id}-error` : undefined}
            {...register('name')}
          />
          <Button type="submit" disabled={isSubmitting}>
            <Plus aria-hidden="true" /> {submitLabel}
          </Button>
        </div>
        <FieldError id={`${id}-error`} errors={[errors.name]} />
      </Field>
    </form>
  )
}

function ModelsPanel({ brandId, brandName }: { brandId: number; brandName: string }) {
  const models = useQuery(modelsQueryOptions(brandId))
  const createModel = useCreateModel(brandId)

  return (
    <div className="space-y-4">
      <AddNameForm
        key={brandId}
        id="new-model"
        label={`Novo modelo de ${brandName}`}
        submitLabel="Adicionar"
        onCreate={(values) => createModel.mutateAsync(values)}
      />
      {models.isError ? (
        <QueryError error={models.error} onRetry={() => void models.refetch()} />
      ) : !models.data ? (
        <Skeleton className="h-40 w-full" />
      ) : models.data.length === 0 ? (
        <p className="text-sm text-muted-foreground">Ainda não há modelos para esta marca.</p>
      ) : (
        <ScrollArea className="h-72 rounded-lg border">
          <ul className="divide-y">
            {models.data.map((model) => (
              <li key={model.id} className="px-3 py-2 text-sm">
                {model.name}
              </li>
            ))}
          </ul>
        </ScrollArea>
      )}
    </div>
  )
}

export function CatalogManager() {
  const brands = useQuery(brandsQueryOptions())
  const createBrand = useCreateBrand()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const selected = brands.data?.find((brand) => brand.id === selectedId) ?? null

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Marcas</CardTitle>
          <CardDescription>Selecione uma marca para gerir os modelos.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <AddNameForm
            id="new-brand"
            label="Nova marca"
            submitLabel="Adicionar"
            onCreate={(values) => createBrand.mutateAsync(values)}
          />
          {brands.isError ? (
            <QueryError error={brands.error} onRetry={() => void brands.refetch()} />
          ) : !brands.data ? (
            <Skeleton className="h-72 w-full" />
          ) : (
            <ScrollArea className="h-72 rounded-lg border">
              <ul className="divide-y" aria-label="Marcas">
                {brands.data.map((brand) => (
                  <li key={brand.id}>
                    <button
                      type="button"
                      aria-pressed={brand.id === selectedId}
                      onClick={() => {
                        setSelectedId(brand.id)
                      }}
                      className={cn(
                        'w-full px-3 py-2 text-left text-sm hover:bg-muted focus-visible:bg-muted focus-visible:outline-none',
                        brand.id === selectedId && 'bg-primary/10 font-medium',
                      )}
                    >
                      {brand.name}
                    </button>
                  </li>
                ))}
              </ul>
            </ScrollArea>
          )}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Modelos</CardTitle>
          <CardDescription>
            {selected ? `Modelos da marca ${selected.name}` : 'Nenhuma marca selecionada.'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {selected ? (
            <ModelsPanel brandId={selected.id} brandName={selected.name} />
          ) : (
            <p className="text-sm text-muted-foreground">
              Escolha uma marca na lista para ver e adicionar modelos.
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
