# Angular Frontend Error Handling Patterns

## Custom ErrorHandler

```typescript
import { ErrorHandler, Injectable, inject } from '@angular/core';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    // Log to console and telemetry
    console.error('Unhandled error:', error);

    // Extract message for user-facing display
    const message =
      error instanceof Error ? error.message : 'An unexpected error occurred.';

    // Optionally: inject a toast/notification service here
    // const toast = inject(ToastService);
    // toast.error(message);
  }
}

// Register in app.config.ts:
// providers: [{ provide: ErrorHandler, useClass: GlobalErrorHandler }]
```

## HTTP Error Interceptor

### Functional Interceptor (Angular 19)

```typescript
import {
  HttpInterceptorFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let userMessage: string;
      switch (true) {
        case error.status === 0:
          userMessage = 'Network error. Please check your connection.';
          break;
        case error.status === 401:
          userMessage = 'Session expired. Please log in again.';
          // Redirect to login if needed
          break;
        case error.status === 403:
          userMessage = 'You do not have permission to perform this action.';
          break;
        case error.status === 404:
          userMessage = 'Resource not found.';
          break;
        case error.status >= 500:
          userMessage = 'Server error. Please try again later.';
          break;
        default:
          userMessage = 'An unexpected error occurred.';
      }
      console.error(`HTTP ${error.status}:`, error.message);
      return throwError(() => new Error(userMessage));
    })
  );
};

// Register in app.config.ts:
// provideHttpClient(withInterceptors([errorInterceptor]))
```

## Retry Interceptor

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { retry, timer } from 'rxjs';

export const retryInterceptor: HttpInterceptorFn = (req, next) => {
  // Only retry idempotent/safe methods
  if (['GET', 'HEAD', 'OPTIONS'].includes(req.method)) {
    return next(req).pipe(
      retry({
        count: 2,
        delay: (error, retryCount) => timer(retryCount * 1000),
      })
    );
  }
  return next(req);
};
```

## Component Error/Loading/Empty State Pattern

```typescript
import { Component, signal } from '@angular/core';

@Component({
  template: `
    @if (loading()) {
      <app-spinner aria-label="Loading items" />
    } @else if (error()) {
      <div role="alert" class="p-4 bg-red-50 rounded">
        <p class="text-red-700">{{ error() }}</p>
        <button (click)="load()" class="mt-2 text-blue-600 underline">
          Retry
        </button>
      </div>
    } @else if (items().length === 0) {
      <app-empty-state message="No items found" />
    } @else {
      @for (item of items(); track item.id) {
        <app-item-card [item]="item" />
      }
    }
  `
})
export class ItemListComponent {
  readonly items = signal<Item[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.itemService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.items.set(response.data);
        } else {
          this.error.set(response.error ?? 'Failed to load items.');
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message);
        this.loading.set(false);
      },
    });
  }
}
```

## API Envelope Error Handling

```typescript
import { ApiResponse } from '../models/api-response';

// In a service:
getProducts(): Observable<Product[]> {
  return this.http.get<ApiResponse<Product[]>>('/api/products').pipe(
    map((response) => {
      if (!response.success) {
        throw new Error(response.error ?? 'Unknown API error');
      }
      return response.data;
    })
  );
}
```
