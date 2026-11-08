import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Eye, EyeOff, LogIn } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'

import { FormField } from '@/components/common/form-field'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { FieldGroup } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { toApiError } from '@/lib/api-error'

import { login } from '../api'
import { loginSchema, type LoginValues } from '../schemas'
import type { AuthUser } from '../types'

export function LoginForm({ onSuccess }: { onSuccess: (user: AuthUser) => void | Promise<void> }) {
  const [showPassword, setShowPassword] = useState(false)
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { username: '', password: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      const user = await login(values)
      await onSuccess(user)
    } catch (error) {
      const apiError = toApiError(error)
      setError('root', {
        message:
          apiError.status === 401
            ? 'Nome de utilizador ou senha incorretos.'
            : apiError.userMessage,
      })
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} noValidate aria-label="Iniciar sessão">
      <FieldGroup>
        {errors.root?.message && (
          <Alert variant="destructive">
            <AlertCircle aria-hidden="true" />
            <AlertDescription>{errors.root.message}</AlertDescription>
          </Alert>
        )}
        <FormField id="username" label="Nome de utilizador" error={errors.username}>
          {(control) => (
            <Input
              {...control}
              autoComplete="username"
              autoCapitalize="none"
              spellCheck={false}
              {...register('username')}
            />
          )}
        </FormField>
        <FormField id="password" label="Senha" error={errors.password}>
          {(control) => (
            <div className="relative">
              <Input
                {...control}
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                className="pr-10"
                {...register('password')}
              />
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                className="absolute top-1/2 right-1 -translate-y-1/2"
                aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
                aria-pressed={showPassword}
                onClick={() => {
                  setShowPassword((value) => !value)
                }}
              >
                {showPassword ? <EyeOff aria-hidden="true" /> : <Eye aria-hidden="true" />}
              </Button>
            </div>
          )}
        </FormField>
        <Button type="submit" size="lg" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? <Spinner /> : <LogIn aria-hidden="true" />}
          Entrar
        </Button>
      </FieldGroup>
    </form>
  )
}
