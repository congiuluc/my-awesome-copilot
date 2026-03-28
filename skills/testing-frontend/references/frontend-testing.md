# Frontend Testing (React)

## Frameworks

- **Vitest** as the test runner and assertion library.
- **React Testing Library** for component testing.
- **@testing-library/user-event** for realistic user interactions.
- **MSW (Mock Service Worker)** for mocking API calls in integration tests.

## Test Structure

```
tests/
  {frontend-app}.tests/
    components/     # Shared component tests
    features/       # Feature-specific component tests
    hooks/          # Custom hook tests
    services/       # API service tests
    utils/          # Utility function tests
    setup.ts        # Global test setup (RTL matchers, MSW handlers)
```

## Naming Convention

- Test file: `{ComponentName}.test.tsx` or `{functionName}.test.ts`.
- Mirror source structure in test directory.

## Component Test Patterns

### Basic Rendering

```tsx
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ProductCard } from '@/features/products';

describe('ProductCard', () => {
  const defaultProps = {
    title: 'Summer Collection',
    imageUrl: '/summer.jpg',
    description: 'Win big this summer',
    onSelect: vi.fn(),
  };

  it('renders title and image with accessible alt text', () => {
    render(<ProductCard {...defaultProps} />);

    expect(screen.getByText('Summer Collection')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: 'Summer Collection' })).toBeInTheDocument();
  });

  it('renders description when provided', () => {
    render(<ProductCard {...defaultProps} />);
    expect(screen.getByText('Win big this summer')).toBeInTheDocument();
  });

  it('does not render description when omitted', () => {
    render(<ProductCard {...defaultProps} description={undefined} />);
    expect(screen.queryByText('Win big this summer')).not.toBeInTheDocument();
  });
});
```

### User Interaction

```tsx
import userEvent from '@testing-library/user-event';

it('calls onSelect when card is clicked', async () => {
  const user = userEvent.setup();
  const onSelect = vi.fn();
  render(<ProductCard {...defaultProps} onSelect={onSelect} />);

  await user.click(screen.getByRole('article'));
  expect(onSelect).toHaveBeenCalledOnce();
});

it('supports keyboard activation', async () => {
  const user = userEvent.setup();
  const onSelect = vi.fn();
  render(<ProductCard {...defaultProps} onSelect={onSelect} />);

  await user.tab();
  await user.keyboard('{Enter}');
  expect(onSelect).toHaveBeenCalledOnce();
});
```

### Form Testing

```tsx
describe('CreateProductForm', () => {
  it('shows validation error for empty name', async () => {
    const user = userEvent.setup();
    render(<CreateProductForm onSubmit={vi.fn()} />);

    await user.click(screen.getByRole('button', { name: /submit/i }));
    expect(screen.getByRole('alert')).toHaveTextContent(/name is required/i);
  });

  it('calls onSubmit with form data when valid', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    render(<CreateProductForm onSubmit={onSubmit} />);

    await user.type(screen.getByLabelText(/name/i), 'New Item');
    await user.click(screen.getByRole('button', { name: /submit/i }));

    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'New Item' })
    );
  });
});
```

### Custom Hook Testing

```tsx
import { renderHook, waitFor } from '@testing-library/react';
import { useProducts } from '@/features/products/hooks/useProducts';

describe('useProducts', () => {
  it('returns loading state initially', () => {
    const { result } = renderHook(() => useProducts());
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toEqual([]);
  });

  it('returns data after successful fetch', async () => {
    const { result } = renderHook(() => useProducts());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toHaveLength(2);
    expect(result.current.error).toBeNull();
  });
});
```

## What to Test

- Component rendering with different props (required, optional, edge values).
- User interactions: clicks, typing, form submissions, keyboard navigation.
- Conditional rendering: loading spinners, error messages, empty states.
- Accessibility: elements have correct roles, labels, and ARIA attributes.
- API integration hooks: loading, success, error states.
- Routing behavior when applicable.

## Rules

- Query elements by **role** first (`getByRole`), then by **label** (`getByLabelText`), then by **text** (`getByText`). Avoid `getByTestId`.
- Use `userEvent` over `fireEvent` for realistic interaction simulation.
- Always `await` user interactions.
- Use `vi.fn()` for mocks, `vi.spyOn()` for spying on module exports.
- Test behavior from the user's perspective, not implementation details.
- Clean up API mocks between tests.
