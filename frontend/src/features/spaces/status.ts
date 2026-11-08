import { CircleCheck, CircleX, Wrench, type LucideIcon } from 'lucide-react'

import type { SpaceStatus } from './types'

export const SPACE_STATUSES: SpaceStatus[] = ['Free', 'Occupied', 'Maintenance']

/** Status is conveyed by colour, icon and text together (never colour alone). */
export const SPACE_STATUS_META: Record<
  SpaceStatus,
  { label: string; icon: LucideIcon; cardClass: string; badgeClass: string }
> = {
  Free: {
    label: 'Livre',
    icon: CircleCheck,
    cardClass:
      'border-emerald-600/40 bg-emerald-50 text-emerald-950 hover:bg-emerald-100 dark:border-emerald-400/30 dark:bg-emerald-950/40 dark:text-emerald-50 dark:hover:bg-emerald-950/70',
    badgeClass: 'bg-emerald-600 text-white dark:bg-emerald-500 dark:text-emerald-950',
  },
  Occupied: {
    label: 'Ocupado',
    icon: CircleX,
    cardClass:
      'border-red-600/40 bg-red-50 text-red-950 hover:bg-red-100 dark:border-red-400/30 dark:bg-red-950/40 dark:text-red-50 dark:hover:bg-red-950/70',
    badgeClass: 'bg-red-600 text-white dark:bg-red-500 dark:text-red-950',
  },
  Maintenance: {
    label: 'Indisponível',
    icon: Wrench,
    cardClass:
      'border-amber-600/40 bg-amber-50 text-amber-950 hover:bg-amber-100 dark:border-amber-400/30 dark:bg-amber-950/40 dark:text-amber-50 dark:hover:bg-amber-950/70',
    badgeClass: 'bg-amber-500 text-amber-950 dark:bg-amber-400 dark:text-amber-950',
  },
}
