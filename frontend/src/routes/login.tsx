import { createFileRoute, redirect } from '@tanstack/react-router'
import { z } from 'zod'

import { ensureSession, safeRedirectTarget } from '@/features/auth/guards'
import { LoginPage } from '@/features/auth/pages/login-page'
import { homePathFor } from '@/features/auth/permissions'

export const Route = createFileRoute('/login')({
  validateSearch: z.object({ redirect: z.string().optional().catch(undefined) }),
  beforeLoad: async ({ search }) => {
    const state = await ensureSession()
    if (state.status === 'authenticated') {
      const target = safeRedirectTarget(search.redirect)
      throw target ? redirect({ href: target }) : redirect({ to: homePathFor(state.user) })
    }
  },
  staticData: { title: 'Iniciar sessão' },
  component: LoginPage,
})
