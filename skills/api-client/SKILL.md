---
name: api-client
description: "Build typed API client layers for React frontends. Covers fetch wrapper, error interceptors, request/response types from API envelope, auth header injection, cancellation, and retry patterns. Use when: creating API service modules, handling API errors, building typed fetch wrappers, or integrating with the backend ApiResponse envelope."
argument-hint: 'Describe the API client need: fetch wrapper, error handling, auth injection, or typed service.'
---

# Frontend API Client

## When to Use

- Creating a typed fetch wrapper for the backend API
- Handling API errors from the `ApiResponse<T>` envelope
- Injecting authentication headers into requests
- Building service modules for specific API domains
- Implementing request cancellation with `AbortController`
- Adding retry logic for transient failures

## Official Documentation

- [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)
- [AbortController](https://developer.mozilla.org/en-US/docs/Web/API/AbortController)

## Key Principles

- **One API client instance** — configure base URL and headers once.
- **Type-safe responses** — map `ApiResponse<T>` envelope to typed results.
- **Errors are first-class** — check `response.success`, throw typed errors.
- **Auth is automatic** — inject bearer token in a single interceptor, not per-call.
- **Cancellation is free** — pass `AbortSignal` to every fetch call.

## Procedure

### 1. Base API Client

```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

interface ApiResponse<T> {
    success: boolean;
    data: T | null;
    error: string | null;
}

class ApiError extends Error {
    constructor(
        message: string,
        public status: number,
        public code?: string
    ) {
        super(message);
        this.name = 'ApiError';
    }
}

async function request<T>(
    path: string,
    options: RequestInit = {},
    signal?: AbortSignal
): Promise<T> {
    const token = getAccessToken();
    const response = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        signal,
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
            ...options.headers,
        },
    });

    if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new ApiError(
            body?.error ?? response.statusText,
            response.status
        );
    }

    const envelope: ApiResponse<T> = await response.json();
    if (!envelope.success) {
        throw new ApiError(envelope.error ?? 'Unknown error', response.status);
    }
    return envelope.data as T;
}
```

### 2. Service Modules

```typescript
export const productService = {
    getAll: (signal?: AbortSignal) =>
        request<Product[]>('/products', {}, signal),

    getById: (id: string, signal?: AbortSignal) =>
        request<Product>(`/products/${encodeURIComponent(id)}`, {}, signal),

    create: (data: CreateProductRequest) =>
        request<Product>('/products', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    update: (id: string, data: UpdateProductRequest) =>
        request<Product>(`/products/${encodeURIComponent(id)}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    delete: (id: string) =>
        request<void>(`/products/${encodeURIComponent(id)}`, {
            method: 'DELETE',
        }),
};
```

### 3. TanStack Query Integration

```typescript
export function useProducts() {
    return useQuery({
        queryKey: ['products'],
        queryFn: ({ signal }) => productService.getAll(signal),
        staleTime: 5 * 60 * 1000,
    });
}
```

- TanStack Query passes `signal` automatically for cancellation.
- Service functions accept `AbortSignal` as optional last parameter.

### 4. Error Handling in Components

```typescript
if (error instanceof ApiError) {
    if (error.status === 401) redirectToLogin();
    if (error.status === 404) return <NotFound />;
    return <ErrorMessage message={error.message} />;
}
```

## Anti-Patterns

- ❌ Calling `fetch` directly in components (use service modules)
- ❌ Not checking `response.success` from the API envelope
- ❌ Hardcoding API base URLs (use environment variables)
- ❌ Building URLs with string concatenation (use `encodeURIComponent`)
- ❌ Ignoring cancellation (always pass `AbortSignal`)
