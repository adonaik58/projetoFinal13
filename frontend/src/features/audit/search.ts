import { z } from 'zod'

import { optionalDateOnly, optionalPage, optionalPageSize, optionalText } from '@/lib/search-params'

/** URL search params of the audit page. */
export const auditSearchSchema = z.object({
  page: optionalPage(),
  pageSize: optionalPageSize(),
  action: optionalText(100),
  username: optionalText(100),
  fromDate: optionalDateOnly(),
  toDate: optionalDateOnly(),
})
