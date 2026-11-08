import type { ColumnDef } from '@tanstack/react-table'
import { KeyRound, MoreHorizontal, Pencil, UserCheck, UserX } from 'lucide-react'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { ROLE_LABELS } from '@/features/auth/permissions'
import { formatDateTime } from '@/lib/format'

import type { UserDto } from '../types'

export interface UserActions {
  currentUserId: number | undefined
  onEdit: (user: UserDto) => void
  onResetPassword: (user: UserDto) => void
  onToggleActive: (user: UserDto) => void
}

export function userColumns(actions: UserActions): ColumnDef<UserDto>[] {
  return [
    {
      id: 'online',
      header: () => <span className="sr-only">Sessão</span>,
      cell: ({ row }) => (
        <span className="inline-flex items-center gap-2">
          <span
            className={`inline-block size-2.5 rounded-full ${row.original.isOnline ? 'bg-emerald-500' : 'bg-muted-foreground/40'}`}
            aria-hidden="true"
          />
          <span className="sr-only">
            {row.original.isOnline ? 'Com sessão ativa' : 'Sem sessão ativa'}
          </span>
        </span>
      ),
    },
    {
      id: 'fullName',
      header: 'Nome completo',
      cell: ({ row }) => (
        <div>
          <div className="font-medium">
            {row.original.fullName}
            {row.original.id === actions.currentUserId && (
              <span className="ml-2 text-xs text-muted-foreground">(você)</span>
            )}
          </div>
          <div className="text-xs text-muted-foreground">@{row.original.username}</div>
        </div>
      ),
    },
    {
      id: 'role',
      header: 'Perfil',
      cell: ({ row }) => (
        <Badge variant={row.original.role === 'Admin' ? 'default' : 'secondary'}>
          {ROLE_LABELS[row.original.role]}
        </Badge>
      ),
    },
    {
      id: 'status',
      header: 'Estado',
      cell: ({ row }) =>
        row.original.isActive ? (
          <Badge variant="outline">Ativo</Badge>
        ) : (
          <Badge variant="destructive">Desativado</Badge>
        ),
    },
    {
      id: 'lastLoginAt',
      header: 'Último acesso',
      cell: ({ row }) => (
        <span className="whitespace-nowrap">{formatDateTime(row.original.lastLoginAt)}</span>
      ),
    },
    {
      id: 'actions',
      header: () => <span className="sr-only">Ações</span>,
      cell: ({ row }) => {
        const user = row.original
        const isSelf = user.id === actions.currentUserId
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon-sm" aria-label={`Ações para ${user.fullName}`}>
                <MoreHorizontal aria-hidden="true" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onSelect={() => {
                  actions.onEdit(user)
                }}
              >
                <Pencil aria-hidden="true" /> Editar
              </DropdownMenuItem>
              <DropdownMenuItem
                onSelect={() => {
                  actions.onResetPassword(user)
                }}
              >
                <KeyRound aria-hidden="true" /> Redefinir senha
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                disabled={isSelf}
                variant={user.isActive ? 'destructive' : 'default'}
                onSelect={() => {
                  actions.onToggleActive(user)
                }}
              >
                {user.isActive ? (
                  <>
                    <UserX aria-hidden="true" /> Desativar
                  </>
                ) : (
                  <>
                    <UserCheck aria-hidden="true" /> Reativar
                  </>
                )}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )
      },
    },
  ]
}
