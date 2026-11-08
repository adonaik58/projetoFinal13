import { useSyncExternalStore } from 'react'

/**
 * One shared 1-second ticker for every live timer on screen, started only while
 * something is subscribed (instead of one interval per card).
 */
const listeners = new Set<() => void>()
let now = Date.now()
let timer: ReturnType<typeof setInterval> | undefined

function subscribe(listener: () => void) {
  listeners.add(listener)
  if (!timer) {
    now = Date.now()
    timer = setInterval(() => {
      now = Date.now()
      for (const l of listeners) l()
    }, 1000)
  }
  return () => {
    listeners.delete(listener)
    if (listeners.size === 0 && timer) {
      clearInterval(timer)
      timer = undefined
    }
  }
}

const getSnapshot = () => now

export function useNow(): number {
  return useSyncExternalStore(subscribe, getSnapshot, getSnapshot)
}
