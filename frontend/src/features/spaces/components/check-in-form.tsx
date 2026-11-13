import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { AlertCircle, IdCard, Save, UserCheck } from 'lucide-react'
import { useEffect } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/ui/combobox'
import { FieldGroup, FieldLegend, FieldSet } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { brandsQueryOptions, modelsQueryOptions } from '@/features/catalog/api'
import { customerLookupQueryOptions } from '@/features/customers/api'
import { useCreateTicket } from '@/features/tickets/api'
import {
  CHECK_IN_FIELDS,
  checkInSchema,
  isValidNationalId,
  type CheckInInput,
  type CheckInValues,
} from '@/features/tickets/schemas'
import type { TicketDetailDto } from '@/features/tickets/types'
import { applyFieldErrors, toApiError } from '@/lib/api-error'
import { toDateOnly } from '@/lib/format'

const NO_MODEL = 'none'
const DEFAULT_COLOR = '#1f2937'

export function CheckInForm({
  spaceId,
  onSuccess,
}: {
  spaceId: number
  onSuccess: (ticket: TicketDetailDto) => void
}) {
  const createTicket = useCreateTicket()
  const {
    register,
    control,
    handleSubmit,
    setError,
    setValue,
    getValues,
    formState: { errors, isSubmitting },
  } = useForm<CheckInInput, unknown, CheckInValues>({
    resolver: zodResolver(checkInSchema),
    defaultValues: {
      spaceId,
      customer: { nationalId: '', fullName: '', birthDate: '', phone: '' },
      vehicle: { plate: '', brandId: 0, modelId: null, color: DEFAULT_COLOR },
    },
  })

  const nationalId = useWatch({ control, name: 'customer.nationalId' })
  const brandId = useWatch({ control, name: 'vehicle.brandId' })
  const color = useWatch({ control, name: 'vehicle.color' })

  const normalizedNationalId = nationalId.trim().toUpperCase()
  const lookup = useQuery(
    customerLookupQueryOptions(normalizedNationalId, isValidNationalId(normalizedNationalId)),
  )
  const brands = useQuery(brandsQueryOptions())
  const models = useQuery(modelsQueryOptions(brandId > 0 ? brandId : null))

  // Returning customer or BI registry data: fill in only the fields the operator has not typed yet.
  const known = lookup.data
  useEffect(() => {
    if (!known) return
    const current = getValues()
    const fill = { shouldValidate: true } as const
    if (!current.customer.fullName) setValue('customer.fullName', known.fullName, fill)
    if (!current.customer.birthDate && known.birthDate) {
      setValue('customer.birthDate', known.birthDate, fill)
    }
    if (!current.customer.phone && known.phone) setValue('customer.phone', known.phone, fill)
    const vehicle = known.lastVehicle
    if (vehicle && !current.vehicle.plate) {
      setValue('vehicle.plate', vehicle.plate)
      setValue('vehicle.brandId', vehicle.brandId)
      setValue('vehicle.modelId', vehicle.modelId)
      setValue('vehicle.color', vehicle.color)
    }
  }, [known, getValues, setValue])

  const onSubmit = handleSubmit(async (values) => {
    try {
      const ticket = await createTicket.mutateAsync(values)
      onSuccess(ticket)
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, CHECK_IN_FIELDS)
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('root', { message: apiError.userMessage })
      }
    }
  })

  const today = toDateOnly(new Date())

  return (
    <form
      onSubmit={(event) => void onSubmit(event)}
      noValidate
      aria-label="Ocupar espaço"
      className="flex flex-col gap-6"
    >
      {errors.root?.message && (
        <Alert variant="destructive">
          <AlertCircle aria-hidden="true" />
          <AlertDescription>{errors.root.message}</AlertDescription>
        </Alert>
      )}

      <FieldSet>
        <FieldLegend>Cliente</FieldLegend>
        <FieldGroup>
          <FormField
            id="customer-nationalId"
            label="Bilhete de Identidade"
            error={errors.customer?.nationalId}
            description={
              lookup.isFetching ? (
                <span className="inline-flex items-center gap-1">
                  <Spinner className="size-3.5" /> A consultar o BI…
                </span>
              ) : known?.source === 'Customer' ? (
                <span className="inline-flex items-center gap-1">
                  <UserCheck className="size-3.5" aria-hidden="true" /> Cliente registado: dados
                  preenchidos automaticamente.
                </span>
              ) : known?.source === 'Registry' ? (
                <span className="inline-flex items-center gap-1">
                  <IdCard className="size-3.5" aria-hidden="true" /> Dados obtidos da consulta do
                  BI: confirme antes de registar.
                </span>
              ) : lookup.isSuccess && isValidNationalId(normalizedNationalId) ? (
                'BI sem dados disponíveis: preencha manualmente.'
              ) : (
                '14 caracteres, ex.: 123456789LA123'
              )
            }
          >
            {(controlProps) => (
              <Input
                {...controlProps}
                autoComplete="off"
                maxLength={14}
                className="font-mono uppercase"
                {...register('customer.nationalId')}
              />
            )}
          </FormField>
          <FormField id="customer-fullName" label="Nome completo" error={errors.customer?.fullName}>
            {(controlProps) => (
              <Input {...controlProps} autoComplete="off" {...register('customer.fullName')} />
            )}
          </FormField>
          <FormField
            id="customer-birthDate"
            label="Data de nascimento"
            error={errors.customer?.birthDate}
          >
            {(controlProps) => (
              <Input
                {...controlProps}
                type="date"
                max={today}
                {...register('customer.birthDate')}
              />
            )}
          </FormField>
          <FormField
            id="customer-phone"
            label="Contacto (opcional)"
            error={errors.customer?.phone}
            description="Telefone do ocupante, ex.: 923 456 789"
          >
            {(controlProps) => (
              <Input
                {...controlProps}
                type="tel"
                inputMode="tel"
                autoComplete="off"
                maxLength={20}
                {...register('customer.phone')}
              />
            )}
          </FormField>
        </FieldGroup>
      </FieldSet>

      <FieldSet>
        <FieldLegend>Viatura</FieldLegend>
        <FieldGroup>
          <FormField id="vehicle-plate" label="Matrícula" error={errors.vehicle?.plate}>
            {(controlProps) => (
              <Input
                {...controlProps}
                autoComplete="off"
                maxLength={20}
                className="font-mono uppercase"
                {...register('vehicle.plate')}
              />
            )}
          </FormField>
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField id="vehicle-brandId" label="Marca" error={errors.vehicle?.brandId}>
              {(controlProps) => (
                <Controller
                  control={control}
                  name="vehicle.brandId"
                  render={({ field }) => (
                    <Combobox
                      {...controlProps}
                      value={field.value > 0 ? String(field.value) : ''}
                      onValueChange={(value) => {
                        field.onChange(value ? Number(value) : 0)
                        setValue('vehicle.modelId', null)
                      }}
                      options={
                        brands.data?.map((brand) => ({
                          value: String(brand.id),
                          label: brand.name,
                        })) ?? []
                      }
                      placeholder={brands.isPending ? 'A carregar…' : 'Pesquisar ou escolher marca'}
                      searchPlaceholder="Pesquisar marca (ex: Toyota, BMW…)"
                      emptyText="Nenhuma marca encontrada."
                      disabled={brands.isPending}
                      onBlur={field.onBlur}
                    />
                  )}
                />
              )}
            </FormField>
            <FormField id="vehicle-modelId" label="Modelo" error={errors.vehicle?.modelId}>
              {(controlProps) => (
                <Controller
                  control={control}
                  name="vehicle.modelId"
                  render={({ field }) => (
                    <Combobox
                      {...controlProps}
                      value={field.value ? String(field.value) : NO_MODEL}
                      onValueChange={(value) => {
                        field.onChange(value === NO_MODEL ? null : Number(value))
                      }}
                      options={[
                        { value: NO_MODEL, label: 'Não indicado' },
                        ...(models.data?.map((model) => ({
                          value: String(model.id),
                          label: model.name,
                        })) ?? []),
                      ]}
                      placeholder={
                        brandId <= 0 ? 'Escolha primeiro a marca' : 'Pesquisar ou escolher modelo'
                      }
                      searchPlaceholder="Pesquisar modelo (ex: Hilux, Corolla…)"
                      emptyText="Nenhum modelo encontrado."
                      disabled={brandId <= 0 || models.isFetching}
                      onBlur={field.onBlur}
                    />
                  )}
                />
              )}
            </FormField>
          </div>
          <FormField id="vehicle-color" label="Cor" error={errors.vehicle?.color}>
            {(controlProps) => (
              <div className="flex items-center gap-3">
                <Input
                  {...controlProps}
                  type="color"
                  className="h-9 w-16 cursor-pointer p-1"
                  {...register('vehicle.color')}
                />
                <span className="font-mono text-sm text-muted-foreground uppercase">{color}</span>
              </div>
            )}
          </FormField>
        </FieldGroup>
      </FieldSet>

      <Button type="submit" size="lg" disabled={isSubmitting}>
        {isSubmitting ? <Spinner /> : <Save aria-hidden="true" />}
        Registar entrada
      </Button>
    </form>
  )
}
