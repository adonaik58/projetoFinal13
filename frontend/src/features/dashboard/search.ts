import { z } from 'zod'

export const dashboardSearchSchema = z.object({
  period: z.enum(['today', '7d', '30d', 'all']).optional().catch(undefined),
  days: z
    .union([z.literal(7), z.literal(30), z.literal(90)])
    .optional()
    .catch(undefined),
})
