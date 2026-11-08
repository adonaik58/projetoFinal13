import { PageHeader } from '@/components/common/page-header'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

import { ChangePasswordForm } from '../components/change-password-form'
import { ROLE_LABELS } from '../permissions'
import { useAuth } from '../use-auth'

export function ProfilePage() {
  const { user } = useAuth()
  if (!user) return null

  return (
    <div className="space-y-6">
      <PageHeader title="Perfil" description="Os seus dados de acesso." />
      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{user.fullName}</CardTitle>
            <CardDescription>
              @{user.username} · {ROLE_LABELS[user.role]}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              Permissões: {user.permissions.length > 0 ? user.permissions.join(', ') : '—'}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Alterar senha</CardTitle>
            <CardDescription>
              As outras sessões abertas com esta conta serão terminadas.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ChangePasswordForm />
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
