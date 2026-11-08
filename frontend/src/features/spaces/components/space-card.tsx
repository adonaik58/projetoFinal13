import { Clock } from 'lucide-react'

import { cn } from '@/lib/utils'
import { formatCurrency, formatElapsedSeconds } from '@/lib/format'

import { formatCountdown, remainingFreeSeconds } from '../free-time'
import { SPACE_STATUS_META } from '../status'
import type { SpaceDto } from '../types'
import { FreeTimeCountdown } from './free-time-countdown'

export function SpaceCard({
  space,
  now,
  onSelect,
}: {
  space: SpaceDto
  /** Current time already corrected by the server clock offset (ms). */
  now: number
  onSelect: (space: SpaceDto) => void
}) {
  const meta = SPACE_STATUS_META[space.status]
  const occupancy = space.occupancy
  const elapsedSeconds = occupancy ? (now - Date.parse(occupancy.entryAt)) / 1000 : 0
  const freeSeconds = occupancy ? remainingFreeSeconds(occupancy, now) : 0
  const description = occupancy
    ? `${meta.label}, matrícula ${occupancy.vehicle.plate}, há ${formatElapsedSeconds(elapsedSeconds)}` +
      (freeSeconds > 0 ? `, tempo grátis restante ${formatCountdown(freeSeconds)}` : '')
    : meta.label

  return (
    <button
      type="button"
      onClick={() => {
        onSelect(space)
      }}
      aria-label={`Espaço ${space.code}: ${description}`}
      data-status={space.status}
      className={cn(
        'flex min-h-28 w-full flex-col justify-between gap-2 rounded-xl border p-3 text-left shadow-xs transition-colors focus-visible:ring-3 focus-visible:ring-ring/60 focus-visible:outline-none',
        meta.cardClass,
      )}
    >
      <span className="flex items-start justify-between gap-2">
        <span className="text-lg font-semibold tracking-tight">{space.code}</span>
        <span className="inline-flex items-center gap-1 text-xs font-medium">
          <meta.icon className="size-3.5" aria-hidden="true" />
          {meta.label}
        </span>
      </span>
      {occupancy ? (
        <span className="space-y-0.5 text-xs">
          <span className="block truncate font-mono font-medium">{occupancy.vehicle.plate}</span>
          <span className="flex items-center gap-1 tabular-nums">
            <Clock className="size-3" aria-hidden="true" />
            {formatElapsedSeconds(elapsedSeconds)}
          </span>
          <FreeTimeCountdown occupancy={occupancy} now={now} compact />
          <span className="block font-semibold tabular-nums">
            {formatCurrency(occupancy.currentAmount)}
          </span>
        </span>
      ) : (
        <span className="text-xs opacity-80">
          {space.status === 'Free' ? 'Disponível para ocupação' : 'Fora de serviço'}
        </span>
      )}
    </button>
  )
}
