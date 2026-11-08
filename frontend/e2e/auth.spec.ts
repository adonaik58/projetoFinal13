import { expect, test } from '@playwright/test'

import { adminCredentials, login } from './helpers'

test.describe('Autenticação', () => {
  test('rejeita credenciais inválidas', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel('Nome de utilizador').fill('utilizador-inexistente')
    await page.getByLabel('Senha', { exact: true }).fill('senha-errada-123')
    await page.getByRole('button', { name: 'Entrar' }).click()

    await expect(page.getByText('Nome de utilizador ou senha incorretos.')).toBeVisible()
    await expect(page).toHaveURL(/\/login/)
  })

  test('envia visitantes para o login com o caminho de regresso', async ({ page }) => {
    await page.goto('/tickets?status=Closed')
    await expect(page).toHaveURL(/\/login\?redirect=/)

    const { username, password } = adminCredentials()
    await page.getByLabel('Nome de utilizador').fill(username)
    await page.getByLabel('Senha', { exact: true }).fill(password)
    await page.getByRole('button', { name: 'Entrar' }).click()

    await expect(page).toHaveURL(/\/tickets\?status=Closed/)
    await expect(page.getByRole('tab', { name: 'Histórico' })).toHaveAttribute(
      'aria-selected',
      'true',
    )
  })

  test('redireciona os URLs do sistema antigo', async ({ page }) => {
    await page.goto('/gestor/users')
    await expect(page).toHaveURL(/redirect=%2Fmanagement%2Fusers/)
  })

  test('o administrador entra no dashboard, recupera a sessão e sai', async ({ page }) => {
    const { username, password } = adminCredentials()
    await login(page, username, password)

    await expect(page.getByRole('heading', { name: 'Dashboard', level: 1 })).toBeVisible()
    await expect(page.getByRole('list', { name: 'Indicadores' })).toBeVisible()

    // The access token lives in memory only; a reload must restore the session via the cookie.
    await page.reload()
    await expect(page.getByRole('heading', { name: 'Dashboard', level: 1 })).toBeVisible()
    expect(await page.evaluate(() => window.localStorage.getItem('accessToken'))).toBeNull()

    const userMenu = page.getByRole('button', { name: /Administrador|Operador/ })
    if (!(await userMenu.isVisible())) {
      // On small screens the user menu lives inside the off-canvas sidebar.
      await page.getByRole('button', { name: 'Alternar menu lateral' }).click()
    }
    await userMenu.click()
    await page.getByRole('menuitem', { name: 'Terminar sessão' }).click()

    await expect(page).toHaveURL(/\/login/)
    await page.goto('/')
    await expect(page).toHaveURL(/\/login/)
  })
})
