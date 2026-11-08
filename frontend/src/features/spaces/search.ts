import { z } from 'zod'

import { optionalText } from '@/lib/search-params'

/** URL search params of the /spaces page. */
export const spacesSearchSchema = z.object({
  search: optionalText(10),
  plate: optionalText(20),
  status: z.enum(['Free', 'Occupied', 'Maintenance']).optional().catch(undefined),
  sortDir: z.enum(['asc', 'desc']).optional().catch(undefined),
})
