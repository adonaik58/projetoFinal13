import { z } from 'zod'

import { passwordSchema, usernameSchema } from '@/features/auth/schemas'

const fullNameSchema = z
  .string()
  .trim()
  .min(2, 'Indique o nome completo.')
  .max(150, 'O nome deve ter no máximo 150 caracteres.')

const roleSchema = z.enum(['Admin', 'Operator'], { error: 'Escolha o perfil.' })

export const createUserSchema = z.object({
  username: usernameSchema,
  fullName: fullNameSchema,
  role: roleSchema,
  password: passwordSchema,
})
export type CreateUserValues = z.infer<typeof createUserSchema>

export const updateUserSchema = z.object({
  username: usernameSchema,
  fullName: fullNameSchema,
  role: roleSchema,
})
export type UpdateUserValues = z.infer<typeof updateUserSchema>

export const resetPasswordSchema = z
  .object({
    newPassword: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'As senhas não coincidem.',
  })
export type ResetPasswordValues = z.infer<typeof resetPasswordSchema>
