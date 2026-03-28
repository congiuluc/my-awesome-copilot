---
description: "Use when writing, modifying, or reviewing frontend React tests. Covers Vitest, React Testing Library, user-event, accessibility assertions, and frontend test conventions."
applyTo: "tests/web-app.tests/**,tests/e2e/**"
---
# Frontend Testing Guidelines (React)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.** No exceptions.

## Frameworks

- **Vitest** as the test runner.
- **React Testing Library** for component testing.
- **@testing-library/user-event** for simulating user interactions.

## Test Structure

```
tests/
  web-app.tests/
    components/     # Shared component tests
    features/       # Feature-specific tests
    hooks/          # Custom hook tests
    services/       # API service tests
    utils/          # Utility function tests
```

## Naming Convention

- Test file: `{ComponentName}.test.tsx` or `{hook}.test.ts`.
- Describe block: component/function name.
- Test: describe behavior from user's perspective.

## Component Test Pattern

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { ProductCard } from '../src/features/products/ProductCard';

describe('ProductCard', () => {
  it('renders title and image', () => {
    render(
      <ProductCard
        title="Summer Collection"
        imageUrl="/img.jpg"
        onSelect={vi.fn()}
      />
    );

    expect(screen.getByText('Summer Collection')).toBeInTheDocument();
    expect(screen.getByRole('img'))
      .toHaveAttribute('alt', 'Summer Collection');
  });

  it('calls onSelect when clicked', async () => {
    const onSelect = vi.fn();
    render(
      <ProductCard title="Test" imageUrl="/img.jpg" onSelect={onSelect} />
    );

    await userEvent.click(screen.getByRole('button'));
    expect(onSelect).toHaveBeenCalledOnce();
  });
});
```

## What to Test

- Component rendering with different props.
- User interactions (clicks, form submissions, keyboard navigation).
- Conditional rendering and loading/error states.
- Accessibility: elements have correct roles, labels, and are keyboard accessible.
- API integration hooks with mocked fetch responses.

## Query Priority (prefer accessible queries)

1. `getByRole` — buttons, links, headings, etc.
2. `getByLabelText` — form inputs.
3. `getByText` — static text content.
4. `getByPlaceholderText` — only as fallback for inputs.
5. **Avoid**: `getByTestId` — only when no accessible query works.

## General Rules

- Tests must be **deterministic** — no reliance on external services or time.
- Use `userEvent` (not `fireEvent`) for user interactions.
- Mock external dependencies at boundaries, not internal logic.
- Keep tests fast: all tests < 100ms each.
- Verify keyboard navigation with `userEvent.tab()` and `userEvent.keyboard('{Enter}')`.
