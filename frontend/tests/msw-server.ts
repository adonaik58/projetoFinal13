import { setupServer } from 'msw/node'

/** Shared MSW server; each test registers the handlers it needs with `server.use(...)`. */
export const server = setupServer()

export const api = (path: string) => `*/api/v1${path}`
