import { z } from 'zod'

export const USERNAME_PATTERN = /^[A-Za-z0-9._-]+$/

export const usernameSchema = z
  .string()
  .trim()
  .min(3, 'O nome de utilizador deve ter pelo menos 3 caracteres.')
  .max(50, 'O nome de utilizador deve ter no máximo 50 caracteres.')
  .regex(USERNAME_PATTERN, 'Use apenas letras, números, ponto, hífen ou underscore.')

export const passwordSchema = z
  .string()
  .min(8, 'A senha deve ter pelo menos 8 caracteres.')
  .max(128, 'A senha deve ter no máximo 128 caracteres.')
  .regex(/[A-Za-z]/, 'A senha deve conter pelo menos uma letra.')
  .regex(/\d/, 'A senha deve conter pelo menos um número.')

export const loginSchema = z.object({
  username: z.string().trim().min(1, 'Indique o nome de utilizador.'),
  // Legacy accounts may still have short passwords, so login only requires a value.
  password: z.string().min(1, 'Indique a senha.'),
})
export type LoginValues = z.infer<typeof loginSchema>

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Indique a senha atual.'),
    newPassword: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'As senhas não coincidem.',
  })
  .refine((v) => v.newPassword !== v.currentPassword, {
    path: ['newPassword'],
    message: 'A nova senha deve ser diferente da atual.',
  })
export type ChangePasswordValues = z.infer<typeof changePasswordSchema>
