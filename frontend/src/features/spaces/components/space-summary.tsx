import { SPACE_STATUS_META } from '../status'
import type { SpaceSummary as Summary } from '../types'

export function SpaceSummary({ summary }: { summary: Summary }) {
  const items = [
    { key: 'Free', value: summary.free },
    { key: 'Occupied', value: summary.occupied },
    { key: 'Maintenance', value: summary.maintenance },
  ] as const

  return (
    <dl className="flex flex-wrap items-center gap-2 text-sm" aria-label="Resumo de ocupação">
      <div className="rounded-full border px-3 py-1">
        <dt className="inline text-muted-foreground">Total: </dt>
        <dd className="inline font-semibold tabular-nums">{summary.total}</dd>
      </div>
      {items.map(({ key, value }) => {
        const meta = SPACE_STATUS_META[key]
        return (
          <div
            key={key}
            className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 ${meta.badgeClass}`}
          >
            <meta.icon className="size-3.5" aria-hidden="true" />
            <dt className="inline">{meta.label}: </dt>
            <dd className="inline font-semibold tabular-nums">{value}</dd>
          </div>
        )
      })}
    </dl>
  )
}
