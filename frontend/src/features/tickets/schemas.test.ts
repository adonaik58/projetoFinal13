import { describe, expect, it } from 'vitest'

import { checkInSchema, isValidNationalId, nationalIdSchema, plateSchema } from './schemas'

const validInput = {
  spaceId: 4,
  customer: {
    nationalId: '123456789la123',
    fullName: '  Ana Kambu ',
    birthDate: '1990-05-17',
    phone: ' 923 456-789 ',
  },
  vehicle: { plate: 'ld-12-34-ab', brandId: 3, modelId: null, color: '#1f2937' },
}

describe('nationalIdSchema (BI)', () => {
  it('accepts the Angolan format and normalises to upper case', () => {
    expect(nationalIdSchema.parse(' 123456789la123 ')).toBe('123456789LA123')
    expect(isValidNationalId('123456789LA123')).toBe(true)
  })

  it.each([
    ['12345678LA123', '14 caracteres'],
    ['1234567890A123', 'inválido'],
    ['123456789LA12X', 'inválido'],
  ])('rejects %s', (value, message) => {
    const result = nationalIdSchema.safeParse(value)
    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.message).toContain(message)
  })
})

describe('plateSchema', () => {
  it('normalises and validates plates', () => {
    expect(plateSchema.parse(' ld-12-34-ab ')).toBe('LD-12-34-AB')
    expect(plateSchema.safeParse('LD_12').success).toBe(false)
    expect(plateSchema.safeParse('A').success).toBe(false)
  })
})

describe('checkInSchema', () => {
  it('produces the API payload shape', () => {
    expect(checkInSchema.parse(validInput)).toEqual({
      spaceId: 4,
      customer: {
        nationalId: '123456789LA123',
        fullName: 'Ana Kambu',
        birthDate: '1990-05-17',
        phone: '923456789',
      },
      vehicle: { plate: 'LD-12-34-AB', brandId: 3, modelId: null, color: '#1f2937' },
    })
  })

  it('requires a brand, a past birth date and a hex colour', () => {
    const result = checkInSchema.safeParse({
      ...validInput,
      customer: { ...validInput.customer, birthDate: '2999-01-01' },
      vehicle: { ...validInput.vehicle, brandId: 0, color: 'red' },
    })
    expect(result.success).toBe(false)
    const paths = result.error?.issues.map((issue) => issue.path.join('.'))
    expect(paths).toEqual(
      expect.arrayContaining(['customer.birthDate', 'vehicle.brandId', 'vehicle.color']),
    )
  })
})
