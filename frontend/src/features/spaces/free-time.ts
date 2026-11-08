import type { Occupancy } from './types'

/** "1:05:09" / "04:59" */
export function formatCountdown(totalSeconds: number): string {
  const seconds = Math.max(0, Math.ceil(totalSeconds))
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  const mmss = `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  return h > 0 ? `${h}:${mmss}` : mmss
}

/** Seconds left of the free period (negative once it has ended). */
export function remainingFreeSeconds(occupancy: Occupancy, now: number): number {
  return (Date.parse(occupancy.freeUntil) - now) / 1000
}
