import { Wrench } from 'lucide-react'
import { toast } from 'sonner'

import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { useAuth } from '@/features/auth/use-auth'
import { openTicketPrint } from '@/features/tickets/print'

import { SPACE_STATUS_META } from '../status'
import type { SpaceDto } from '../types'
import { CheckInForm } from './check-in-form'
import { OccupancyDetails } from './occupancy-details'

export function SpaceSheet({
  space,
  now,
  onOpenChange,
}: {
  space: SpaceDto | null
  now: number
  onOpenChange: (open: boolean) => void
}) {
  const { can } = useAuth()
  const meta = space ? SPACE_STATUS_META[space.status] : null

  return (
    <Sheet open={!!space} onOpenChange={onOpenChange}>
      <SheetContent className="w-full overflow-y-auto sm:max-w-lg">
        {space && meta && (
          <>
            <SheetHeader>
              <SheetTitle>
                Espaço {space.code}
                <span className="ml-2 align-middle text-sm font-normal text-muted-foreground">
                  · {meta.label}
                </span>
              </SheetTitle>
              <SheetDescription>
                {space.status === 'Free'
                  ? 'Registe a entrada do cliente e da viatura.'
                  : space.status === 'Occupied'
                    ? 'Detalhes da ocupação atual.'
                    : 'Este espaço está fora de serviço.'}
              </SheetDescription>
            </SheetHeader>
            <div className="px-4 pb-6">
              {space.status === 'Free' &&
                (can('tickets.open') ? (
                  <CheckInForm
                    key={space.id}
                    spaceId={space.id}
                    onSuccess={(ticket) => {
                      onOpenChange(false)
                      const printed = openTicketPrint(ticket.id)
                      toast.success(`Entrada registada no espaço ${ticket.spaceCode}.`, {
                        description: `Ticket ${ticket.code}`,
                        duration: 10_000,
                        ...(printed
                          ? {}
                          : {
                              action: {
                                label: 'Imprimir',
                                onClick: () => openTicketPrint(ticket.id),
                              },
                            }),
                      })
                    }}
                  />
                ) : (
                  <p className="text-sm text-muted-foreground">
                    Não tem permissão para registar entradas.
                  </p>
                ))}
              {space.status === 'Occupied' && space.occupancy && (
                <OccupancyDetails
                  spaceCode={space.code}
                  occupancy={space.occupancy}
                  now={now}
                  onClosed={() => {
                    onOpenChange(false)
                  }}
                />
              )}
              {space.status === 'Maintenance' && (
                <div className="flex items-start gap-3 rounded-xl border bg-muted/40 p-4 text-sm">
                  <Wrench className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
                  <p>
                    O espaço está marcado como indisponível. Um administrador pode reativá-lo em
                    Gestão › Configuração › Espaços.
                  </p>
                </div>
              )}
            </div>
          </>
        )}
      </SheetContent>
    </Sheet>
  )
}
