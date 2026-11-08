import { zodResolver } from '@hookform/resolvers/zod'
import { KeyRound } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'

import { FormField } from '@/components/common/form-field'
import { Button } from '@/components/ui/button'
import { FieldGroup } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { applyFieldErrors, toApiError } from '@/lib/api-error'

import { changePassword } from '../api'
import { changePasswordSchema, type ChangePasswordValues } from '../schemas'

export function ChangePasswordForm() {
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })

  const onSubmit = handleSubmit(async ({ currentPassword, newPassword }) => {
    try {
      await changePassword({ currentPassword, newPassword })
      reset()
      toast.success('Senha alterada com sucesso.')
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, ['currentPassword', 'newPassword'])
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('currentPassword', { message: apiError.userMessage })
      }
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate className="max-w-md">
      <FieldGroup>
        <FormField id="currentPassword" label="Senha atual" error={errors.currentPassword}>
          {(control) => (
            <Input
              {...control}
              type="password"
              autoComplete="current-password"
              {...register('currentPassword')}
            />
          )}
        </FormField>
        <FormField
          id="newPassword"
          label="Nova senha"
          error={errors.newPassword}
          description="Pelo menos 8 caracteres, com letras e números."
        >
          {(control) => (
            <Input
              {...control}
              type="password"
              autoComplete="new-password"
              {...register('newPassword')}
            />
          )}
        </FormField>
        <FormField id="confirmPassword" label="Confirmar nova senha" error={errors.confirmPassword}>
          {(control) => (
            <Input
              {...control}
              type="password"
              autoComplete="new-password"
              {...register('confirmPassword')}
            />
          )}
        </FormField>
        <div>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? <Spinner /> : <KeyRound aria-hidden="true" />}
            Alterar senha
          </Button>
        </div>
      </FieldGroup>
    </form>
  )
}
