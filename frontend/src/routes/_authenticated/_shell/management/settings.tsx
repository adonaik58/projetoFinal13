import { createFileRoute } from '@tanstack/react-router'

import { prefetch } from '@/app/prefetch'
import { requirePermission } from '@/features/auth/guards'
import { settingsQueryOptions } from '@/features/settings/api'
import { SettingsPage } from '@/features/settings/pages/settings-page'
import { settingsSearchSchema } from '@/features/settings/search'

export const Route = createFileRoute('/_authenticated/_shell/management/settings')({
  validateSearch: settingsSearchSchema,
  beforeLoad: requirePermission('settings.manage'),
  loader: ({ context }) => {
    prefetch(context.queryClient, settingsQueryOptions())
  },
  staticData: { title: 'Configuração', section: 'Gestão' },
  component: SettingsPage,
})
