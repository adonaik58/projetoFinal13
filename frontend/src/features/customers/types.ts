export interface CustomerLookup {
  id: number
  fullName: string
  nationalId: string
  birthDate: string
  lastVehicle: {
    plate: string
    brandId: number
    modelId: number | null
    color: string
  } | null
}
