---
description: "Use when writing E2E feature tests with Playwright. Covers cross-browser testing, accessibility audits with axe-core, responsive testing, and page object patterns."
applyTo: "tests/MyApp.E2E/**,tests/e2e/**,**/*.spec.ts"
---
# Feature Testing Guidelines (Playwright)

## Test Scope

Every feature test must verify the **completeness matrix**:

| Dimension | What to Check |
|-----------|---------------|
| Completeness | Happy path + edge cases + error paths |
| Usability | Clear feedback, logical flow, no dead ends |
| Accessibility | Zero axe-core critical/serious violations |
| Responsiveness | 375px (mobile), 768px (tablet), 1280px (desktop) |
| Performance | Page load < 3s, no layout shift |
| Cross-browser | Chromium, Firefox, WebKit |

## Accessibility Testing

```typescript
import AxeBuilder from '@axe-core/playwright';

const results = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa'])
    .analyze();
expect(results.violations).toEqual([]);
```

- Run axe-core on every page under test — no exceptions.
- Test full keyboard navigation: Tab, Enter, Space, Escape.

## Test Structure

- Use **Page Object Model** for reusable selectors and actions.
- Name tests: `{Feature} - {Scenario}` (e.g., "Login - Valid credentials redirects to dashboard").
- One assertion theme per test — don't test everything in one test.

## Selectors

- Prefer `getByRole()`, `getByLabel()`, `getByText()` — accessible selectors.
- Avoid CSS selectors and XPath — they break on refactors.
- Use `data-testid` only when no accessible selector is available.

## Responsive Testing

```typescript
test('renders correctly on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    // assertions
});
```

- Test at all three breakpoints for layout-dependent features.
- Verify no horizontal overflow or content truncation.

## Test Isolation

- Each test starts from a clean state — no dependency on other tests.
- Use API calls to set up test data, not UI interactions.
- Clean up test data in `afterEach` or use per-test isolation.
