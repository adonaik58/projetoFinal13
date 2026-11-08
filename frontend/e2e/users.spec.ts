import { expect, test } from '@playwright/test'

import { login, loginAsAdmin, selectOption, uniqueSuffix } from './helpers'

test.describe.configure({ mode: 'serial' })

test('gestão de utilizadores e controlo de acesso por perfil', async ({ page, browser }) => {
  const suffix = uniqueSuffix().toLowerCase()
  const username = `operador.${suffix}`
  const password = `Operador${suffix}1`

  await loginAsAdmin(page)
  await page.goto('/management/users')

  await page.getByRole('button', { name: 'Adicionar utilizador' }).click()
  const dialog = page.getByRole('dialog')
  await selectOption(page, 'Perfil', 'Operador')
  await dialog.getByLabel('Nome completo').fill('Operador E2E')
  await dialog.getByLabel('Nome de utilizador').fill(username)
  await dialog.getByLabel('Senha').fill(password)
  await dialog.getByRole('button', { name: 'Criar utilizador' }).click()
  await expect(page.getByText('Utilizador criado.')).toBeVisible()

  await page.getByLabel('Pesquisar').fill(username)
  const row = page.getByRole('row', { name: new RegExp(username) })
  await expect(row).toBeVisible()

  // The operator only sees operational pages.
  const operatorContext = await browser.newContext({ ignoreHTTPSErrors: true })
  const operatorPage = await operatorContext.newPage()
  await login(operatorPage, username, password)
  await expect(operatorPage).toHaveURL(/\/spaces/)
  const nav = operatorPage.getByRole('navigation', { name: 'Navegação principal' })
  await expect(nav.getByRole('link', { name: 'Espaços' })).toBeVisible()
  await expect(nav.getByRole('link', { name: 'Utilizadores' })).toHaveCount(0)
  await operatorPage.goto('/management/users')
  await expect(operatorPage).toHaveURL(/\/spaces/)
  await operatorContext.close()

  await row.getByRole('button', { name: /Ações para/ }).click()
  await page.getByRole('menuitem', { name: 'Editar' }).click()
  await page.getByRole('dialog').getByLabel('Nome completo').fill('Operador E2E Editado')
  await page.getByRole('button', { name: 'Guardar alterações' }).click()
  await expect(page.getByText('Utilizador atualizado.')).toBeVisible()
  await expect(row).toContainText('Operador E2E Editado')

  await row.getByRole('button', { name: /Ações para/ }).click()
  await page.getByRole('menuitem', { name: 'Desativar' }).click()
  await page.getByRole('alertdialog').getByRole('button', { name: 'Desativar' }).click()
  await expect(page.getByText('Utilizador desativado.')).toBeVisible()
  await expect(row).toContainText('Desativado')

  // A deactivated account can no longer sign in.
  const blockedContext = await browser.newContext({ ignoreHTTPSErrors: true })
  const blockedPage = await blockedContext.newPage()
  await blockedPage.goto('/login')
  await blockedPage.getByLabel('Nome de utilizador').fill(username)
  await blockedPage.getByLabel('Senha', { exact: true }).fill(password)
  await blockedPage.getByRole('button', { name: 'Entrar' }).click()
  await expect(blockedPage.getByRole('alert')).toBeVisible()
  await expect(blockedPage).toHaveURL(/\/login/)
  await blockedContext.close()
})
