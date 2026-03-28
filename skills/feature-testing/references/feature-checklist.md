# Feature Test Checklist

Use this checklist for every feature test suite to ensure coverage across all dimensions.

## Pre-Test Setup

- [ ] Feature acceptance criteria documented (from user story)
- [ ] Test data seeded or mocked via API/fixtures
- [ ] Environment running (dev server + API)

## Completeness

- [ ] Happy path: primary user flow completes successfully
- [ ] All acceptance criteria have at least one test
- [ ] Data persists correctly (create → read back)
- [ ] Navigation: user can reach and leave the feature
- [ ] State resets: feature works correctly on revisit

## Usability

- [ ] Form validation shows clear error messages
- [ ] Loading states display during async operations
- [ ] Empty states show helpful content
- [ ] Success feedback confirms completed actions
- [ ] No dead-end pages (always a way forward or back)

## Accessibility

- [ ] axe-core scan: zero critical/serious violations
- [ ] Keyboard navigation: Tab through all interactive elements
- [ ] Focus management: focus moves logically after actions
- [ ] Screen reader: elements have accessible names and roles
- [ ] Color contrast: meets WCAG 2.1 AA (4.5:1 normal, 3:1 large)

## Responsiveness

- [ ] Mobile (375px): content readable, no horizontal scroll
- [ ] Tablet (768px): layout adjusts, touch targets ≥ 44px
- [ ] Desktop (1280px): uses available space, no wasted whitespace

## Error Handling

- [ ] Invalid form input shows inline errors
- [ ] Network failure shows error message with retry option
- [ ] 404 / not-found shows appropriate fallback
- [ ] Unauthorized access redirects or shows message

## Performance

- [ ] Page loads in < 3s (Playwright `page.waitForLoadState('networkidle')`)
- [ ] No cumulative layout shift during load
- [ ] Images use lazy loading
- [ ] No unnecessary API calls on mount

## Official References

- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [WCAG 2.1 Quick Reference](https://www.w3.org/WAI/WCAG21/quickref/)
