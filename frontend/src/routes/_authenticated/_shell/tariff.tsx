import { createFileRoute } from '@tanstack/react-router'

import { prefetch } from '@/app/prefetch'
import { requirePermission } from '@/features/auth/guards'
import { tariffQueryOptions } from '@/features/settings/api'
import { TariffPage } from '@/features/settings/pages/tariff-page'

export const Route = createFileRoute('/_authenticated/_shell/tariff')({
  beforeLoad: requirePermission('settings.view'),
  loader: ({ context }) => {
    prefetch(context.queryClient, tariffQueryOptions())
  },
  staticData: { title: 'Tarifário' },
  component: TariffPage,
})
