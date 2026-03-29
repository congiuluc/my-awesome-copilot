---
name: state-management-angular
description: "Manage frontend state in Angular 19 applications. Covers Angular signals, signal store (NgRx SignalStore), RxJS-based service state, URL state with Router, and state composition patterns. Use when: choosing a state management approach, implementing caching, managing global UI state, or optimizing change detection."
argument-hint: 'Describe the state requirement: signals, signal store, service state, or caching pattern.'
---

# Angular State Management

## When to Use

- Choosing a state management approach for a new feature
- Setting up Angular signals for component state
- Using NgRx SignalStore for feature-level state
- Managing global UI state (theme, sidebar, modals)
- Implementing reactive service patterns with RxJS
- Optimizing change detection with OnPush and signals

## Official Documentation

- [Angular Signals](https://angular.dev/guide/signals)
- [NgRx SignalStore](https://ngrx.io/guide/signals)
- [Angular Router](https://angular.dev/guide/routing)
- [RxJS](https://rxjs.dev/)

## Key Principles

- **Signals are the default** — use `signal()` and `computed()` for reactive state.
- **Collocate state** — keep state as close to where it's used as possible.
- **URL is state** — search params, filters, pagination belong in query params.
- **Derive, don't duplicate** — use `computed()` instead of storing derived values.
- **OnPush everywhere** — signals + OnPush = optimal change detection.

## State Categories

| Category | Tool | Examples |
|----------|------|---------|
| **Local UI** | `signal()`, `computed()` | Form inputs, open/close, selection |
| **Feature** | NgRx SignalStore | Feature-level CRUD state, complex workflows |
| **Global UI** | Service + signals | Theme, auth status, sidebar |
| **URL** | `ActivatedRoute`, `Router` | Filters, sort, page number |
| **Form** | Typed Reactive Forms | Validation, submission, field state |

## Procedure

### 1. Component State (Signals)

```typescript
import { Component, signal, computed, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-counter',
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <p>Count: {{ count() }}</p>
        <p>Double: {{ doubleCount() }}</p>
        <button (click)="increment()">+</button>
    `,
})
export class CounterComponent {
    readonly count = signal(0);
    readonly doubleCount = computed(() => this.count() * 2);

    increment() {
        this.count.update((c) => c + 1);
    }
}
```

- Use `signal()` for mutable state.
- Use `computed()` for derived state.
- Use `update()` for state transitions based on previous value.

### 2. Feature State (NgRx SignalStore)

```typescript
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { ProductService } from '../services/product.service';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap } from 'rxjs';
import { tapResponse } from '@ngrx/operators';

interface ProductState {
    products: Product[];
    loading: boolean;
    error: string | null;
    selectedId: string | null;
}

const initialState: ProductState = {
    products: [],
    loading: false,
    error: null,
    selectedId: null,
};

export const ProductStore = signalStore(
    { providedIn: 'root' },
    withState(initialState),
    withComputed((store) => ({
        selectedProduct: computed(() =>
            store.products().find((p) => p.id === store.selectedId())
        ),
        productCount: computed(() => store.products().length),
    })),
    withMethods((store, productService = inject(ProductService)) => ({
        loadProducts: rxMethod<void>(
            pipe(
                tap(() => patchState(store, { loading: true, error: null })),
                switchMap(() =>
                    productService.getAll().pipe(
                        tapResponse({
                            next: (products) => patchState(store, { products, loading: false }),
                            error: (error: Error) =>
                                patchState(store, { error: error.message, loading: false }),
                        })
                    )
                )
            )
        ),
        selectProduct(id: string) {
            patchState(store, { selectedId: id });
        },
    }))
);
```

- Use `signalStore` for feature-level state that multiple components share.
- Use `withState` for initial state shape.
- Use `withComputed` for derived state.
- Use `withMethods` + `rxMethod` for async operations.
- Use `patchState` for immutable updates.

### 3. Global UI State (Service + Signals)

```typescript
import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
    private readonly themeSignal = signal<'light' | 'dark'>('dark');

    readonly theme = this.themeSignal.asReadonly();
    readonly isDark = computed(() => this.themeSignal() === 'dark');

    toggle() {
        this.themeSignal.update((t) => (t === 'light' ? 'dark' : 'light'));
    }
}
```

- Use services with private `signal()` and public `asReadonly()`.
- Expose `computed()` for derived state.
- Keep services focused on a single concern.

### 4. URL State (Router)

```typescript
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Component({ /* ... */ })
export class ProductListComponent {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);

    readonly page = toSignal(
        this.route.queryParamMap.pipe(
            map((params) => Number(params.get('page') ?? '1'))
        ),
        { initialValue: 1 }
    );

    goToPage(page: number) {
        this.router.navigate([], {
            queryParams: { page },
            queryParamsHandling: 'merge',
        });
    }
}
```

- Use `toSignal()` to bridge RxJS route params into signals.
- Use `queryParamsHandling: 'merge'` to preserve other params.
- Filters, sort direction, and pagination belong in the URL.

### 5. RxJS-to-Signal Bridge

```typescript
import { toSignal, toObservable } from '@angular/core/rxjs-interop';

// Observable → Signal
const data = toSignal(observable$, { initialValue: [] });

// Signal → Observable
const data$ = toObservable(signalValue);
```

- Use `toSignal()` when consuming observables in templates.
- Use `toObservable()` when feeding signals into RxJS pipelines.
- Always provide `initialValue` for `toSignal()` to avoid `undefined`.

## Anti-Patterns to Avoid

- **BehaviorSubject for simple state** — use `signal()` instead
- **NgRx Store for simple CRUD** — use SignalStore or service + signals
- **Storing derived state** — use `computed()` instead
- **Manual subscriptions in components** — use `toSignal()` or `async` pipe
- **State in components that should be shared** — extract to a service or store
- **Mutating signal values directly** — always use `set()`, `update()`, or `patchState()`
