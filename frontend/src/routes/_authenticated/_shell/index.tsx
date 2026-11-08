import { createFileRoute } from '@tanstack/react-router'

import { prefetch } from '@/app/prefetch'
import { requirePermission } from '@/features/auth/guards'
import { dashboardSummaryQueryOptions, revenueSeriesQueryOptions } from '@/features/dashboard/api'
import { DashboardPage } from '@/features/dashboard/pages/dashboard-page'
import { dashboardSearchSchema } from '@/features/dashboard/search'

export const Route = createFileRoute('/_authenticated/_shell/')({
  validateSearch: dashboardSearchSchema,
  beforeLoad: requirePermission('dashboard.view'),
  loaderDeps: ({ search }) => ({ period: search.period ?? 'today', days: search.days ?? 30 }),
  loader: ({ context, deps }) => {
    prefetch(context.queryClient, dashboardSummaryQueryOptions(deps.period))
    prefetch(context.queryClient, revenueSeriesQueryOptions(deps.days))
  },
  staticData: { title: 'Dashboard' },
  component: DashboardPage,
})
