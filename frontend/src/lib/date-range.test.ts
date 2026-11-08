import { describe, expect, it } from 'vitest'

import { customRange, presetRange } from './date-range'

// Wednesday, 2026-11-11 15:30 local time.
const now = new Date(2026, 10, 11, 15, 30)

const local = (y: number, m: number, d: number, h = 0, min = 0, s = 0, ms = 0) =>
  new Date(y, m, d, h, min, s, ms).toISOString()

describe('presetRange', () => {
  it('today covers the whole local day', () => {
    expect(presetRange('today', now)).toEqual({
      from: local(2026, 10, 11),
      to: local(2026, 10, 11, 23, 59, 59, 999),
    })
  })

  it('yesterday covers only the previous day', () => {
    expect(presetRange('yesterday', now)).toEqual({
      from: local(2026, 10, 10),
      to: local(2026, 10, 10, 23, 59, 59, 999),
    })
  })

  it('3 days and 1 week include today', () => {
    expect(presetRange('3d', now).from).toBe(local(2026, 10, 9))
    expect(presetRange('1w', now).from).toBe(local(2026, 10, 5))
    expect(presetRange('1w', now).to).toBe(local(2026, 10, 11, 23, 59, 59, 999))
  })

  it('1 month and 1 year go back by calendar units', () => {
    expect(presetRange('1m', now).from).toBe(local(2026, 9, 11))
    expect(presetRange('1y', now).from).toBe(local(2025, 10, 11))
  })
})

describe('customRange', () => {
  it('converts inclusive local dates to UTC instants', () => {
    expect(customRange('2026-01-01', '2026-01-31')).toEqual({
      from: local(2026, 0, 1),
      to: local(2026, 0, 31, 23, 59, 59, 999),
    })
  })

  it('supports open-ended ranges and ignores invalid dates', () => {
    expect(customRange('2026-01-01')).toEqual({ from: local(2026, 0, 1) })
    expect(customRange(undefined, 'bad')).toEqual({})
  })
})
