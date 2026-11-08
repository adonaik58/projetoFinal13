import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { FieldGroup } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { applyFieldErrors, toApiError } from '@/lib/api-error'

import { useResetPassword } from '../api'
import { resetPasswordSchema, type ResetPasswordValues } from '../schemas'
import type { UserDto } from '../types'

function ResetPasswordForm({ user, onDone }: { user: UserDto; onDone: () => void }) {
  const resetPassword = useResetPassword(user.id)
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  })

  const onSubmit = handleSubmit(async ({ newPassword }) => {
    try {
      await resetPassword.mutateAsync(newPassword)
      onDone()
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, ['newPassword'])
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('newPassword', { message: apiError.userMessage })
      }
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <FieldGroup>
        <FormField
          id="reset-newPassword"
          label="Nova senha"
          error={errors.newPassword}
          description="Pelo menos 8 caracteres, com letras e números."
        >
          {(c) => (
            <Input
              {...c}
              type="password"
              autoComplete="new-password"
              {...register('newPassword')}
            />
          )}
        </FormField>
        <FormField
          id="reset-confirmPassword"
          label="Confirmar senha"
          error={errors.confirmPassword}
        >
          {(c) => (
            <Input
              {...c}
              type="password"
              autoComplete="new-password"
              {...register('confirmPassword')}
            />
          )}
        </FormField>
        <DialogFooter>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner />}
            Redefinir senha
          </Button>
        </DialogFooter>
      </FieldGroup>
    </form>
  )
}

export function ResetPasswordDialog({
  user,
  onOpenChange,
}: {
  user: UserDto | null
  onOpenChange: (open: boolean) => void
}) {
  return (
    <Dialog open={user !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Redefinir senha</DialogTitle>
          <DialogDescription>
            {user
              ? `Nova senha para ${user.fullName} (@${user.username}). As sessões abertas deste utilizador serão terminadas.`
              : ''}
          </DialogDescription>
        </DialogHeader>
        {user && (
          <ResetPasswordForm
            key={user.id}
            user={user}
            onDone={() => {
              onOpenChange(false)
            }}
          />
        )}
      </DialogContent>
    </Dialog>
  )
}
