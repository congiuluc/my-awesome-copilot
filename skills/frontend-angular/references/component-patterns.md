# Angular Component Patterns

## Standalone Component (Standard Pattern)

All components MUST be standalone. Never use NgModules for component declarations.

```typescript
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-item-list',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './item-list.component.html',
  styleUrl: './item-list.component.css',
})
export class ItemListComponent {
  private readonly itemService = inject(ItemService);
  private readonly destroyRef = inject(DestroyRef);

  readonly items = signal<Item[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  constructor() {
    this.loadItems();
  }

  private loadItems(): void {
    this.itemService.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.items.set(response.data);
          } else {
            this.error.set(response.error ?? 'Failed to load items');
          }
          this.loading.set(false);
        },
        error: () => {
          this.error.set('An unexpected error occurred');
          this.loading.set(false);
        },
      });
  }

  retry(): void {
    this.loading.set(true);
    this.error.set(null);
    this.loadItems();
  }
}
```

## Template Pattern with State Handling

```html
@if (loading()) {
  <app-spinner aria-label="Loading items" />
} @else if (error(); as errorMessage) {
  <div role="alert" class="error-container">
    <p>{{ errorMessage }}</p>
    <button (click)="retry()" type="button">Try Again</button>
  </div>
} @else if (items().length === 0) {
  <div class="empty-state">
    <p>No items found.</p>
  </div>
} @else {
  <ul role="list">
    @for (item of items(); track item.id) {
      <li>
        <app-item-card [item]="item" />
      </li>
    }
  </ul>
}
```

## Typed Reactive Forms

```typescript
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

interface ItemForm {
  name: FormControl<string>;
  description: FormControl<string>;
  category: FormControl<string>;
}

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  // ...
})
export class ItemFormComponent {
  readonly form = new FormGroup<ItemForm>({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    description: new FormControl('', { nonNullable: true }),
    category: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  readonly submitting = signal(false);

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    const value = this.form.getRawValue();
    // Submit logic...
  }
}
```

## Service Pattern with HttpClient

```typescript
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/items';

  getAll(): Observable<ApiResponse<Item[]>> {
    return this.http.get<ApiResponse<Item[]>>(this.baseUrl);
  }

  getById(id: string): Observable<ApiResponse<Item>> {
    return this.http.get<ApiResponse<Item>>(`${this.baseUrl}/${encodeURIComponent(id)}`);
  }

  create(request: CreateItemRequest): Observable<ApiResponse<Item>> {
    return this.http.post<ApiResponse<Item>>(this.baseUrl, request);
  }

  update(id: string, request: UpdateItemRequest): Observable<ApiResponse<Item>> {
    return this.http.put<ApiResponse<Item>>(
      `${this.baseUrl}/${encodeURIComponent(id)}`,
      request,
    );
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/${encodeURIComponent(id)}`,
    );
  }
}
```

## Functional Guard

```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/login']);
};
```

## Functional HTTP Interceptor

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
    return next(cloned);
  }
  return next(req);
};
```

## Lazy-Loaded Routes

```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'items',
    loadComponent: () => import('./features/items/pages/item-list/item-list.component')
      .then(m => m.ItemListComponent),
  },
  {
    path: 'items/new',
    loadComponent: () => import('./features/items/pages/item-form/item-form.component')
      .then(m => m.ItemFormComponent),
  },
  {
    path: 'items/:id',
    loadComponent: () => import('./features/items/pages/item-detail/item-detail.component')
      .then(m => m.ItemDetailComponent),
  },
];
```

## Computed Signals

Use `computed()` for derived state instead of recalculating in templates:

```typescript
readonly items = signal<Item[]>([]);
readonly searchTerm = signal('');

readonly filteredItems = computed(() => {
  const term = this.searchTerm().toLowerCase();
  if (!term) return this.items();
  return this.items().filter(item =>
    item.name.toLowerCase().includes(term)
  );
});

readonly itemCount = computed(() => this.filteredItems().length);
```
