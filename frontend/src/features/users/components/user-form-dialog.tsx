import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle } from 'lucide-react'
import { Controller, useForm } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Alert, AlertDescription } from '@/components/ui/alert'
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Spinner } from '@/components/ui/spinner'
import { ROLE_LABELS } from '@/features/auth/permissions'
import type { Role } from '@/features/auth/types'
import { applyFieldErrors, toApiError } from '@/lib/api-error'

import { useCreateUser, useUpdateUser } from '../api'
import {
  createUserSchema,
  updateUserSchema,
  type CreateUserValues,
  type UpdateUserValues,
} from '../schemas'
import type { UserDto } from '../types'

const ROLES: Role[] = ['Operator', 'Admin']

function RoleSelect({
  value,
  onChange,
  onBlur,
  error,
}: {
  value: Role | undefined
  onChange: (role: Role) => void
  onBlur: () => void
  error: { message?: string | undefined } | undefined
}) {
  return (
    <FormField id="user-role" label="Perfil" error={error}>
      {(controlProps) => (
        <Select
          value={value ?? ''}
          onValueChange={(role) => {
            onChange(role as Role)
          }}
        >
          <SelectTrigger {...controlProps} className="w-full" onBlur={onBlur}>
            <SelectValue placeholder="Escolher perfil" />
          </SelectTrigger>
          <SelectContent>
            {ROLES.map((role) => (
              <SelectItem key={role} value={role}>
                {ROLE_LABELS[role]}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      )}
    </FormField>
  )
}

function CreateUserForm({ onDone }: { onDone: () => void }) {
  const createUser = useCreateUser()
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { username: '', fullName: '', role: 'Operator', password: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      await createUser.mutateAsync(values)
      onDone()
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, [
        'username',
        'fullName',
        'role',
        'password',
      ])
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('root', { message: apiError.userMessage })
      }
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <FieldGroup>
        {errors.root?.message && (
          <Alert variant="destructive">
            <AlertCircle aria-hidden="true" />
            <AlertDescription>{errors.root.message}</AlertDescription>
          </Alert>
        )}
        <Controller
          control={control}
          name="role"
          render={({ field }) => (
            <RoleSelect
              value={field.value}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={errors.role}
            />
          )}
        />
        <FormField id="user-fullName" label="Nome completo" error={errors.fullName}>
          {(c) => <Input {...c} autoComplete="off" {...register('fullName')} />}
        </FormField>
        <FormField id="user-username" label="Nome de utilizador" error={errors.username}>
          {(c) => (
            <Input {...c} autoComplete="off" autoCapitalize="none" {...register('username')} />
          )}
        </FormField>
        <FormField
          id="user-password"
          label="Senha"
          error={errors.password}
          description="Pelo menos 8 caracteres, com letras e números."
        >
          {(c) => (
            <Input {...c} type="password" autoComplete="new-password" {...register('password')} />
          )}
        </FormField>
        <DialogFooter>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner />}
            Criar utilizador
          </Button>
        </DialogFooter>
      </FieldGroup>
    </form>
  )
}

function EditUserForm({ user, onDone }: { user: UserDto; onDone: () => void }) {
  const updateUser = useUpdateUser(user.id)
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<UpdateUserValues>({
    resolver: zodResolver(updateUserSchema),
    defaultValues: { username: user.username, fullName: user.fullName, role: user.role },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      await updateUser.mutateAsync(values)
      onDone()
    } catch (error) {
      const apiError = toApiError(error)
      const unmatched = applyFieldErrors(apiError, setError, ['username', 'fullName', 'role'])
      if (unmatched.length > 0 || Object.keys(apiError.errors).length === 0) {
        setError('root', { message: apiError.userMessage })
      }
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate>
      <FieldGroup>
        {errors.root?.message && (
          <Alert variant="destructive">
            <AlertCircle aria-hidden="true" />
            <AlertDescription>{errors.root.message}</AlertDescription>
          </Alert>
        )}
        <Controller
          control={control}
          name="role"
          render={({ field }) => (
            <RoleSelect
              value={field.value}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={errors.role}
            />
          )}
        />
        <FormField id="user-fullName" label="Nome completo" error={errors.fullName}>
          {(c) => <Input {...c} autoComplete="off" {...register('fullName')} />}
        </FormField>
        <FormField id="user-username" label="Nome de utilizador" error={errors.username}>
          {(c) => (
            <Input {...c} autoComplete="off" autoCapitalize="none" {...register('username')} />
          )}
        </FormField>
        <DialogFooter>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner />}
            Guardar alterações
          </Button>
        </DialogFooter>
      </FieldGroup>
    </form>
  )
}

export type UserDialogState = { mode: 'create' } | { mode: 'edit'; user: UserDto } | null

export function UserFormDialog({
  state,
  onOpenChange,
}: {
  state: UserDialogState
  onOpenChange: (open: boolean) => void
}) {
  const close = () => {
    onOpenChange(false)
  }
  return (
    <Dialog open={state !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {state?.mode === 'edit' ? 'Atualizar utilizador' : 'Criar novo utilizador'}
          </DialogTitle>
          <DialogDescription>
            {state?.mode === 'edit'
              ? 'Altere o perfil e os dados de identificação. A senha é redefinida à parte.'
              : 'O utilizador poderá iniciar sessão imediatamente.'}
          </DialogDescription>
        </DialogHeader>
        {state?.mode === 'create' && <CreateUserForm onDone={close} />}
        {state?.mode === 'edit' && (
          <EditUserForm key={state.user.id} user={state.user} onDone={close} />
        )}
      </DialogContent>
    </Dialog>
  )
}
