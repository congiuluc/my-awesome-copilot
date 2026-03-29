---
name: error-handling-frontend-angular
description: >-
  Implement consistent Angular error handling with ErrorHandler, HTTP interceptors,
  user-friendly error states, and toast notifications. Use when: creating global
  ErrorHandler, handling HTTP errors in interceptors, building retry UI, or showing
  error toasts in Angular apps.
argument-hint: 'Describe the Angular error scenario or component to protect.'
---

# Frontend Error Handling (Angular)

## When to Use

- Setting up a global `ErrorHandler` for uncaught errors
- Creating HTTP error interceptors
- Handling loading/error/empty states in components
- Building retry UI for failed requests
- Showing toast notifications for errors

## Official Documentation

- [Angular ErrorHandler](https://angular.dev/api/core/ErrorHandler)
- [Angular HTTP Interceptors](https://angular.dev/guide/http/interceptors)
- [RxJS Error Handling](https://rxjs.dev/guide/operators#error-handling-operators)

## Procedure

1. Create a global `ErrorHandler` implementation
2. Create a functional HTTP error interceptor
3. Review [Angular error handling patterns](./references/angular-error-handling.md) for advanced scenarios
4. Review [error interceptor sample](./samples/error-interceptor.ts)
4. Handle loading/error/empty states in **every** data-fetching component
5. Show user-friendly messages — never expose raw error details
6. Provide retry/refresh actions for recoverable errors
7. Log errors for monitoring (console + optional telemetry)
8. Handle API envelope errors: check `response.success` and `response.error`

## Global Error Handler

```typescript
import { ErrorHandler, Injectable, inject } from '@angular/core';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    console.error('Unhandled error:', error);
    // Send to telemetry service if available
  }
}

// Register in app.config.ts:
// providers: [{ provide: ErrorHandler, useClass: GlobalErrorHandler }]
```

## HTTP Error Interceptor

```typescript
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { inject } from '@angular/core';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';
      if (error.status === 0) {
        message = 'Network error. Please check your connection.';
      } else if (error.status === 404) {
        message = 'Resource not found.';
      } else if (error.status >= 500) {
        message = 'Server error. Please try again later.';
      }
      // Show toast notification
      console.error(message, error);
      return throwError(() => new Error(message));
    })
  );
};
```

## Error State Pattern

```typescript
@Component({
  template: `
    @if (loading()) {
      <app-spinner aria-label="Loading" />
    } @else if (error()) {
      <app-error-message [message]="error()!" (retry)="load()" />
    } @else if (items().length === 0) {
      <app-empty-state message="No items found" />
    } @else {
      <!-- render items -->
    }
  `
})
export class ItemListComponent {
  readonly items = signal<Item[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
}
```

## Constraints

- NEVER expose raw error objects or stack traces to the user
- ALWAYS handle loading, error, and empty states in data-fetching components
- ALWAYS provide retry actions for network/server errors
- ALWAYS use the API envelope pattern to check `response.success`
- ALWAYS log errors for monitoring before showing user messages
