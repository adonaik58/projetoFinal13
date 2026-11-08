import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { AlertCircle, Save, UserCheck } from 'lucide-react'
import { useEffect } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { FieldGroup, FieldLegend, FieldSet } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
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
      customer: { nationalId: '', fullName: '', birthDate: '' },
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

  // Returning customer: fill in only the fields the operator has not typed yet.
  const known = lookup.data
  useEffect(() => {
    if (!known) return
    const current = getValues()
    if (!current.customer.fullName) setValue('customer.fullName', known.fullName)
    if (!current.customer.birthDate) setValue('customer.birthDate', known.birthDate)
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
                'A procurar cliente…'
              ) : known ? (
                <span className="inline-flex items-center gap-1">
                  <UserCheck className="size-3.5" aria-hidden="true" /> Cliente registado: dados
                  preenchidos automaticamente.
                </span>
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
                    <Select
                      value={field.value > 0 ? String(field.value) : ''}
                      onValueChange={(value) => {
                        field.onChange(Number(value))
                        setValue('vehicle.modelId', null)
                      }}
                      disabled={brands.isPending}
                    >
                      <SelectTrigger {...controlProps} className="w-full" onBlur={field.onBlur}>
                        <SelectValue
                          placeholder={brands.isPending ? 'A carregar…' : 'Escolher a marca'}
                        />
                      </SelectTrigger>
                      <SelectContent>
                        {brands.data?.map((brand) => (
                          <SelectItem key={brand.id} value={String(brand.id)}>
                            {brand.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
                    <Select
                      value={field.value ? String(field.value) : NO_MODEL}
                      onValueChange={(value) => {
                        field.onChange(value === NO_MODEL ? null : Number(value))
                      }}
                      disabled={brandId <= 0 || models.isFetching}
                    >
                      <SelectTrigger {...controlProps} className="w-full" onBlur={field.onBlur}>
                        <SelectValue placeholder="Escolher o modelo" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value={NO_MODEL}>Não indicado</SelectItem>
                        {models.data?.map((model) => (
                          <SelectItem key={model.id} value={String(model.id)}>
                            {model.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
