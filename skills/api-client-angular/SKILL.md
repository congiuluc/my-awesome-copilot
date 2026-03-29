---
name: api-client-angular
description: "Build typed API client services for Angular 19 frontends. Covers HttpClient configuration, typed interceptors, error handling, request/response types from API envelope, auth header injection, and retry patterns. Use when: creating Angular API services, handling API errors, building typed HttpClient wrappers, or integrating with the backend ApiResponse envelope."
argument-hint: 'Describe the API client need: HttpClient service, error interceptor, auth injection, or typed service.'
---

# Angular API Client

## When to Use

- Creating a typed HttpClient service for the backend API
- Handling API errors from the `ApiResponse<T>` envelope
- Injecting authentication headers via functional interceptors
- Building typed services for specific API domains
- Implementing retry logic for transient failures
- Configuring request/response interceptors

## Official Documentation

- [Angular HttpClient](https://angular.dev/guide/http)
- [Angular HTTP Interceptors](https://angular.dev/guide/http/interceptors)
- [RxJS Operators](https://rxjs.dev/guide/operators)

## Key Principles

- **One `HttpClient` configuration** — set up base URL and interceptors in `provideHttpClient()`.
- **Type-safe responses** — map `ApiResponse<T>` envelope to typed results.
- **Errors are first-class** — check `response.success`, throw typed errors.
- **Auth is automatic** — inject bearer token in a functional interceptor, not per-call.
- **Use RxJS pipelines** — leverage `pipe()`, `map()`, `catchError()` for clean data flows.

## Procedure

### 1. API Configuration

```typescript
// app.config.ts
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
    providers: [
        provideHttpClient(
            withInterceptors([authInterceptor, errorInterceptor])
        ),
    ],
};
```

### 2. API Response Envelope

```typescript
// models/api-response.model.ts
export interface ApiResponse<T> {
    success: boolean;
    data: T | null;
    error: string | null;
}
```

### 3. Auth Interceptor (Functional)

```typescript
// interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getAccessToken();

    if (token) {
        req = req.clone({
            setHeaders: { Authorization: `Bearer ${token}` },
        });
    }

    return next(req);
};
```

### 4. Error Interceptor (Functional)

```typescript
// interceptors/error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export class ApiError extends Error {
    constructor(
        message: string,
        public status: number,
        public code?: string
    ) {
        super(message);
        this.name = 'ApiError';
    }
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            const apiError = new ApiError(
                error.error?.error ?? error.statusText,
                error.status
            );
            return throwError(() => apiError);
        })
    );
};
```

### 5. Base API Service

```typescript
// services/api.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = environment.apiUrl;

    get<T>(path: string): Observable<T> {
        return this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`).pipe(
            map((envelope) => this.unwrap(envelope))
        );
    }

    post<T>(path: string, body: unknown): Observable<T> {
        return this.http.post<ApiResponse<T>>(`${this.baseUrl}${path}`, body).pipe(
            map((envelope) => this.unwrap(envelope))
        );
    }

    put<T>(path: string, body: unknown): Observable<T> {
        return this.http.put<ApiResponse<T>>(`${this.baseUrl}${path}`, body).pipe(
            map((envelope) => this.unwrap(envelope))
        );
    }

    delete<T>(path: string): Observable<T> {
        return this.http.delete<ApiResponse<T>>(`${this.baseUrl}${path}`).pipe(
            map((envelope) => this.unwrap(envelope))
        );
    }

    private unwrap<T>(envelope: ApiResponse<T>): T {
        if (!envelope.success) {
            throw new ApiError(envelope.error ?? 'Unknown error', 0);
        }
        return envelope.data as T;
    }
}
```

### 6. Domain Service Example

```typescript
// services/product.service.ts
import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Product, CreateProductRequest } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
    private readonly api = inject(ApiService);

    getAll() {
        return this.api.get<Product[]>('/products');
    }

    getById(id: string) {
        return this.api.get<Product>(`/products/${encodeURIComponent(id)}`);
    }

    create(request: CreateProductRequest) {
        return this.api.post<Product>('/products', request);
    }

    update(id: string, request: Partial<CreateProductRequest>) {
        return this.api.put<Product>(`/products/${encodeURIComponent(id)}`, request);
    }

    delete(id: string) {
        return this.api.delete<void>(`/products/${encodeURIComponent(id)}`);
    }
}
```

## Anti-Patterns to Avoid

- **Subscribing in services** — return `Observable`, let consumers subscribe
- **Manual `Authorization` headers per request** — use the auth interceptor
- **Ignoring the envelope** — always unwrap `ApiResponse<T>` in the base service
- **Using `any` for response types** — always type the response
- **Not encoding URL parameters** — always use `encodeURIComponent` for path params
