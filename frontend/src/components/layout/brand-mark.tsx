import { ParkingMeter } from 'lucide-react'

export function BrandMark() {
  return (
    <>
      <span className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
        <ParkingMeter className="size-4" aria-hidden="true" />
      </span>
      <span className="grid flex-1 text-left leading-tight">
        <span className="truncate font-semibold">Xyami.Park</span>
        <span className="truncate text-xs text-muted-foreground">Gestão de estacionamento</span>
      </span>
    </>
  )
}
