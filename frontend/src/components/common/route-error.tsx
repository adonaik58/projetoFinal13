import { useQueryErrorResetBoundary } from '@tanstack/react-query'
import { type ErrorComponentProps, useRouter } from '@tanstack/react-router'
import { AlertTriangle, RotateCcw } from 'lucide-react'
import { useEffect } from 'react'

import { Button } from '@/components/ui/button'
import { toApiError } from '@/lib/api-error'

export function RouteError({ error, reset }: ErrorComponentProps) {
  const router = useRouter()
  const queryErrorResetBoundary = useQueryErrorResetBoundary()
  const apiError = toApiError(error)

  useEffect(() => {
    queryErrorResetBoundary.reset()
  }, [queryErrorResetBoundary])

  return (
    <div
      role="alert"
      className="mx-auto flex max-w-lg flex-col items-center gap-4 py-16 text-center"
    >
      <AlertTriangle className="size-10 text-destructive" aria-hidden="true" />
      <div className="space-y-1">
        <h2 className="text-lg font-semibold">Não foi possível apresentar esta página</h2>
        <p className="text-sm text-muted-foreground">{apiError.userMessage}</p>
        {apiError.correlationId && (
          <p className="text-xs text-muted-foreground">
            Referência: <code>{apiError.correlationId}</code>
          </p>
        )}
      </div>
      <Button
        onClick={() => {
          reset()
          void router.invalidate()
        }}
      >
        <RotateCcw aria-hidden="true" /> Tentar novamente
      </Button>
    </div>
  )
}
