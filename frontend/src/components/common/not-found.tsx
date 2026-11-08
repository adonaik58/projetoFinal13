import { Link } from '@tanstack/react-router'
import { MapPinOff } from 'lucide-react'

import { Button } from '@/components/ui/button'

export function NotFound() {
  return (
    <div className="mx-auto flex max-w-lg flex-col items-center gap-4 py-16 text-center">
      <MapPinOff className="size-10 text-muted-foreground" aria-hidden="true" />
      <div className="space-y-1">
        <h1 className="text-lg font-semibold">Página não encontrada</h1>
        <p className="text-sm text-muted-foreground">
          O endereço que procura não existe ou foi movido.
        </p>
      </div>
      <Button asChild>
        <Link to="/">Voltar ao início</Link>
      </Button>
    </div>
  )
}
