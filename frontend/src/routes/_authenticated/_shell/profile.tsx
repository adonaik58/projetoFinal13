import { createFileRoute } from '@tanstack/react-router'

import { ProfilePage } from '@/features/auth/pages/profile-page'

export const Route = createFileRoute('/_authenticated/_shell/profile')({
  staticData: { title: 'Perfil' },
  component: ProfilePage,
})
