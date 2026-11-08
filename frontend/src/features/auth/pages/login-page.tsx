import { getRouteApi, useNavigate } from '@tanstack/react-router'
import { ParkingMeter } from 'lucide-react'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

import { LoginForm } from '../components/login-form'
import { safeRedirectTarget } from '../guards'
import { homePathFor } from '../permissions'

const routeApi = getRouteApi('/login')

export function LoginPage() {
  const navigate = useNavigate()
  const { redirect: redirectTo } = routeApi.useSearch()

  return (
    <main className="flex min-h-svh items-center justify-center bg-muted/40 p-4">
      <div className="w-full max-w-sm space-y-6">
        <div className="flex flex-col items-center gap-2 text-center">
          <span className="flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <ParkingMeter className="size-6" aria-hidden="true" />
          </span>
          <h1 className="text-2xl font-semibold tracking-tight">Xyami.Park</h1>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Iniciar sessão</CardTitle>
            <CardDescription>Introduza as suas credenciais para continuar.</CardDescription>
          </CardHeader>
          <CardContent>
            <LoginForm
              onSuccess={async (user) => {
                const target = safeRedirectTarget(redirectTo)
                await (target
                  ? navigate({ href: target, replace: true })
                  : navigate({ to: homePathFor(user), replace: true }))
              }}
            />
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
