import { z } from 'zod'

export const settingsSchema = z.object({
  ratePerMinute: z
    .number({ error: 'Indique a renda por minuto.' })
    .positive('A renda por minuto deve ser superior a zero.')
    .max(1_000_000, 'Valor demasiado elevado.')
    .multipleOf(0.01, 'Use no máximo duas casas decimais.'),
  freeHours: z
    .number({ error: 'Indique as horas grátis.' })
    .int('Use um número inteiro de horas.')
    .min(0, 'Não pode ser negativo.')
    .max(720, 'No máximo 720 horas.'),
  maxSpaces: z
    .number({ error: 'Indique a capacidade.' })
    .int('Use um número inteiro.')
    .min(1, 'Pelo menos 1 espaço.')
    .max(100_000, 'No máximo 100 000 espaços.'),
})
export type SettingsValues = z.infer<typeof settingsSchema>
