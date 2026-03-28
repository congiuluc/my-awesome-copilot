---
description: "Use when implementing frontend error handling: error boundaries, API error states, retry UI, and user-friendly error messages in React components."
applyTo: "src/web-app/src/components/error/**,src/web-app/src/components/Error*"
---
# Frontend Error Handling Guidelines

## Error Boundaries

- Wrap **route-level components** with `ErrorBoundary`.
- Error boundaries catch rendering errors — not event handler or async errors.
- Show a user-friendly fallback UI with a "Try Again" action.
- Log caught errors to an error reporting service.

## Data-Fetching Components

Handle **all four states** in every data-fetching component:

```tsx
const { data, loading, error, refetch } = useProducts();
if (loading) return <Spinner />;
if (error) return <ErrorMessage message={error} onRetry={refetch} />;
if (!data || data.length === 0) return <EmptyState />;
return <ProductList items={data} />;
```

- Never show a blank screen — always handle loading, error, and empty states.
- Provide retry/refresh actions for recoverable errors.

## API Error Handling

- Check `response.success` and `response.error` from the API envelope.
- Show user-friendly messages — never expose raw server errors or stack traces.
- Use toast notifications for transient errors (network timeouts, 5xx).
- Use inline error messages for validation errors (4xx).

## Error Messages

- Keep messages actionable: tell the user **what happened** and **what to do**.
- ✅ "Could not load products. Check your connection and try again."
- ❌ "Error: TypeError: Cannot read properties of undefined"

## Form Validation Errors

- Display errors inline, adjacent to the invalid field.
- Use `aria-describedby` to associate error messages with inputs.
- Clear errors when the user corrects the input.
