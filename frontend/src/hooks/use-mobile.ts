import { useSyncExternalStore } from 'react'

const MOBILE_BREAKPOINT = 768
const QUERY = `(max-width: ${String(MOBILE_BREAKPOINT - 1)}px)`

function subscribe(onChange: () => void) {
  if (typeof window.matchMedia !== 'function') return () => undefined
  const media = window.matchMedia(QUERY)
  media.addEventListener('change', onChange)
  return () => {
    media.removeEventListener('change', onChange)
  }
}

function getSnapshot() {
  return typeof window.matchMedia === 'function' ? window.matchMedia(QUERY).matches : false
}

export function useIsMobile(): boolean {
  return useSyncExternalStore(subscribe, getSnapshot, () => false)
}
