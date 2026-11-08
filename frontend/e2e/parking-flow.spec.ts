import { expect, test, type Page } from '@playwright/test'

import { loginAsAdmin, randomNationalId, randomPlate, selectOption, uniqueSuffix } from './helpers'

test.describe.configure({ mode: 'serial' })

/** Creates a dedicated space for this run, or falls back to any free space if capacity is full. */
async function prepareFreeSpace(page: Page): Promise<string> {
  const code = `E${String(1000 + Math.floor(Math.random() * 9000))}`
  await page.goto('/management/settings?tab=spaces')
  await page.getByLabel('Código').fill(code)
  await page.getByRole('button', { name: 'Criar espaço' }).click()

  const created = page.getByText('Espaço criado.')
  const failed = page.locator('#space-code-error')
  await expect(created.or(failed)).toBeVisible()
  if (await created.isVisible()) return code

  await page.goto('/spaces?status=Free')
  const firstFree = page.getByRole('button', { name: /^Espaço [A-Z]{1,3}\d{1,4}: Livre$/ }).first()
  await expect(firstFree).toBeVisible()
  const name = (await firstFree.getAttribute('aria-label')) ?? ''
  return /Espaço (\S+):/.exec(name)?.[1] ?? ''
}

test('check-in e check-out completos com impressão do ticket', async ({ page, context }) => {
  await loginAsAdmin(page)

  const brand = `E2E Marca ${uniqueSuffix()}`
  await page.goto('/management/settings?tab=catalog')
  await page.getByLabel('Nova marca').fill(brand)
  await page.getByRole('button', { name: 'Adicionar' }).first().click()
  await expect(page.getByText('Marca adicionada.')).toBeVisible()

  const code = await prepareFreeSpace(page)
  const plate = randomPlate()
  const nationalId = randomNationalId()

  await page.goto(`/spaces?search=${code}`)
  await page.getByRole('button', { name: `Espaço ${code}: Livre` }).click()

  const sheet = page.getByRole('dialog')
  await expect(sheet.getByRole('heading', { name: new RegExp(`Espaço ${code}`) })).toBeVisible()
  await sheet.getByLabel('Bilhete de Identidade').fill(nationalId)
  await sheet.getByLabel('Nome completo').fill('Cliente Teste E2E')
  await sheet.getByLabel('Data de nascimento').fill('1990-05-17')
  await sheet.getByLabel('Matrícula').fill(plate)
  await selectOption(page, 'Marca', brand)

  const printPage = context.waitForEvent('page', { timeout: 5_000 }).catch(() => null)
  await sheet.getByRole('button', { name: 'Registar entrada' }).click()
  await expect(page.getByText(`Entrada registada no espaço ${code}.`)).toBeVisible()

  const popup = await printPage
  if (popup) {
    await expect(popup.getByRole('article')).toContainText(code)
    await expect(popup.getByRole('article')).toContainText(plate)
    await popup.close()
  }

  const occupied = page.getByRole('button', { name: new RegExp(`^Espaço ${code}: Ocupado`) })
  await expect(occupied).toBeVisible()
  await expect(occupied).toContainText(plate)

  await occupied.click()
  await expect(page.getByRole('dialog').getByText('Cliente Teste E2E')).toBeVisible()
  await page.getByRole('button', { name: 'Fechar ticket' }).click()
  await page.getByRole('alertdialog').getByRole('button', { name: 'Fechar ticket' }).click()
  await expect(page.getByText(/Ticket \S+ fechado\./)).toBeVisible()
  await expect(page.getByRole('button', { name: `Espaço ${code}: Livre` })).toBeVisible()

  await page.goto(`/tickets?status=Closed&plate=${plate}`)
  const row = page.getByRole('row', { name: new RegExp(plate) })
  await expect(row).toBeVisible()
  await expect(row).toContainText(code)
  await expect(row).toContainText('Cliente Teste E2E')
})
