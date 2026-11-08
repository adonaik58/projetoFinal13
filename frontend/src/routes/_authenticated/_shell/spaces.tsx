import { createFileRoute } from '@tanstack/react-router'

import { prefetch } from '@/app/prefetch'
import { requirePermission } from '@/features/auth/guards'
import { spaceBoardQueryOptions } from '@/features/spaces/api'
import { SpacesPage } from '@/features/spaces/pages/spaces-page'
import { spacesSearchSchema } from '@/features/spaces/search'

export const Route = createFileRoute('/_authenticated/_shell/spaces')({
  validateSearch: spacesSearchSchema,
  beforeLoad: requirePermission('spaces.view'),
  loaderDeps: ({ search }) => search,
  loader: ({ context, deps }) => {
    prefetch(context.queryClient, spaceBoardQueryOptions(deps))
  },
  staticData: { title: 'Espaços' },
  component: SpacesPage,
})
