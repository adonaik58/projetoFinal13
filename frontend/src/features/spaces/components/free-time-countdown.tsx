import { Gift, TimerOff } from 'lucide-react'

import { cn } from '@/lib/utils'
import { formatTime } from '@/lib/format'

import { formatCountdown, remainingFreeSeconds } from '../free-time'
import type { Occupancy } from '../types'

/** Live countdown of the free period of an open ticket (`now` already corrected by the server clock offset). */
export function FreeTimeCountdown({
  occupancy,
  now,
  compact = false,
}: {
  occupancy: Occupancy
  now: number
  compact?: boolean
}) {
  if (occupancy.freeMinutes <= 0) {
    return compact ? null : (
      <p className="text-sm text-muted-foreground">Sem tempo grátis: cobrado desde a entrada.</p>
    )
  }

  const remaining = remainingFreeSeconds(occupancy, now)
  const active = remaining > 0

  if (compact) {
    return active ? (
      <span className="flex items-center gap-1 font-medium tabular-nums">
        <Gift className="size-3" aria-hidden="true" />
        Grátis {formatCountdown(remaining)}
      </span>
    ) : null
  }

  const progress = active ? Math.min(100, (remaining / (occupancy.freeMinutes * 60)) * 100) : 0

  return (
    <div
      className={cn(
        'rounded-xl border p-3',
        active ? 'border-emerald-500/40 bg-emerald-500/10' : 'border-amber-500/40 bg-amber-500/10',
      )}
    >
      <div className="flex items-center justify-between gap-3 text-sm">
        <span className="inline-flex items-center gap-1.5 font-medium">
          {active ? (
            <Gift className="size-4" aria-hidden="true" />
          ) : (
            <TimerOff className="size-4" aria-hidden="true" />
          )}
          {active ? 'Tempo grátis restante' : 'Tempo grátis terminado'}
        </span>
        <span className="font-mono text-base font-semibold tabular-nums" aria-live="off">
          {active ? formatCountdown(remaining) : `às ${formatTime(occupancy.freeUntil)}`}
        </span>
      </div>
      {active && (
        <div
          className="mt-2 h-1.5 overflow-hidden rounded-full bg-emerald-500/20"
          role="progressbar"
          aria-label="Tempo grátis restante"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={Math.round(progress)}
        >
          <div className="h-full rounded-full bg-emerald-600" style={{ width: `${progress}%` }} />
        </div>
      )}
      <p className="mt-1.5 text-xs text-muted-foreground">
        {occupancy.freeMinutes} min grátis
        {active ? `, até às ${formatTime(occupancy.freeUntil)}` : '; a cobrar desde então'}.
      </p>
    </div>
  )
}
