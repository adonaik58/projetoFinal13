import type { QueryClient } from '@tanstack/react-query'
import { createRouter } from '@tanstack/react-router'

import { RouteError } from '@/components/common/route-error'
import { PageSkeleton } from '@/components/common/page-skeleton'
import { NotFound } from '@/components/common/not-found'
import { routeTree } from '@/routeTree.gen'

export interface RouterContext {
  queryClient: QueryClient
}

export function createAppRouter(queryClient: QueryClient) {
  return createRouter({
    routeTree,
    context: { queryClient },
    defaultPreload: 'intent',
    // Data freshness is owned by TanStack Query.
    defaultPreloadStaleTime: 0,
    scrollRestoration: true,
    defaultPendingMs: 300,
    defaultPendingComponent: PageSkeleton,
    defaultErrorComponent: RouteError,
    defaultNotFoundComponent: NotFound,
  })
}

declare module '@tanstack/react-router' {
  interface Register {
    router: ReturnType<typeof createAppRouter>
  }
  interface StaticDataRouteOption {
    /** Page title used by the breadcrumb and document title. */
    title?: string
    /** Optional parent section shown in the breadcrumb (e.g. "Gestão"). */
    section?: string
  }
}
