import { createFileRoute } from '@tanstack/react-router'

import { requirePermission } from '@/features/auth/guards'
import { UsersPage } from '@/features/users/pages/users-page'
import { usersSearchSchema } from '@/features/users/search'

export const Route = createFileRoute('/_authenticated/_shell/management/users')({
  validateSearch: usersSearchSchema,
  beforeLoad: requirePermission('users.manage'),
  staticData: { title: 'Utilizadores', section: 'Gestão' },
  component: UsersPage,
})
