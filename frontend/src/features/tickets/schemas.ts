import { z } from 'zod'

import { ageFrom, parseDateOnly } from '@/lib/format'

export const NATIONAL_ID_PATTERN = /^\d{9}[A-Z]{2}\d{3}$/
export const PLATE_PATTERN = /^[A-Z0-9][A-Z0-9 -]{0,18}[A-Z0-9]$/
export const COLOR_PATTERN = /^#[0-9a-fA-F]{6}$/
export const PHONE_PATTERN = /^\+?\d{9,15}$/

/** Optional contact: separators are ignored, blank becomes `null`. */
export const phoneSchema = z
  .string()
  .transform((value) => value.replace(/[\s.()-]/g, ''))
  .refine((value) => value === '' || PHONE_PATTERN.test(value), {
    message: 'Contacto inválido: use 9 a 15 dígitos (ex.: 923 456 789).',
  })
  .transform((value) => (value === '' ? null : value))

export const nationalIdSchema = z
  .string()
  .trim()
  .toUpperCase()
  .length(14, 'O Bilhete de Identidade contém 14 caracteres.')
  .regex(NATIONAL_ID_PATTERN, 'Bilhete de Identidade inválido (ex.: 123456789LA123).')

export function isValidNationalId(value: string): boolean {
  return NATIONAL_ID_PATTERN.test(value.trim().toUpperCase())
}

export const plateSchema = z
  .string()
  .trim()
  .toUpperCase()
  .min(2, 'Indique a matrícula.')
  .max(20, 'A matrícula deve ter no máximo 20 caracteres.')
  .regex(PLATE_PATTERN, 'Use apenas letras, números, espaços ou hífens (ex.: LD-12-34-AB).')

export const colorSchema = z.string().regex(COLOR_PATTERN, 'Escolha uma cor válida.')

export const birthDateSchema = z
  .string()
  .min(1, 'Indique a data de nascimento.')
  .refine((value) => parseDateOnly(value) !== null, 'Data inválida.')
  .refine((value) => {
    const date = parseDateOnly(value)
    return date !== null && date.getTime() < Date.now()
  }, 'A data de nascimento tem de ser no passado.')
  .refine((value) => (ageFrom(value) ?? 0) <= 120, 'Data de nascimento demasiado antiga.')

export const checkInSchema = z.object({
  spaceId: z.number().int().positive(),
  customer: z.object({
    nationalId: nationalIdSchema,
    fullName: z
      .string()
      .trim()
      .min(2, 'Indique o nome completo.')
      .max(150, 'O nome deve ter no máximo 150 caracteres.'),
    birthDate: birthDateSchema,
    phone: phoneSchema,
  }),
  vehicle: z.object({
    plate: plateSchema,
    brandId: z.number({ error: 'Escolha a marca.' }).int().positive('Escolha a marca.'),
    modelId: z.number().int().positive().nullable(),
    color: colorSchema,
  }),
})
export type CheckInInput = z.input<typeof checkInSchema>
export type CheckInValues = z.output<typeof checkInSchema>

export const CHECK_IN_FIELDS = [
  'customer.nationalId',
  'customer.fullName',
  'customer.birthDate',
  'customer.phone',
  'vehicle.plate',
  'vehicle.brandId',
  'vehicle.modelId',
  'vehicle.color',
] as const
