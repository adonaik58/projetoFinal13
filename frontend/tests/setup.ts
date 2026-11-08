import '@testing-library/jest-dom/vitest'

import { cleanup } from '@testing-library/react'
import { afterAll, afterEach, beforeAll, vi } from 'vitest'

import { server } from './msw-server'

// jsdom lacks a few browser APIs used by Radix UI and our hooks.
if (typeof window.matchMedia !== 'function') {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn((query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  })
}

class ResizeObserverStub {
  observe() {
    // no-op in tests
  }
  unobserve() {
    // no-op in tests
  }
  disconnect() {
    // no-op in tests
  }
}

function stub(target: object, name: string, value: unknown) {
  Object.defineProperty(target, name, { value, configurable: true, writable: true })
}

stub(globalThis, 'ResizeObserver', ResizeObserverStub)
stub(Element.prototype, 'scrollIntoView', vi.fn())
stub(
  Element.prototype,
  'hasPointerCapture',
  vi.fn(() => false),
)
stub(Element.prototype, 'releasePointerCapture', vi.fn())

beforeAll(() => {
  server.listen({ onUnhandledFrame: 'error' })
})

afterEach(() => {
  cleanup()
  server.resetHandlers()
})

afterAll(() => {
  server.close()
})
