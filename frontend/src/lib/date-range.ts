import { parseDateOnly } from './format'

export const DATE_PRESETS = ['today', 'yesterday', '3d', '1w', '1m', '1y'] as const
export type DatePreset = (typeof DATE_PRESETS)[number]

export const DATE_PRESET_LABELS: Record<DatePreset, string> = {
  today: 'Hoje',
  yesterday: 'Ontem',
  '3d': '3 dias',
  '1w': '1 semana',
  '1m': '1 mês',
  '1y': '1 ano',
}

export interface DateRange {
  /** Inclusive lower bound, ISO-8601 UTC. */
  from?: string
  /** Inclusive upper bound, ISO-8601 UTC. */
  to?: string
}

function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate())
}

function endOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 59, 999)
}

function addDays(date: Date, days: number): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days)
}

/** Local-time calendar boundaries for a preset, expressed as UTC instants for the API. */
export function presetRange(preset: DatePreset, now: Date = new Date()): Required<DateRange> {
  const today = startOfDay(now)
  const endOfToday = endOfDay(now)
  switch (preset) {
    case 'today':
      return { from: today.toISOString(), to: endOfToday.toISOString() }
    case 'yesterday': {
      const yesterday = addDays(today, -1)
      return { from: yesterday.toISOString(), to: endOfDay(yesterday).toISOString() }
    }
    case '3d':
      return { from: addDays(today, -2).toISOString(), to: endOfToday.toISOString() }
    case '1w':
      return { from: addDays(today, -6).toISOString(), to: endOfToday.toISOString() }
    case '1m':
      return {
        from: new Date(today.getFullYear(), today.getMonth() - 1, today.getDate()).toISOString(),
        to: endOfToday.toISOString(),
      }
    case '1y':
      return {
        from: new Date(today.getFullYear() - 1, today.getMonth(), today.getDate()).toISOString(),
        to: endOfToday.toISOString(),
      }
  }
}

/** Custom local `YYYY-MM-DD` dates → inclusive UTC instants. */
export function customRange(fromDate?: string, toDate?: string): DateRange {
  const range: DateRange = {}
  const from = fromDate ? parseDateOnly(fromDate) : null
  const to = toDate ? parseDateOnly(toDate) : null
  if (from) range.from = startOfDay(from).toISOString()
  if (to) range.to = endOfDay(to).toISOString()
  return range
}
