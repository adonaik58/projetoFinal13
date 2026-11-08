import { Outlet } from '@tanstack/react-router'
import { lazy, Suspense } from 'react'

// Devtools are loaded only in development builds.
const Devtools = import.meta.env.DEV
  ? lazy(async () => {
      const [router, query] = await Promise.all([
        import('@tanstack/react-router-devtools'),
        import('@tanstack/react-query-devtools'),
      ])
      return {
        default: function DevtoolsPanel() {
          return (
            <>
              <router.TanStackRouterDevtools position="bottom-right" />
              <query.ReactQueryDevtools buttonPosition="bottom-left" />
            </>
          )
        },
      }
    })
  : function NoDevtools() {
      return null
    }

export function RootLayout() {
  return (
    <>
      <Outlet />
      <Suspense>
        <Devtools />
      </Suspense>
    </>
  )
}
