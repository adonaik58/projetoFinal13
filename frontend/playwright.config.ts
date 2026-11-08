import { defineConfig, devices } from '@playwright/test'

/**
 * End-to-end tests run against a deployed stack (Docker Compose locally, staging in CI):
 *   E2E_BASE_URL=https://localhost E2E_ADMIN_USERNAME=… E2E_ADMIN_PASSWORD=… npm run test:e2e
 */
const baseURL = process.env.E2E_BASE_URL ?? 'https://localhost'

export default defineConfig({
  testDir: './e2e',
  // Flows share server state (spaces, users), so run them one at a time.
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  timeout: 60_000,
  expect: { timeout: 10_000 },
  reporter: process.env.CI
    ? [['list'], ['html', { open: 'never' }], ['junit', { outputFile: 'test-results/junit.xml' }]]
    : [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL,
    // The local stack uses a self-signed certificate.
    ignoreHTTPSErrors: true,
    locale: 'pt-PT',
    timezoneId: 'Africa/Luanda',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    {
      name: 'mobile',
      use: { ...devices['Pixel 7'] },
      testMatch: /auth\.spec\.ts/,
    },
  ],
})
