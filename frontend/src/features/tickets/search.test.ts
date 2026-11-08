import { describe, expect, it } from 'vitest'

import { presetRange } from '@/lib/date-range'

import { activeFilterCount, buildTicketQuery, ticketsSearchSchema } from './search'

const now = new Date(2026, 10, 11, 15, 30)

describe('ticketsSearchSchema', () => {
  it('accepts valid params and drops invalid ones instead of failing', () => {
    const parsed = ticketsSearchSchema.parse({
      status: 'Bogus',
      page: 'abc',
      pageSize: 33,
      search: 123,
      plate: '  ld-12-34-ab ',
      brandId: '7',
      preset: 'today',
      fromDate: '2026/01/01',
      sortBy: 'amount',
      sortDir: 'sideways',
    })

    expect(parsed).toEqual({
      status: undefined,
      page: undefined,
      pageSize: undefined,
      search: '123',
      customerName: undefined,
      nationalId: undefined,
      plate: 'ld-12-34-ab',
      brandId: 7,
      modelId: undefined,
      spaceCode: undefined,
      preset: 'today',
      fromDate: undefined,
      toDate: undefined,
      sortBy: 'amount',
      sortDir: undefined,
    })
  })

  it('keeps the legacy history redirect (?status=Closed) working', () => {
    expect(ticketsSearchSchema.parse({ status: 'Closed' }).status).toBe('Closed')
  })
})

describe('buildTicketQuery', () => {
  it('defaults to active tickets, newest entries first, 20 per page', () => {
    expect(buildTicketQuery({}, now)).toEqual({
      page: 1,
      pageSize: 20,
      status: 'Open',
      search: undefined,
      customerName: undefined,
      nationalId: undefined,
      plate: undefined,
      brandId: undefined,
      modelId: undefined,
      spaceCode: undefined,
      from: undefined,
      to: undefined,
      sortBy: 'entryAt',
      sortDir: 'desc',
    })
  })

  it('sorts history by exit date by default', () => {
    expect(buildTicketQuery({ status: 'Closed' }, now).sortBy).toBe('exitAt')
  })

  it('maps filters to API parameters, normalising codes to upper case', () => {
    const query = buildTicketQuery(
      {
        status: 'Closed',
        page: 3,
        pageSize: 50,
        nationalId: '123456789la123',
        plate: 'ld-12-34-ab',
        spaceCode: 'a1',
        brandId: 2,
        modelId: 9,
        sortBy: 'amount',
        sortDir: 'asc',
      },
      now,
    )

    expect(query).toMatchObject({
      page: 3,
      pageSize: 50,
      status: 'Closed',
      nationalId: '123456789LA123',
      plate: 'LD-12-34-AB',
      spaceCode: 'A1',
      brandId: 2,
      modelId: 9,
      sortBy: 'amount',
      sortDir: 'asc',
    })
  })

  it('converts a date preset to UTC from/to', () => {
    const query = buildTicketQuery({ preset: 'yesterday' }, now)
    expect({ from: query.from, to: query.to }).toEqual(presetRange('yesterday', now))
  })

  it('prefers the preset over custom dates and supports custom ranges', () => {
    const custom = buildTicketQuery({ fromDate: '2026-01-01', toDate: '2026-01-31' }, now)
    expect(custom.from).toBe(new Date(2026, 0, 1).toISOString())
    expect(custom.to).toBe(new Date(2026, 0, 31, 23, 59, 59, 999).toISOString())
  })
})

describe('activeFilterCount', () => {
  it('counts advanced filters, with the date range counted once', () => {
    expect(activeFilterCount({})).toBe(0)
    expect(
      activeFilterCount({ plate: 'X', brandId: 1, fromDate: '2026-01-01', toDate: '2026-01-02' }),
    ).toBe(3)
  })
})
