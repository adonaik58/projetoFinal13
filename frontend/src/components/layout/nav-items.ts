import {
  CarFront,
  LayoutDashboard,
  Receipt,
  ScrollText,
  Settings,
  Ticket,
  Users,
  type LucideIcon,
} from 'lucide-react'

import type { Permission } from '@/features/auth/permissions'

export interface NavItem {
  title: string
  to:
    | '/'
    | '/spaces'
    | '/tickets'
    | '/tariff'
    | '/management/users'
    | '/management/settings'
    | '/management/audit'
  icon: LucideIcon
  permission: Permission
}

export interface NavGroup {
  label: string
  items: NavItem[]
}

export const NAV_GROUPS: NavGroup[] = [
  {
    label: 'Operação',
    items: [
      { title: 'Dashboard', to: '/', icon: LayoutDashboard, permission: 'dashboard.view' },
      { title: 'Espaços', to: '/spaces', icon: CarFront, permission: 'spaces.view' },
      { title: 'Tickets', to: '/tickets', icon: Ticket, permission: 'tickets.view' },
      { title: 'Tarifário', to: '/tariff', icon: Receipt, permission: 'settings.view' },
    ],
  },
  {
    label: 'Gestão',
    items: [
      { title: 'Utilizadores', to: '/management/users', icon: Users, permission: 'users.manage' },
      {
        title: 'Configuração',
        to: '/management/settings',
        icon: Settings,
        permission: 'settings.manage',
      },
      { title: 'Auditoria', to: '/management/audit', icon: ScrollText, permission: 'audit.view' },
    ],
  },
]
