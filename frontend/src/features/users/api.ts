import { keepPreviousData, queryOptions, useMutation, useQueryClient } from '@tanstack/react-query'

import { http } from '@/lib/http'
import { compactParams, type Paged } from '@/lib/paged'
import { whenRealtimeDown } from '@/lib/realtime'

import type { CreateUserValues, UpdateUserValues } from './schemas'
import type { UserDto, UserQueryParams } from './types'

export const userKeys = {
  all: ['users'] as const,
  list: (params: UserQueryParams) => [...userKeys.all, 'list', params] as const,
}

export const usersQueryOptions = (params: UserQueryParams) =>
  queryOptions({
    queryKey: userKeys.list(params),
    queryFn: async ({ signal }) =>
      (await http.get<Paged<UserDto>>('/users', { params: compactParams(params), signal })).data,
    placeholderData: keepPreviousData,
    // Logins, logouts and account changes arrive by server push; poll only while it is down.
    refetchInterval: whenRealtimeDown(60_000),
  })

function useInvalidateUsers() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: userKeys.all })
}

export function useCreateUser() {
  const invalidate = useInvalidateUsers()
  return useMutation({
    mutationFn: async (values: CreateUserValues) =>
      (await http.post<UserDto>('/users', values)).data,
    meta: { successMessage: 'Utilizador criado.', skipGlobalErrorToast: true },
    onSuccess: invalidate,
  })
}

export function useUpdateUser(id: number) {
  const invalidate = useInvalidateUsers()
  return useMutation({
    mutationFn: async (values: UpdateUserValues) =>
      (await http.put<UserDto>(`/users/${String(id)}`, values)).data,
    meta: { successMessage: 'Utilizador atualizado.', skipGlobalErrorToast: true },
    onSuccess: invalidate,
  })
}

export function useResetPassword(id: number) {
  return useMutation({
    mutationFn: async (newPassword: string) => {
      await http.post(`/users/${String(id)}/reset-password`, { newPassword })
    },
    meta: { successMessage: 'Senha redefinida.', skipGlobalErrorToast: true },
  })
}

export function useSetUserActive() {
  const invalidate = useInvalidateUsers()
  return useMutation({
    mutationFn: async ({ id, active }: { id: number; active: boolean }) => {
      await http.post(`/users/${String(id)}/${active ? 'activate' : 'deactivate'}`)
    },
    onSuccess: invalidate,
  })
}
