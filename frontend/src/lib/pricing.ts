/**
 * Same rule as the API's PricingPolicy, used to keep amounts live between server pushes
 * (the server still prices the ticket at check-out): whole elapsed minutes; nothing is charged
 * during the free period, then every minute beyond it is charged at the snapshot rate.
 */
export function elapsedMinutes(entryAt: string, now: number): number {
  const ms = now - Date.parse(entryAt)
  return ms <= 0 ? 0 : Math.floor(ms / 60_000)
}

export function amountFor(minutes: number, ratePerMinute: number, freeMinutes: number): number {
  if (minutes < freeMinutes) return 0
  return Math.round((minutes - freeMinutes) * ratePerMinute * 100) / 100
}
