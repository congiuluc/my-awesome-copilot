---
name: feature-testing
description: >-
  End-to-end feature testing with Playwright to verify completeness, usability,
  accessibility, and production readiness. Use when: writing E2E tests, verifying
  a feature works across browsers, testing accessibility with axe-core, running
  visual regression, or checking responsive behavior in tests.
argument-hint: 'Describe the feature or user flow to test end-to-end.'
---

# Feature Testing (Playwright)

## When to Use

- Verifying a feature works end-to-end (API + UI integrated)
- Testing critical user flows across browsers (Chromium, Firefox, WebKit)
- Running accessibility audits with `@axe-core/playwright`
- Visual regression testing for UI consistency
- Verifying responsive behavior at different viewports
- Smoke testing before deployment

## Official Documentation

- [Playwright Docs](https://playwright.dev/docs/intro)
- [Playwright Test API](https://playwright.dev/docs/api/class-test)
- [Playwright Accessibility](https://playwright.dev/docs/accessibility-testing)
- [axe-core/playwright](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)

## Procedure

1. Define test scope from [feature checklist](./references/feature-checklist.md)
2. Write tests following [Playwright patterns](./references/playwright-patterns.md)
3. Review [feature test sample](./samples/feature-test-sample.spec.ts) for structure
4. Test completeness: all acceptance criteria from user story are covered
5. Test usability: user can complete the flow without confusion
6. Test accessibility: run axe-core scan on every page of the flow
7. Test responsiveness: run at mobile (375px), tablet (768px), desktop (1280px)
8. Test error paths: invalid input, network failure, empty states
9. Run cross-browser: `npx playwright test --project=chromium --project=firefox --project=webkit`
10. Generate report: `npx playwright show-report`

## Completion Matrix

| Dimension | What to Verify |
|-----------|---------------|
| **Completeness** | All acceptance criteria pass, happy + edge paths |
| **Usability** | Flow completes in logical steps, no dead ends, clear feedback |
| **Accessibility** | Zero axe-core violations (critical/serious), keyboard-navigable |
| **Responsiveness** | Works at 375px, 768px, 1280px without overflow or truncation |
| **Performance** | Page loads < 3s, no layout shift, images lazy-loaded |
| **Cross-browser** | Passes on Chromium, Firefox, WebKit |
