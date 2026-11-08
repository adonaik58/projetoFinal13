import { z } from 'zod'

import { optionalPage, optionalPageSize, optionalText } from '@/lib/search-params'

/** URL search params of the users page. */
export const usersSearchSchema = z.object({
  page: optionalPage(),
  pageSize: optionalPageSize(),
  search: optionalText(100),
  role: z.enum(['Admin', 'Operator']).optional().catch(undefined),
  status: z.enum(['active', 'inactive']).optional().catch(undefined),
})
