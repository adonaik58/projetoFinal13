import { use } from 'react'

import { ThemeContext, type ThemeContextValue } from '@/lib/theme'

export function useTheme(): ThemeContextValue {
  const context = use(ThemeContext)
  if (!context) throw new Error('useTheme must be used inside <ThemeProvider>.')
  return context
}
