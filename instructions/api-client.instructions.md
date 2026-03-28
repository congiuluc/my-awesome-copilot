---
description: "Use when building frontend API service modules, fetch wrappers, error interceptors, auth header injection, or typed API client code."
applyTo: "src/web-app/src/services/**,src/web-app/src/api/**,src/web-app/src/lib/api*"
---
# API Client Guidelines

## Base Client

- Configure API base URL from `import.meta.env.VITE_API_URL`.
- Use a single `request<T>()` function that handles auth headers, envelope parsing, and errors.
- Auto-inject `Authorization: Bearer <token>` from a central token provider.
- Always set `Content-Type: application/json`.

## API Envelope

All responses follow the `ApiResponse<T>` envelope:

```typescript
interface ApiResponse<T> {
    success: boolean;
    data: T | null;
    error: string | null;
}
```

- Always check `envelope.success` — throw `ApiError` if false.
- Return `envelope.data` — never the raw response.

## Service Modules

- One file per domain: `productService.ts`, `userService.ts`.
- Export an object with typed methods: `getAll`, `getById`, `create`, `update`, `delete`.
- Accept `AbortSignal` as optional last parameter for cancellation.
- Use `encodeURIComponent()` for URL parameters.

## Error Handling

- Define `ApiError` class with `message`, `status`, and optional `code`.
- Handle 401 → redirect to login, 404 → show not found, others → show error message.
- Never expose raw server errors to users.

## Integration with TanStack Query

- Extract queries into custom hooks per domain: `useProducts()`, `useCreateProduct()`.
- TanStack Query passes `signal` automatically — forward it to the service function.
