import { z } from 'zod'

export const SPACE_CODE_PATTERN = /^[A-Z]{1,3}\d{1,4}$/
export const SECTION_PATTERN = /^[A-Z]{1,3}$/

export const spaceCodeSchema = z
  .string()
  .trim()
  .toUpperCase()
  .regex(SPACE_CODE_PATTERN, 'Use 1 a 3 letras seguidas de 1 a 4 dígitos (ex.: A1, B12).')

export const createSpaceSchema = z.object({ code: spaceCodeSchema })
export type CreateSpaceValues = z.infer<typeof createSpaceSchema>

/**
 * Accepts a comma/space separated list of sections and single-letter ranges:
 * "A, B, C", "A-C", "A-C, AA".
 */
export function parseSections(input: string): string[] | null {
  const tokens = input
    .toUpperCase()
    .split(/[\s,;]+/)
    .map((t) => t.trim())
    .filter(Boolean)
  if (tokens.length === 0) return null

  const sections = new Set<string>()
  for (const token of tokens) {
    const range = /^([A-Z])-([A-Z])$/.exec(token)
    if (range) {
      const start = range[1]?.charCodeAt(0) ?? 0
      const end = range[2]?.charCodeAt(0) ?? 0
      if (start > end) return null
      for (let c = start; c <= end; c++) sections.add(String.fromCharCode(c))
    } else if (SECTION_PATTERN.test(token)) {
      sections.add(token)
    } else {
      return null
    }
  }
  return [...sections]
}

export const bulkSpacesSchema = z
  .object({
    sections: z
      .string()
      .trim()
      .min(1, 'Indique pelo menos uma secção.')
      .refine((value) => parseSections(value) !== null, {
        message: 'Use letras separadas por vírgulas ou intervalos (ex.: A-C, AA).',
      }),
    fromNumber: z
      .number({ error: 'Indique o número inicial.' })
      .int('Use um número inteiro.')
      .min(1, 'Mínimo 1.')
      .max(9999, 'Máximo 9999.'),
    toNumber: z
      .number({ error: 'Indique o número final.' })
      .int('Use um número inteiro.')
      .min(1, 'Mínimo 1.')
      .max(9999, 'Máximo 9999.'),
  })
  .refine((v) => v.toNumber >= v.fromNumber, {
    path: ['toNumber'],
    message: 'O número final deve ser maior ou igual ao inicial.',
  })
export type BulkSpacesValues = z.infer<typeof bulkSpacesSchema>
