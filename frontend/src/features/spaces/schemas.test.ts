import { describe, expect, it } from 'vitest'

import { bulkSpacesSchema, parseSections, spaceCodeSchema } from './schemas'

describe('spaceCodeSchema', () => {
  it('normalises and validates codes', () => {
    expect(spaceCodeSchema.parse(' a1 ')).toBe('A1')
    expect(spaceCodeSchema.parse('AB1234')).toBe('AB1234')
    expect(spaceCodeSchema.safeParse('1A').success).toBe(false)
    expect(spaceCodeSchema.safeParse('ABCD1').success).toBe(false)
  })
})

describe('parseSections', () => {
  it('expands ranges and de-duplicates', () => {
    expect(parseSections('a-c, AA; b')).toEqual(['A', 'B', 'C', 'AA'])
  })

  it('rejects invalid input', () => {
    expect(parseSections('')).toBeNull()
    expect(parseSections('C-A')).toBeNull()
    expect(parseSections('A1')).toBeNull()
  })
})

describe('bulkSpacesSchema', () => {
  it('requires toNumber >= fromNumber', () => {
    const result = bulkSpacesSchema.safeParse({ sections: 'A', fromNumber: 10, toNumber: 1 })
    expect(result.error?.issues[0]?.path).toEqual(['toNumber'])
  })
})
