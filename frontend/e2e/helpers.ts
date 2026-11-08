import { expect, type Page } from '@playwright/test'

export function adminCredentials() {
  const username = process.env.E2E_ADMIN_USERNAME
  const password = process.env.E2E_ADMIN_PASSWORD
  if (!username || !password) {
    throw new Error('Set E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD to run the end-to-end tests.')
  }
  return { username, password }
}

export async function login(page: Page, username: string, password: string) {
  await page.goto('/login')
  await page.getByLabel('Nome de utilizador').fill(username)
  await page.getByLabel('Senha', { exact: true }).fill(password)
  await page.getByRole('button', { name: 'Entrar' }).click()
  await expect(page).not.toHaveURL(/\/login/)
}

export async function loginAsAdmin(page: Page) {
  const { username, password } = adminCredentials()
  await login(page, username, password)
}

/** Short unique suffix so repeated runs never collide on unique columns. */
export function uniqueSuffix(): string {
  return `${Date.now().toString(36)}${Math.floor(Math.random() * 1296).toString(36)}`.toUpperCase()
}

const digits = (count: number) =>
  Array.from({ length: count }, () => Math.floor(Math.random() * 10)).join('')

/** A syntactically valid Angolan BI (9 digits, 2 letters, 3 digits). */
export function randomNationalId(): string {
  return `${digits(9)}LA${digits(3)}`
}

export function randomPlate(): string {
  return `LD-${digits(2)}-${digits(2)}-E${String.fromCharCode(65 + Math.floor(Math.random() * 26))}`
}

/** Picks an option of a Radix/shadcn Select identified by its label. */
export async function selectOption(page: Page, label: string, option: string | RegExp) {
  await page.getByRole('combobox', { name: label }).click()
  await page.getByRole('option', { name: option }).click()
}
