---
description: "Use when building Angular API service modules, HttpClient wrappers, interceptors, auth header injection, or typed API client code."
applyTo: "src/web-app/src/app/services/**,src/web-app/src/app/interceptors/**"
---
# Angular API Client Guidelines

## Base Service

- Configure API base URL from `environment.apiUrl`.
- Use a single `ApiService` with generic methods (`get<T>`, `post<T>`, `put<T>`, `delete<T>`).
- Auto-inject `Authorization: Bearer <token>` via a functional HTTP interceptor.
- Always use `provideHttpClient(withInterceptors([...]))` in `app.config.ts`.

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
- Unwrap `envelope.data` in the base service — domain services receive clean types.

## Service Modules

- One service per API domain (e.g., `ProductService`, `UserService`).
- Inject `ApiService` — never use `HttpClient` directly in domain services.
- Return `Observable<T>` — let consumers decide when to subscribe.
- Keep services stateless — use component or state management for caching.

## Interceptors (Functional)

- Auth interceptor: clones request with `Authorization` header.
- Error interceptor: maps `HttpErrorResponse` to typed `ApiError`.
- Register in `provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))`.
- Order matters — auth first, then error.

## Error Handling

- Use typed `ApiError` class with `message`, `status`, and optional `code`.
- Handle errors in `catchError` within the error interceptor.
- Components handle user-facing error display, not services.
