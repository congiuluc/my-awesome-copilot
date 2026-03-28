---
name: error-handling-frontend
description: >-
  Implement consistent frontend error handling with error boundaries, API error
  states, and user-friendly error messages. Use when: creating error boundaries,
  handling API errors in React, building retry UI, or showing toast notifications
  for errors.
argument-hint: 'Describe the React error scenario or component to protect.'
---

# Frontend Error Handling (React)

## When to Use

- Creating React error boundaries for rendering errors
- Handling API fetch errors in data-fetching components
- Building retry UI for failed requests
- Showing toast notifications or inline error messages
- Handling form validation errors from the API

## Official Documentation

- [React Error Boundaries](https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary)
- [Error Handling Patterns (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Control_flow_and_error_handling)

## Procedure

1. Wrap route-level components with `ErrorBoundary` — see [error handling reference](./references/frontend-error-handling.md)
2. Review [error boundary sample](./samples/error-boundary-sample.tsx)
3. Handle loading/error/empty states in **every** data-fetching component
4. Show user-friendly messages — never expose raw error details
5. Provide retry/refresh actions for recoverable errors
6. Log frontend errors for monitoring (console + optional telemetry)
7. Handle API envelope errors: check `response.success` and `response.error`

## Error State Pattern

```tsx
const { data, loading, error } = useProducts();

if (loading) return <Spinner aria-label="Loading" />;
if (error) return <ErrorMessage message={error} onRetry={refetch} />;
if (data.length === 0) return <EmptyState message="No items found" />;
```
