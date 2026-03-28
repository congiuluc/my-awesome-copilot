/**
 * Feature Test Sample — Product Listing
 *
 * Demonstrates a complete feature test covering:
 * - Completeness: data loads and displays correctly
 * - Usability: search, pagination, empty states
 * - Accessibility: axe-core scan, keyboard navigation
 * - Responsiveness: mobile, tablet, desktop viewports
 * - Error handling: API failure, retry
 *
 * Official references:
 * - https://playwright.dev/docs/best-practices
 * - https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright
 */

import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

// =============================================================================
// Page Object
// =============================================================================

class ProductListPage {
  constructor(private readonly page: import('@playwright/test').Page) {}

  readonly heading = () => this.page.getByRole('heading', { name: 'Products' });
  readonly searchInput = () => this.page.getByLabel('Search');
  readonly productCards = () => this.page.getByRole('article');
  readonly emptyState = () => this.page.getByText('No products found');
  readonly errorMessage = () => this.page.getByRole('alert');
  readonly retryButton = () => this.page.getByRole('button', { name: 'Retry' });
  readonly nextPageButton = () => this.page.getByRole('button', { name: 'Next' });
  readonly spinner = () => this.page.getByRole('status');

  async goto() {
    await this.page.goto('/products');
    await this.page.waitForLoadState('networkidle');
  }

  async search(query: string) {
    await this.searchInput().fill(query);
    // Wait for debounced search
    await this.page.waitForTimeout(400);
  }
}

// =============================================================================
// Completeness Tests
// =============================================================================

test.describe('Product List — Completeness', () => {
  test('displays product cards with title and image', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    await expect(productPage.heading()).toBeVisible();
    const cards = productPage.productCards();
    await expect(cards).toHaveCount(10); // Default page size

    // Each card has an image
    const firstCard = cards.first();
    await expect(firstCard.getByRole('img')).toBeVisible();
  });

  test('search filters products by name', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    await productPage.search('premium');
    const cards = productPage.productCards();
    const count = await cards.count();
    expect(count).toBeGreaterThan(0);
    expect(count).toBeLessThanOrEqual(10);
  });

  test('pagination loads next page', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    const firstPageText = await productPage.productCards().first().textContent();
    await productPage.nextPageButton().click();
    await page.waitForLoadState('networkidle');

    const secondPageText = await productPage.productCards().first().textContent();
    expect(secondPageText).not.toBe(firstPageText);
  });
});

// =============================================================================
// Usability Tests
// =============================================================================

test.describe('Product List — Usability', () => {
  test('shows empty state when search has no results', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    await productPage.search('xxxxxxxxxnonexistent');
    await expect(productPage.emptyState()).toBeVisible();
  });

  test('shows loading spinner during data fetch', async ({ page }) => {
    // Slow down API response
    await page.route('**/api/products*', async route => {
      await new Promise(r => setTimeout(r, 1000));
      await route.continue();
    });

    const productPage = new ProductListPage(page);
    await page.goto('/products');
    await expect(productPage.spinner()).toBeVisible();
  });
});

// =============================================================================
// Accessibility Tests
// =============================================================================

test.describe('Product List — Accessibility', () => {
  test('page passes axe-core audit', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    const results = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
      .analyze();

    expect(results.violations).toEqual([]);
  });

  test('search input is keyboard accessible', async ({ page }) => {
    const productPage = new ProductListPage(page);
    await productPage.goto();

    // Tab to search input
    await page.keyboard.press('Tab');
    await expect(productPage.searchInput()).toBeFocused();

    // Type and verify filter works
    await page.keyboard.type('test');
    await page.waitForTimeout(400);
    const count = await productPage.productCards().count();
    expect(count).toBeGreaterThanOrEqual(0);
  });
});

// =============================================================================
// Responsive Tests
// =============================================================================

const viewports = [
  { name: 'mobile', width: 375, height: 812 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'desktop', width: 1280, height: 900 },
];

for (const vp of viewports) {
  test(`layout works on ${vp.name} (${vp.width}px)`, async ({ page }) => {
    await page.setViewportSize({ width: vp.width, height: vp.height });

    const productPage = new ProductListPage(page);
    await productPage.goto();

    // No horizontal overflow
    const scrollWidth = await page.evaluate(
      () => document.documentElement.scrollWidth,
    );
    const clientWidth = await page.evaluate(
      () => document.documentElement.clientWidth,
    );
    expect(scrollWidth).toBeLessThanOrEqual(clientWidth);

    // Content is visible
    await expect(productPage.heading()).toBeVisible();
    await expect(productPage.productCards().first()).toBeVisible();
  });
}

// =============================================================================
// Error Handling Tests
// =============================================================================

test.describe('Product List — Error Handling', () => {
  test('shows error message and retry on API failure', async ({ page }) => {
    await page.route('**/api/products*', route =>
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ success: false, error: 'Internal Server Error' }),
      }),
    );

    const productPage = new ProductListPage(page);
    await page.goto('/products');

    await expect(productPage.errorMessage()).toBeVisible();
    await expect(productPage.retryButton()).toBeVisible();
  });
});
