import { CarFront } from 'lucide-react'

import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'

import type { SpaceDto } from '../types'
import { SpaceCard } from './space-card'

export function SpaceGrid({
  spaces,
  now,
  onSelect,
}: {
  spaces: SpaceDto[]
  now: number
  onSelect: (space: SpaceDto) => void
}) {
  if (spaces.length === 0) {
    return (
      <Empty className="border">
        <EmptyHeader>
          <EmptyMedia variant="icon">
            <CarFront aria-hidden="true" />
          </EmptyMedia>
          <EmptyTitle>Nenhum espaço encontrado</EmptyTitle>
          <EmptyDescription>
            Ajuste os filtros ou crie espaços em Gestão › Configuração.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    )
  }

  return (
    <ul
      aria-label="Espaços de estacionamento"
      className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 2xl:grid-cols-8"
    >
      {spaces.map((space) => (
        <li key={space.id}>
          <SpaceCard space={space} now={now} onSelect={onSelect} />
        </li>
      ))}
    </ul>
  )
}
