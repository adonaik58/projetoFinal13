import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, type RenderOptions } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ReactElement, ReactNode } from 'react'

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
      mutations: { retry: false },
    },
  })
}

export function renderWithQuery(ui: ReactElement, options?: RenderOptions) {
  const queryClient = createTestQueryClient()
  function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
  return { queryClient, ...render(ui, { wrapper: Wrapper, ...options }) }
}

/** Intl uses narrow no-break spaces; normalise them for readable assertions. */
export const normalizeSpaces = (text: string) => text.replace(/\s/g, ' ')

/** user-event without artificial delays between keystrokes (keeps tests fast on busy CI runners). */
export const setupUser = () => userEvent.setup({ delay: null })
