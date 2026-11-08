import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Save } from 'lucide-react'
import { useForm } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { FieldGroup } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { applyFieldErrors, toApiError } from '@/lib/api-error'
import { formatDateTime } from '@/lib/format'

import { useUpdateSettings } from '../api'
import { settingsSchema, type SettingsValues } from '../schemas'
import type { SettingsDto } from '../types'

export function TariffSettingsForm({ settings }: { settings: SettingsDto }) {
  const update = useUpdateSettings()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<SettingsValues>({
    resolver: zodResolver(settingsSchema),
    values: {
      ratePerMinute: settings.ratePerMinute,
      freeHours: settings.freeHours,
      maxSpaces: settings.maxSpaces,
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      await update.mutateAsync(values)
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, [
        'ratePerMinute',
        'freeHours',
        'maxSpaces',
      ])
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('root', { message: apiError.userMessage })
      }
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate className="max-w-3xl">
      <FieldGroup>
        {errors.root?.message && (
          <Alert variant="destructive">
            <AlertCircle aria-hidden="true" />
            <AlertDescription>{errors.root.message}</AlertDescription>
          </Alert>
        )}
        <div className="grid gap-4 md:grid-cols-3">
          <FormField
            id="ratePerMinute"
            label="Renda por minuto (Kz)"
            error={errors.ratePerMinute}
            description="Aplicada a novas entradas."
          >
            {(c) => (
              <Input
                {...c}
                type="number"
                inputMode="decimal"
                step="0.01"
                min="0.01"
                {...register('ratePerMinute', { valueAsNumber: true })}
              />
            )}
          </FormField>
          <FormField
            id="freeHours"
            label="Horas grátis"
            error={errors.freeHours}
            description="Estadias mais curtas não pagam."
          >
            {(c) => (
              <Input
                {...c}
                type="number"
                inputMode="numeric"
                step="1"
                min="0"
                {...register('freeHours', { valueAsNumber: true })}
              />
            )}
          </FormField>
          <FormField
            id="maxSpaces"
            label="Capacidade máxima"
            error={errors.maxSpaces}
            description="Número máximo de espaços."
          >
            {(c) => (
              <Input
                {...c}
                type="number"
                inputMode="numeric"
                step="1"
                min="1"
                {...register('maxSpaces', { valueAsNumber: true })}
              />
            )}
          </FormField>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Button type="submit" disabled={isSubmitting || !isDirty}>
            {isSubmitting ? <Spinner /> : <Save aria-hidden="true" />}
            Guardar
          </Button>
          {settings.updatedAt && (
            <p className="text-xs text-muted-foreground">
              Última alteração: {formatDateTime(settings.updatedAt)}
              {settings.updatedBy ? ` por ${settings.updatedBy}` : ''}
            </p>
          )}
        </div>
      </FieldGroup>
    </form>
  )
}
