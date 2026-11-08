import { queryOptions, useMutation, useQueryClient } from '@tanstack/react-query'
import { z } from 'zod'

import { http } from '@/lib/http'

import type { BrandDto, ModelDto } from './types'

export const catalogKeys = {
  all: ['catalog'] as const,
  brands: () => [...catalogKeys.all, 'brands'] as const,
  models: (brandId: number) => [...catalogKeys.all, 'brands', brandId, 'models'] as const,
}

// Reference data changes rarely; keep it fresh for 10 minutes.
const CATALOG_STALE_TIME = 10 * 60_000

export const brandsQueryOptions = () =>
  queryOptions({
    queryKey: catalogKeys.brands(),
    queryFn: async ({ signal }) => (await http.get<BrandDto[]>('/catalog/brands', { signal })).data,
    staleTime: CATALOG_STALE_TIME,
  })

export const modelsQueryOptions = (brandId: number | null | undefined) =>
  queryOptions({
    queryKey: catalogKeys.models(brandId ?? 0),
    queryFn: async ({ signal }) =>
      (await http.get<ModelDto[]>(`/catalog/brands/${String(brandId)}/models`, { signal })).data,
    enabled: !!brandId,
    staleTime: CATALOG_STALE_TIME,
  })

export const catalogNameSchema = z
  .string()
  .trim()
  .min(1, 'Indique o nome.')
  .max(80, 'O nome deve ter no máximo 80 caracteres.')

export const brandFormSchema = z.object({ name: catalogNameSchema })
export type BrandFormValues = z.infer<typeof brandFormSchema>

export function useCreateBrand() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (values: BrandFormValues) =>
      (await http.post<BrandDto>('/catalog/brands', values)).data,
    meta: { successMessage: 'Marca adicionada.', skipGlobalErrorToast: true },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: catalogKeys.brands() }),
  })
}

export function useCreateModel(brandId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (values: BrandFormValues) =>
      (await http.post<ModelDto>(`/catalog/brands/${String(brandId)}/models`, values)).data,
    meta: { successMessage: 'Modelo adicionado.', skipGlobalErrorToast: true },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: catalogKeys.models(brandId) }),
  })
}
