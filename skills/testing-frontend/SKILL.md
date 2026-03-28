---
name: testing-frontend
description: >-
  Write frontend tests using Vitest and React Testing Library. Use when: testing
  React components, hooks, user interactions, form submissions, accessibility
  assertions, or mocking API services.
argument-hint: 'Describe the React component or hook you want to test.'
---

# Frontend Testing (React)

## When to Use

- Testing React components (rendering, interaction, accessibility)
- Testing custom hooks
- Mocking API calls or services
- Writing integration tests for pages/features
- Verifying form validation and submission

## Official Documentation

- [Vitest](https://vitest.dev/guide/)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [MSW (Mock Service Worker)](https://mswjs.io/docs)
- [jest-dom matchers](https://github.com/testing-library/jest-dom)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.**

## Procedure

1. Follow [React testing reference](./references/frontend-testing.md) for patterns and conventions
2. Review [component test sample](./samples/component-test-sample.tsx)
3. Query elements by accessible role/label — never by CSS class or test ID
4. Use `userEvent` (not `fireEvent`) for user interactions
5. Test: rendering, interaction, accessibility, edge cases, error states
6. Mock API services with MSW or vi.fn()
7. Verify tests are deterministic — no real network calls
8. Run `npm test` locally before committing

## Test Structure

```
tests/
  web-app.tests/
    components/          # Shared component tests
    features/            # Feature-specific tests
    hooks/               # Custom hook tests
    setup.ts             # Test configuration
```

## Naming Convention

- Test file: `{ComponentName}.test.tsx`
- Test suite: `describe('{ComponentName}', () => { ... })`
- Test case: `it('should {expected behavior}', ...)`

## Key Patterns

- Query by role: `screen.getByRole('button', { name: 'Submit' })`
- Query by label: `screen.getByLabelText('Email')`
- Assert accessibility: `toBeVisible()`, `toHaveAccessibleName()`
- Keyboard: `userEvent.tab()`, `userEvent.keyboard('{Enter}')`
