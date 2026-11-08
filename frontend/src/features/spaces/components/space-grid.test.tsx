import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { normalizeSpaces, setupUser } from '../../../../tests/render'
import type { SpaceDto } from '../types'
import { SpaceGrid } from './space-grid'

const entryAt = '2026-11-07T08:00:00.000Z'
const now = Date.parse('2026-11-07T10:05:30.000Z')

const spaces: SpaceDto[] = [
  { id: 1, code: 'A1', section: 'A', number: 1, status: 'Free', occupancy: null },
  {
    id: 2,
    code: 'A2',
    section: 'A',
    number: 2,
    status: 'Occupied',
    occupancy: {
      ticketId: 10,
      ticketCode: 'K7Q2M9XP',
      entryAt,
      elapsedMinutes: 125,
      currentAmount: 650,
      customer: {
        id: 5,
        fullName: 'Ana Kambu',
        nationalId: '123456789LA123',
        birthDate: '1990-05-17',
        age: 36,
      },
      vehicle: {
        id: 8,
        plate: 'LD-12-34-AB',
        color: '#1f2937',
        brandId: 3,
        brandName: 'Toyota',
        modelId: null,
        modelName: null,
      },
    },
  },
  { id: 3, code: 'A3', section: 'A', number: 3, status: 'Maintenance', occupancy: null },
]

describe('SpaceGrid', () => {
  it('renders each space as a button whose name conveys status in text', () => {
    render(<SpaceGrid spaces={spaces} now={now} onSelect={vi.fn()} />)

    const list = screen.getByRole('list', { name: 'Espaços de estacionamento' })
    expect(within(list).getAllByRole('listitem')).toHaveLength(3)
    expect(screen.getByRole('button', { name: 'Espaço A1: Livre' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Espaço A3: Indisponível' })).toBeInTheDocument()
    expect(
      screen.getByRole('button', {
        name: 'Espaço A2: Ocupado, matrícula LD-12-34-AB, há 2h e 5min',
      }),
    ).toBeInTheDocument()
  })

  it('shows plate, live elapsed time and server amount for occupied spaces', () => {
    render(<SpaceGrid spaces={spaces} now={now} onSelect={vi.fn()} />)
    const card = screen.getByRole('button', { name: /Espaço A2/ })

    expect(within(card).getByText('LD-12-34-AB')).toBeInTheDocument()
    expect(within(card).getByText('2h e 5min')).toBeInTheDocument()
    expect(normalizeSpaces(card.textContent)).toContain('650,00 Kz')
  })

  it('reports the selected space', async () => {
    const user = setupUser()
    const onSelect = vi.fn()
    render(<SpaceGrid spaces={spaces} now={now} onSelect={onSelect} />)

    await user.click(screen.getByRole('button', { name: 'Espaço A1: Livre' }))

    expect(onSelect).toHaveBeenCalledWith(spaces[0])
  })

  it('shows an empty state', () => {
    render(<SpaceGrid spaces={[]} now={now} onSelect={vi.fn()} />)
    expect(screen.getByText('Nenhum espaço encontrado')).toBeInTheDocument()
  })
})
