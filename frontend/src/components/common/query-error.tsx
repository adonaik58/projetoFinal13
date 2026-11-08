import { AlertTriangle, RotateCcw } from 'lucide-react'

import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { toApiError } from '@/lib/api-error'

export function QueryError({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  return (
    <Alert variant="destructive">
      <AlertTriangle aria-hidden="true" />
      <AlertTitle>Erro ao carregar os dados</AlertTitle>
      <AlertDescription className="flex flex-wrap items-center gap-3">
        <span>{toApiError(error).userMessage}</span>
        {onRetry && (
          <Button size="sm" variant="outline" onClick={onRetry}>
            <RotateCcw aria-hidden="true" /> Tentar novamente
          </Button>
        )}
      </AlertDescription>
    </Alert>
  )
}
