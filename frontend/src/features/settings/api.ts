import { queryOptions, useMutation, useQueryClient } from '@tanstack/react-query'

import { http } from '@/lib/http'

import type { SettingsValues } from './schemas'
import type { SettingsDto, TariffDto } from './types'

export const settingsKeys = {
  all: ['settings'] as const,
  current: () => [...settingsKeys.all, 'current'] as const,
  tariff: () => [...settingsKeys.all, 'tariff'] as const,
}

export const settingsQueryOptions = () =>
  queryOptions({
    queryKey: settingsKeys.current(),
    queryFn: async ({ signal }) => (await http.get<SettingsDto>('/settings', { signal })).data,
  })

export const tariffQueryOptions = () =>
  queryOptions({
    queryKey: settingsKeys.tariff(),
    queryFn: async ({ signal }) => (await http.get<TariffDto>('/settings/tariff', { signal })).data,
  })

export function useUpdateSettings() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (values: SettingsValues) =>
      (await http.put<SettingsDto>('/settings', values)).data,
    meta: { successMessage: 'Configuração atualizada.', skipGlobalErrorToast: true },
    onSuccess: (data) => {
      queryClient.setQueryData(settingsKeys.current(), data)
      return Promise.all([
        queryClient.invalidateQueries({ queryKey: settingsKeys.tariff() }),
        queryClient.invalidateQueries({ queryKey: ['spaces'] }),
      ])
    },
  })
}
