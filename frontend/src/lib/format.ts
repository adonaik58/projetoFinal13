const LOCALE = 'pt-AO'

const currencyFormatter = new Intl.NumberFormat(LOCALE, {
  style: 'currency',
  currency: 'AOA',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const numberFormatter = new Intl.NumberFormat(LOCALE)

const dateTimeFormatter = new Intl.DateTimeFormat(LOCALE, {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

const dateFormatter = new Intl.DateTimeFormat(LOCALE, {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
})

const timeFormatter = new Intl.DateTimeFormat(LOCALE, { hour: '2-digit', minute: '2-digit' })

/** 1250.5 → "1 250,50 Kz" */
export function formatCurrency(amount: number | null | undefined): string {
  return currencyFormatter.format(amount ?? 0)
}

export function formatNumber(value: number | null | undefined): string {
  return numberFormatter.format(value ?? 0)
}

/** ISO instant (UTC) → local date and time. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? '—' : dateTimeFormatter.format(date)
}

export function formatTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? '—' : timeFormatter.format(date)
}

/** Parses a `YYYY-MM-DD` value as a local calendar date (no time-zone shift). */
export function parseDateOnly(value: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value)
  if (!match) return null
  const [, y, m, d] = match
  const date = new Date(Number(y), Number(m) - 1, Number(d))
  return Number.isNaN(date.getTime()) ? null : date
}

/** Local calendar date → `YYYY-MM-DD`. */
export function toDateOnly(date: Date): string {
  const y = String(date.getFullYear()).padStart(4, '0')
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return '—'
  const date = parseDateOnly(value) ?? new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : dateFormatter.format(date)
}

const MINUTES_PER_HOUR = 60
const MINUTES_PER_DAY = 24 * MINUTES_PER_HOUR
const MINUTES_PER_MONTH = 30 * MINUTES_PER_DAY
const MINUTES_PER_YEAR = 365 * MINUTES_PER_DAY

/**
 * Human duration in the same style as the legacy system, keeping the two most significant units:
 * "1 ano e 2 meses", "2m e 3d", "3d e 4h", "2h e 5min", "45min".
 */
export function formatDurationMinutes(totalMinutes: number | null | undefined): string {
  const minutes = Math.max(0, Math.floor(totalMinutes ?? 0))
  if (minutes >= MINUTES_PER_YEAR) {
    const years = Math.floor(minutes / MINUTES_PER_YEAR)
    const months = Math.floor((minutes % MINUTES_PER_YEAR) / MINUTES_PER_MONTH)
    const yearLabel = years === 1 ? 'ano' : 'anos'
    if (months === 0) return `${years} ${yearLabel}`
    return `${years} ${yearLabel} e ${months} ${months === 1 ? 'mês' : 'meses'}`
  }
  if (minutes >= MINUTES_PER_MONTH) {
    const months = Math.floor(minutes / MINUTES_PER_MONTH)
    const days = Math.floor((minutes % MINUTES_PER_MONTH) / MINUTES_PER_DAY)
    return `${months}m e ${days}d`
  }
  if (minutes >= MINUTES_PER_DAY) {
    const days = Math.floor(minutes / MINUTES_PER_DAY)
    const hours = Math.floor((minutes % MINUTES_PER_DAY) / MINUTES_PER_HOUR)
    return `${days}d e ${hours}h`
  }
  if (minutes >= MINUTES_PER_HOUR) {
    const hours = Math.floor(minutes / MINUTES_PER_HOUR)
    const rest = minutes % MINUTES_PER_HOUR
    return rest === 0 ? `${hours}h` : `${hours}h e ${rest}min`
  }
  return `${minutes}min`
}

/** Live timer text with second precision below one hour: "12min e 05s", "40s". */
export function formatElapsedSeconds(totalSeconds: number): string {
  const seconds = Math.max(0, Math.floor(totalSeconds))
  if (seconds >= 3600) return formatDurationMinutes(seconds / 60)
  const minutes = Math.floor(seconds / 60)
  const rest = seconds % 60
  if (minutes === 0) return `${rest}s`
  return `${minutes}min e ${String(rest).padStart(2, '0')}s`
}

/** Full years between a `YYYY-MM-DD` birth date and `now`. */
export function ageFrom(birthDate: string, now: Date = new Date()): number | null {
  const birth = parseDateOnly(birthDate)
  if (!birth) return null
  let age = now.getFullYear() - birth.getFullYear()
  const beforeBirthday =
    now.getMonth() < birth.getMonth() ||
    (now.getMonth() === birth.getMonth() && now.getDate() < birth.getDate())
  if (beforeBirthday) age -= 1
  return age
}

/** "Maria da Silva" → "MS" */
export function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  const first = parts[0]?.[0] ?? ''
  const last = parts.length > 1 ? (parts[parts.length - 1]?.[0] ?? '') : ''
  return (first + last).toUpperCase() || '?'
}
