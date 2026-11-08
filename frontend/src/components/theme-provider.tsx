import { useEffect, useMemo, useState, type ReactNode } from 'react'

import {
  applyTheme,
  readStoredTheme,
  storeTheme,
  systemPrefersDark,
  ThemeContext,
  type ResolvedTheme,
  type Theme,
  type ThemeContextValue,
} from '@/lib/theme'

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(readStoredTheme)
  const [systemDark, setSystemDark] = useState(systemPrefersDark)

  useEffect(() => {
    if (typeof window.matchMedia !== 'function') return
    const media = window.matchMedia('(prefers-color-scheme: dark)')
    const onChange = (event: MediaQueryListEvent) => {
      setSystemDark(event.matches)
    }
    media.addEventListener('change', onChange)
    return () => {
      media.removeEventListener('change', onChange)
    }
  }, [])

  const resolvedTheme: ResolvedTheme = theme === 'system' ? (systemDark ? 'dark' : 'light') : theme

  useEffect(() => {
    applyTheme(theme)
  }, [theme, resolvedTheme])

  const value = useMemo<ThemeContextValue>(
    () => ({
      theme,
      resolvedTheme,
      setTheme: (next) => {
        storeTheme(next)
        setThemeState(next)
      },
    }),
    [theme, resolvedTheme],
  )

  return <ThemeContext value={value}>{children}</ThemeContext>
}
