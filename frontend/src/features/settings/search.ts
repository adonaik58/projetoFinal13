import { z } from 'zod'

export const SETTINGS_TABS = ['tariff', 'spaces', 'catalog'] as const
export type SettingsTab = (typeof SETTINGS_TABS)[number]

export const settingsSearchSchema = z.object({
  tab: z.enum(SETTINGS_TABS).optional().catch(undefined),
})
