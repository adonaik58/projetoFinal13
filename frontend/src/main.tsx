import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from '@tanstack/react-router'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import { createQueryClient } from '@/app/query-client'
import { createAppRouter } from '@/app/router'
import { ThemeProvider } from '@/components/theme-provider'
import { Toaster } from '@/components/ui/sonner'
import { TooltipProvider } from '@/components/ui/tooltip'
import { setUnauthenticatedHandler } from '@/lib/http'
import { applyTheme } from '@/lib/theme'

import '@/styles/index.css'

applyTheme()

const queryClient = createQueryClient()
const router = createAppRouter(queryClient)

setUnauthenticatedHandler(() => {
  queryClient.clear()
  const { pathname, searchStr } = router.state.location
  if (pathname !== '/login') {
    void router.navigate({
      to: '/login',
      search: { redirect: pathname + searchStr },
      replace: true,
    })
  }
})

const rootElement = document.getElementById('root')
if (!rootElement) throw new Error('Root element #root not found.')

createRoot(rootElement).render(
  <StrictMode>
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <TooltipProvider delayDuration={300}>
          <RouterProvider router={router} />
          <Toaster position="top-right" richColors closeButton />
        </TooltipProvider>
      </QueryClientProvider>
    </ThemeProvider>
  </StrictMode>,
)
