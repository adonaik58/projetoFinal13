export interface SettingsDto {
  ratePerMinute: number
  freeHours: number
  maxSpaces: number
  updatedAt: string | null
  updatedBy: string | null
}

export interface TariffDto {
  ratePerMinute: number
  freeHours: number
  examples: { minutes: number; amount: number }[]
}
