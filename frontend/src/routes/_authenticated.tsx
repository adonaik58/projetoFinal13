import { createFileRoute, Outlet } from '@tanstack/react-router'

import { requireAuthenticated } from '@/features/auth/guards'

/** Pathless guard: every child route requires a valid session. */
export const Route = createFileRoute('/_authenticated')({
  beforeLoad: ({ location }) => requireAuthenticated(location),
  component: Outlet,
})
