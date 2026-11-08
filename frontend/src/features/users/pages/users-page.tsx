import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'
import { Search, UserPlus } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { toast } from 'sonner'

import { ConfirmDialog } from '@/components/common/confirm-dialog'
import { DataTable } from '@/components/common/data-table'
import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { ROLE_LABELS } from '@/features/auth/permissions'
import type { Role } from '@/features/auth/types'
import { useAuth } from '@/features/auth/use-auth'
import { usersQueryOptions, useSetUserActive } from '@/features/users/api'
import { ResetPasswordDialog } from '@/features/users/components/reset-password-dialog'
import { userColumns } from '@/features/users/components/user-columns'
import { UserFormDialog, type UserDialogState } from '@/features/users/components/user-form-dialog'
import type { UserDto } from '@/features/users/types'
import { useDebouncedValue } from '@/hooks/use-debounced-value'

const routeApi = getRouteApi('/_authenticated/_shell/management/users')

const ALL = 'all'

export function UsersPage() {
  const search = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const { user: currentUser } = useAuth()
  const [quickSearch, setQuickSearch] = useState(search.search ?? '')
  const debouncedSearch = useDebouncedValue(quickSearch)
  const [dialog, setDialog] = useState<UserDialogState>(null)
  const [resetFor, setResetFor] = useState<UserDto | null>(null)
  const [toggleFor, setToggleFor] = useState<UserDto | null>(null)
  const setActive = useSetUserActive()

  const params = {
    page: search.page ?? 1,
    pageSize: search.pageSize ?? 20,
    search: search.search,
    role: search.role,
    isActive: search.status === undefined ? undefined : search.status === 'active',
  }
  const users = useQuery(usersQueryOptions(params))

  useEffect(() => {
    const next = debouncedSearch.trim() || undefined
    if (next !== search.search) {
      void navigate({
        search: (prev) => ({ ...prev, search: next, page: undefined }),
        replace: true,
      })
    }
  }, [debouncedSearch, search.search, navigate])

  const columns = useMemo(
    () =>
      userColumns({
        currentUserId: currentUser?.id,
        onEdit: (user) => {
          setDialog({ mode: 'edit', user })
        },
        onResetPassword: setResetFor,
        onToggleActive: setToggleFor,
      }),
    [currentUser?.id],
  )

  return (
    <div className="space-y-6">
      <PageHeader
        title="Utilizadores"
        description="Perfis de acesso de operadores e administradores."
        actions={
          <Button
            onClick={() => {
              setDialog({ mode: 'create' })
            }}
          >
            <UserPlus aria-hidden="true" /> Adicionar utilizador
          </Button>
        }
      />

      <div className="grid gap-3 sm:grid-cols-3" role="search">
        <div className="grid gap-1.5">
          <Label htmlFor="users-search">Pesquisar</Label>
          <div className="relative">
            <Search
              className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <Input
              id="users-search"
              className="pl-8"
              placeholder="Nome ou utilizador"
              value={quickSearch}
              onChange={(event) => {
                setQuickSearch(event.target.value)
              }}
            />
          </div>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="users-role">Perfil</Label>
          <Select
            value={search.role ?? ALL}
            onValueChange={(value) => {
              void navigate({
                search: (prev) => ({
                  ...prev,
                  role: value === ALL ? undefined : (value as Role),
                  page: undefined,
                }),
                replace: true,
              })
            }}
          >
            <SelectTrigger id="users-role" className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL}>Todos</SelectItem>
              <SelectItem value="Admin">{ROLE_LABELS.Admin}</SelectItem>
              <SelectItem value="Operator">{ROLE_LABELS.Operator}</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="users-status">Estado</Label>
          <Select
            value={search.status ?? ALL}
            onValueChange={(value) => {
              void navigate({
                search: (prev) => ({
                  ...prev,
                  status: value === ALL ? undefined : (value as 'active' | 'inactive'),
                  page: undefined,
                }),
                replace: true,
              })
            }}
          >
            <SelectTrigger id="users-status" className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL}>Todos</SelectItem>
              <SelectItem value="active">Ativos</SelectItem>
              <SelectItem value="inactive">Desativados</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {users.isError && !users.data ? (
        <QueryError error={users.error} onRetry={() => void users.refetch()} />
      ) : (
        <DataTable
          caption="Utilizadores"
          columns={columns}
          data={users.data?.items ?? []}
          isLoading={users.isPending}
          emptyMessage="Nenhum utilizador encontrado."
          getRowId={(user) => String(user.id)}
          pagination={
            users.data ?? {
              page: params.page,
              pageSize: params.pageSize,
              totalCount: 0,
              totalPages: 0,
            }
          }
          onPageChange={(page) => {
            void navigate({ search: (prev) => ({ ...prev, page }), replace: true })
          }}
          onPageSizeChange={(pageSize) => {
            void navigate({
              search: (prev) => ({ ...prev, pageSize, page: undefined }),
              replace: true,
            })
          }}
        />
      )}

      <UserFormDialog
        state={dialog}
        onOpenChange={(open) => {
          if (!open) setDialog(null)
        }}
      />
      <ResetPasswordDialog
        user={resetFor}
        onOpenChange={(open) => {
          if (!open) setResetFor(null)
        }}
      />
      <ConfirmDialog
        open={toggleFor !== null}
        onOpenChange={(open) => {
          if (!open) setToggleFor(null)
        }}
        title={
          toggleFor?.isActive
            ? `Desativar ${toggleFor.fullName}?`
            : `Reativar ${toggleFor?.fullName ?? ''}?`
        }
        description={
          toggleFor?.isActive
            ? 'O utilizador deixa de conseguir iniciar sessão e as sessões abertas são terminadas. O histórico de tickets é mantido.'
            : 'O utilizador volta a poder iniciar sessão.'
        }
        confirmLabel={toggleFor?.isActive ? 'Desativar' : 'Reativar'}
        destructive={toggleFor?.isActive ?? false}
        pending={setActive.isPending}
        onConfirm={() => {
          if (!toggleFor) return
          const active = !toggleFor.isActive
          setActive.mutate(
            { id: toggleFor.id, active },
            {
              onSuccess: () => {
                toast.success(active ? 'Utilizador reativado.' : 'Utilizador desativado.')
              },
              onSettled: () => {
                setToggleFor(null)
              },
            },
          )
        }}
      />
    </div>
  )
}
