import { describe, expect, it } from 'vitest'

import { normalizeSpaces } from '../../tests/render'
import {
  ageFrom,
  formatCurrency,
  formatDurationMinutes,
  formatElapsedSeconds,
  initialsOf,
  parseDateOnly,
  toDateOnly,
} from './format'

describe('formatCurrency', () => {
  it('formats kwanzas with two decimals', () => {
    expect(normalizeSpaces(formatCurrency(1250.5))).toBe('1 250,50 Kz')
    expect(normalizeSpaces(formatCurrency(0))).toBe('0,00 Kz')
    expect(normalizeSpaces(formatCurrency(null))).toBe('0,00 Kz')
  })
})

describe('formatDurationMinutes (legacy style)', () => {
  it.each([
    [0, '0min'],
    [45, '45min'],
    [60, '1h'],
    [125, '2h e 5min'],
    [24 * 60 + 4 * 60, '1d e 4h'],
    [30 * 24 * 60 * 2 + 3 * 24 * 60, '2m e 3d'],
    [365 * 24 * 60, '1 ano'],
    [365 * 24 * 60 + 60 * 24 * 60, '1 ano e 2 meses'],
    [2 * 365 * 24 * 60 + 30 * 24 * 60, '2 anos e 1 mês'],
    [-5, '0min'],
  ])('%i minutes → %s', (minutes, expected) => {
    expect(formatDurationMinutes(minutes)).toBe(expected)
  })
})

describe('formatElapsedSeconds', () => {
  it('shows seconds below one hour', () => {
    expect(formatElapsedSeconds(40)).toBe('40s')
    expect(formatElapsedSeconds(12 * 60 + 5)).toBe('12min e 05s')
  })

  it('delegates to minute formatting from one hour', () => {
    expect(formatElapsedSeconds(2 * 3600 + 5 * 60 + 59)).toBe('2h e 5min')
  })
})

describe('date-only helpers', () => {
  it('round-trips local calendar dates', () => {
    const date = parseDateOnly('2026-02-28')
    expect(date).not.toBeNull()
    expect(toDateOnly(date ?? new Date())).toBe('2026-02-28')
    expect(parseDateOnly('28/02/2026')).toBeNull()
  })

  it('computes full years of age', () => {
    const now = new Date(2026, 10, 7)
    expect(ageFrom('2000-11-07', now)).toBe(26)
    expect(ageFrom('2000-11-08', now)).toBe(25)
    expect(ageFrom('invalid', now)).toBeNull()
  })
})

describe('initialsOf', () => {
  it('uses the first and last names', () => {
    expect(initialsOf('Maria da Silva')).toBe('MS')
    expect(initialsOf('admin')).toBe('A')
    expect(initialsOf('   ')).toBe('?')
  })
})
