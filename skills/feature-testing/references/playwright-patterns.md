# Playwright Patterns

> Official reference: [Playwright Best Practices](https://playwright.dev/docs/best-practices)

## Project Setup

```bash
npm init playwright@latest
```

Config file (`playwright.config.ts`):

```ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [['html'], ['list']],
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
    { name: 'mobile-chrome', use: { ...devices['Pixel 5'] } },
    { name: 'mobile-safari', use: { ...devices['iPhone 13'] } },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
});
```

## Locator Best Practices

Always use **user-facing locators** — never CSS selectors or XPath:

```ts
// ✅ Preferred — accessible locators
page.getByRole('button', { name: 'Submit' });
page.getByLabel('Email address');
page.getByText('Welcome back');
page.getByPlaceholder('Search...');

// ✅ Test ID as fallback only
page.getByTestId('product-card');

// ❌ Avoid — brittle selectors
page.locator('.btn-primary');
page.locator('#submit-btn');
page.locator('div > span.title');
```

## Page Object Model

Encapsulate page interactions in reusable classes:

```ts
// pages/login.page.ts
import { type Page, type Locator } from '@playwright/test';

export class LoginPage {
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorMessage: Locator;

  constructor(private readonly page: Page) {
    this.emailInput = page.getByLabel('Email');
    this.passwordInput = page.getByLabel('Password');
    this.submitButton = page.getByRole('button', { name: 'Sign in' });
    this.errorMessage = page.getByRole('alert');
  }

  async goto() {
    await this.page.goto('/login');
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }
}
```

## Accessibility Testing with axe-core

```ts
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test('page has no accessibility violations', async ({ page }) => {
  await page.goto('/products');
  await page.waitForLoadState('networkidle');

  const results = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
    .analyze();

  expect(results.violations).toEqual([]);
});
```

## Responsive Testing

```ts
import { test, expect, devices } from '@playwright/test';

const viewports = [
  { name: 'mobile', width: 375, height: 812 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'desktop', width: 1280, height: 900 },
];

for (const vp of viewports) {
  test(`layout works on ${vp.name}`, async ({ page }) => {
    await page.setViewportSize({ width: vp.width, height: vp.height });
    await page.goto('/');

    // No horizontal scrollbar
    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    expect(scrollWidth).toBeLessThanOrEqual(clientWidth);
  });
}
```

## API Mocking

```ts
test('shows error when API fails', async ({ page }) => {
  await page.route('**/api/products', route =>
    route.fulfill({ status: 500, body: JSON.stringify({ success: false, error: 'Server error' }) })
  );

  await page.goto('/products');
  await expect(page.getByText('Something went wrong')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Retry' })).toBeVisible();
});
```

## Visual Regression

```ts
test('product card matches snapshot', async ({ page }) => {
  await page.goto('/products');
  await page.waitForLoadState('networkidle');

  await expect(page.getByTestId('product-grid')).toHaveScreenshot('product-grid.png', {
    maxDiffPixelRatio: 0.01,
  });
});
```

## Official References

- [Playwright Locators](https://playwright.dev/docs/locators)
- [Playwright Page Object Model](https://playwright.dev/docs/pom)
- [Playwright API Mocking](https://playwright.dev/docs/mock)
- [Playwright Visual Comparisons](https://playwright.dev/docs/test-snapshots)
- [axe-core Playwright](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright)
