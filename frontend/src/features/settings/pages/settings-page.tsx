import { useQuery } from '@tanstack/react-query'
import { getRouteApi } from '@tanstack/react-router'

import { PageHeader } from '@/components/common/page-header'
import { QueryError } from '@/components/common/query-error'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { settingsQueryOptions } from '@/features/settings/api'
import type { SettingsTab } from '@/features/settings/search'
import { CatalogManager } from '@/features/settings/components/catalog-manager'
import { SpacesManager } from '@/features/settings/components/spaces-manager'
import { TariffSettingsForm } from '@/features/settings/components/tariff-settings-form'

const routeApi = getRouteApi('/_authenticated/_shell/management/settings')

export function SettingsPage() {
  const { tab = 'tariff' } = routeApi.useSearch()
  const navigate = routeApi.useNavigate()
  const settings = useQuery(settingsQueryOptions())

  return (
    <div className="space-y-6">
      <PageHeader title="Configuração" description="Tarifa, espaços e catálogo de viaturas." />
      <Tabs
        value={tab}
        onValueChange={(value) => {
          void navigate({ search: { tab: value as SettingsTab }, replace: true })
        }}
      >
        <TabsList>
          <TabsTrigger value="tariff">Tarifa</TabsTrigger>
          <TabsTrigger value="spaces">Espaços</TabsTrigger>
          <TabsTrigger value="catalog">Catálogo</TabsTrigger>
        </TabsList>
        <TabsContent value="tariff" className="pt-4">
          <Card>
            <CardHeader>
              <CardTitle>Configurar sistema</CardTitle>
              <CardDescription>
                A tarifa é registada em cada ticket no momento da entrada; alterações aplicam-se a
                novas entradas.
              </CardDescription>
            </CardHeader>
            <CardContent>
              {settings.isError ? (
                <QueryError error={settings.error} onRetry={() => void settings.refetch()} />
              ) : !settings.data ? (
                <Skeleton className="h-32 w-full" />
              ) : (
                <TariffSettingsForm settings={settings.data} />
              )}
            </CardContent>
          </Card>
        </TabsContent>
        <TabsContent value="spaces" className="pt-4">
          <SpacesManager />
        </TabsContent>
        <TabsContent value="catalog" className="pt-4">
          <CatalogManager />
        </TabsContent>
      </Tabs>
    </div>
  )
}
