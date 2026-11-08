import { z } from 'zod'

import { PAGE_SIZES } from './paged'

/**
 * URL search params arrive JSON-parsed by the router (`?q=123` → 123), so free text accepts numbers.
 * Invalid values are dropped (`.catch(undefined)`) instead of breaking the page.
 */
export const optionalText = (max = 150) =>
  z
    .preprocess(
      (value) =>
        (typeof value === 'string' || typeof value === 'number') && String(value).trim() !== ''
          ? String(value).trim()
          : undefined,
      z.string().max(max).optional(),
    )
    .catch(undefined)

export const optionalPage = () => z.coerce.number().int().min(1).optional().catch(undefined)

export const optionalPageSize = () =>
  z.coerce
    .number()
    .refine((value) => (PAGE_SIZES as readonly number[]).includes(value))
    .optional()
    .catch(undefined)

export const optionalDateOnly = () =>
  z
    .string()
    .regex(/^\d{4}-\d{2}-\d{2}$/)
    .optional()
    .catch(undefined)
