import { Outlet } from '@tanstack/react-router'

import { SidebarInset, SidebarProvider } from '@/components/ui/sidebar'
import { useRealtimeSync } from '@/hooks/use-realtime-sync'

import { AppHeader } from './app-header'
import { AppSidebar } from './app-sidebar'
import { PageTransition } from './page-transition'

export function AppShell() {
  useRealtimeSync()

  return (
    <SidebarProvider>
      <a
        href="#main-content"
        className="sr-only z-50 rounded-md bg-background px-3 py-2 focus:not-sr-only focus:fixed focus:top-2 focus:left-2"
      >
        Saltar para o conteúdo
      </a>
      <AppSidebar />
      <SidebarInset>
        <AppHeader />
        <div id="main-content" tabIndex={-1} className="flex-1 p-4 outline-none md:p-6">
          <PageTransition>
            <Outlet />
          </PageTransition>
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}
