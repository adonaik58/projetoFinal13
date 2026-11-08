import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { Plus, Rows3, Trash, Wrench, CircleCheck } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'

import { ConfirmDialog } from '@/components/common/confirm-dialog'
import { FormField } from '@/components/common/form-field'
import { QueryError } from '@/components/common/query-error'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { FieldGroup } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
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
import {
  spaceBoardQueryOptions,
  useBulkCreateSpaces,
  useCreateSpace,
  useDeleteSpace,
  useUpdateSpaceStatus,
} from '@/features/spaces/api'
import {
  bulkSpacesSchema,
  createSpaceSchema,
  parseSections,
  type BulkSpacesValues,
  type CreateSpaceValues,
} from '@/features/spaces/schemas'
import { SPACE_STATUS_META } from '@/features/spaces/status'
import type { SpaceDto } from '@/features/spaces/types'
import { toApiError } from '@/lib/api-error'
import { formatDateTime } from '@/lib/format'

function CreateSpaceForm() {
  const createSpace = useCreateSpace()
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateSpaceValues>({
    resolver: zodResolver(createSpaceSchema),
    defaultValues: { code: '' },
  })

  const onSubmit = handleSubmit(async ({ code }) => {
    try {
      await createSpace.mutateAsync(code)
      reset()
    } catch (error) {
      setError('code', { message: toApiError(error).userMessage })
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <FieldGroup>
        <FormField id="space-code" label="Código" error={errors.code} description="ex.: A1, B12">
          {(c) => <Input {...c} className="uppercase" autoComplete="off" {...register('code')} />}
        </FormField>
        <div>
          <Button type="submit" disabled={isSubmitting}>
            <Plus aria-hidden="true" /> Criar espaço
          </Button>
        </div>
      </FieldGroup>
    </form>
  )
}

function BulkCreateForm() {
  const bulkCreate = useBulkCreateSpaces()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<BulkSpacesValues>({
    resolver: zodResolver(bulkSpacesSchema),
    defaultValues: { sections: '', fromNumber: 1, toNumber: 10 },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      const result = await bulkCreate.mutateAsync({
        sections: parseSections(values.sections) ?? [],
        fromNumber: values.fromNumber,
        toNumber: values.toNumber,
      })
      toast.success(`${String(result.created)} espaço(s) criado(s).`, {
        description:
          result.skipped > 0
            ? `${String(result.skipped)} já existiam e foram ignorados.`
            : undefined,
      })
    } catch (error) {
      setError('root', { message: toApiError(error).userMessage })
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <FieldGroup>
        {errors.root?.message && (
          <p role="alert" className="text-sm text-destructive">
            {errors.root.message}
          </p>
        )}
        <FormField
          id="bulk-sections"
          label="Secções"
          error={errors.sections}
          description="Letras separadas por vírgulas ou intervalos, ex.: A-C, AA"
        >
          {(c) => (
            <Input {...c} className="uppercase" autoComplete="off" {...register('sections')} />
          )}
        </FormField>
        <div className="grid grid-cols-2 gap-4">
          <FormField id="bulk-from" label="Do número" error={errors.fromNumber}>
            {(c) => (
              <Input
                {...c}
                type="number"
                min="1"
                {...register('fromNumber', { valueAsNumber: true })}
              />
            )}
          </FormField>
          <FormField id="bulk-to" label="Ao número" error={errors.toNumber}>
            {(c) => (
              <Input
                {...c}
                type="number"
                min="1"
                {...register('toNumber', { valueAsNumber: true })}
              />
            )}
          </FormField>
        </div>
        <div>
          <Button type="submit" disabled={isSubmitting}>
            <Rows3 aria-hidden="true" /> Criar em lote
          </Button>
        </div>
      </FieldGroup>
    </form>
  )
}

export function SpacesManager() {
  const board = useQuery({ ...spaceBoardQueryOptions({ sortDir: 'asc' }), refetchInterval: false })
  const updateStatus = useUpdateSpaceStatus()
  const deleteSpace = useDeleteSpace()
  const [toDelete, setToDelete] = useState<SpaceDto | null>(null)

  return (
    <div className="grid gap-6 lg:grid-cols-[minmax(0,22rem)_1fr]">
      <div className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Novo espaço</CardTitle>
            <CardDescription>Cria um único espaço.</CardDescription>
          </CardHeader>
          <CardContent>
            <CreateSpaceForm />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Criar em lote</CardTitle>
            <CardDescription>
              Gera os códigos para cada secção e número (ex.: A1…A10). Respeita a capacidade máxima.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <BulkCreateForm />
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Espaços existentes</CardTitle>
          <CardDescription>
            {board.data
              ? `${String(board.data.summary.total)} espaços · ${String(board.data.summary.maintenance)} indisponíveis`
              : 'A carregar…'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {board.isError ? (
            <QueryError error={board.error} onRetry={() => void board.refetch()} />
          ) : !board.data ? (
            <div className="space-y-2">
              {Array.from({ length: 6 }, (_, i) => (
                <Skeleton key={i} className="h-8 w-full" />
              ))}
            </div>
          ) : (
            <div className="max-h-[32rem] overflow-auto rounded-lg border">
              <Table>
                <TableCaption className="sr-only">Lista de espaços</TableCaption>
                <TableHeader>
                  <TableRow>
                    <TableHead>Código</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead>Adicionado por</TableHead>
                    <TableHead className="text-right">Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {board.data.items.map((space) => {
                    const meta = SPACE_STATUS_META[space.status]
                    return (
                      <TableRow key={space.id}>
                        <TableCell className="font-semibold">{space.code}</TableCell>
                        <TableCell>
                          <Badge className={meta.badgeClass}>
                            <meta.icon aria-hidden="true" /> {meta.label}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <span className="block">{space.createdBy ?? '—'}</span>
                          <span className="block text-xs text-muted-foreground">
                            {formatDateTime(space.createdAt)}
                          </span>
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="inline-flex gap-1">
                            {space.status === 'Free' && (
                              <Button
                                size="sm"
                                variant="outline"
                                disabled={updateStatus.isPending}
                                onClick={() => {
                                  updateStatus.mutate({ id: space.id, status: 'Maintenance' })
                                }}
                              >
                                <Wrench aria-hidden="true" /> Indisponível
                              </Button>
                            )}
                            {space.status === 'Maintenance' && (
                              <Button
                                size="sm"
                                variant="outline"
                                disabled={updateStatus.isPending}
                                onClick={() => {
                                  updateStatus.mutate({ id: space.id, status: 'Free' })
                                }}
                              >
                                <CircleCheck aria-hidden="true" /> Disponibilizar
                              </Button>
                            )}
                            <Button
                              size="icon-sm"
                              variant="ghost"
                              aria-label={`Eliminar espaço ${space.code}`}
                              disabled={space.status === 'Occupied'}
                              onClick={() => {
                                setToDelete(space)
                              }}
                            >
                              <Trash aria-hidden="true" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    )
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title={`Eliminar o espaço ${toDelete?.code ?? ''}?`}
        description="Só é possível eliminar espaços sem histórico de tickets. Para retirar um espaço com histórico, marque-o como indisponível."
        confirmLabel="Eliminar"
        destructive
        pending={deleteSpace.isPending}
        onConfirm={() => {
          if (!toDelete) return
          deleteSpace.mutate(toDelete.id, {
            onSettled: () => {
              setToDelete(null)
            },
          })
        }}
      />
    </div>
  )
}
