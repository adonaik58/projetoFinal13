/** `Customer`: returning customer of this park; `Registry`: new customer pre-filled from the BI lookup. */
export type CustomerLookupSource = 'Customer' | 'Registry'

export interface CustomerLookup {
  id: number | null
  fullName: string
  nationalId: string
  birthDate: string | null
  phone: string | null
  lastVehicle: {
    plate: string
    brandId: number
    modelId: number | null
    color: string
  } | null
  source: CustomerLookupSource
}
