import { useRealtimeStatus } from '@/hooks/use-realtime-sync'
import { cn } from '@/lib/utils'

const LABELS = {
  connected: 'Tempo real',
  connecting: 'A ligar…',
  reconnecting: 'A reconectar…',
  offline: 'Offline',
} as const

/** Shows whether views are updated by server push or by the polling fallback. */
export function RealtimeIndicator() {
  const status = useRealtimeStatus()
  const live = status === 'connected'
  return (
    <span
      role="status"
      title={
        live
          ? 'Atualizações em tempo real'
          : 'Sem ligação em tempo real: a atualizar periodicamente'
      }
      className="inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs text-muted-foreground"
    >
      <span
        aria-hidden="true"
        className={cn(
          'size-2 rounded-full',
          live ? 'bg-emerald-500' : 'animate-pulse bg-amber-500',
        )}
      />
      <span className="hidden sm:inline">{LABELS[status]}</span>
      <span className="sr-only sm:hidden">{LABELS[status]}</span>
    </span>
  )
}
