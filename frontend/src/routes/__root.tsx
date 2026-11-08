import { createRootRouteWithContext } from '@tanstack/react-router'

import type { RouterContext } from '@/app/router'
import { NotFound } from '@/components/common/not-found'
import { RootLayout } from '@/components/layout/root-layout'

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
  notFoundComponent: NotFound,
})
