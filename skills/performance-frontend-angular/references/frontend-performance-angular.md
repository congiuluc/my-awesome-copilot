# Angular Frontend Performance Patterns

## Lazy Loading Routes

```typescript
// app.routes.ts — lazy load all feature routes
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list.component')
        .then(m => m.ProductListComponent),
  },
  {
    path: 'products/:id',
    loadComponent: () =>
      import('./features/products/product-detail.component')
        .then(m => m.ProductDetailComponent),
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./features/admin/admin.routes')
        .then(m => m.ADMIN_ROUTES),
  },
];
```

## OnPush Change Detection

```typescript
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-product-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="card">
      <h3>{{ product().name }}</h3>
      <p>{{ product().price | currency }}</p>
      <button (click)="selected.emit(product())">View</button>
    </div>
  `,
})
export class ProductCardComponent {
  product = input.required<Product>();
  selected = output<Product>();
}
```

## TrackBy with @for

```typescript
// Angular 19 @for syntax with built-in track
@Component({
  template: `
    @for (product of products(); track product.id) {
      <app-product-card [product]="product" />
    }
  `,
})
export class ProductListComponent {
  products = signal<Product[]>([]);
}
```

## NgOptimizedImage

```typescript
import { NgOptimizedImage } from '@angular/common';

@Component({
  standalone: true,
  imports: [NgOptimizedImage],
  template: `
    <!-- Above-the-fold: add priority for LCP -->
    <img [ngSrc]="heroImageUrl" width="1200" height="600" priority />

    <!-- Below-the-fold: lazy loaded automatically -->
    <img [ngSrc]="product.imageUrl" width="400" height="300" />
  `,
})
export class ProductPageComponent {}
```

## Virtual Scrolling for Long Lists

```typescript
import { ScrollingModule } from '@angular/cdk/scrolling';

@Component({
  standalone: true,
  imports: [ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport itemSize="72" class="h-[600px]">
      <div *cdkVirtualFor="let item of items; trackBy: trackById" class="h-[72px]">
        {{ item.name }}
      </div>
    </cdk-virtual-scroll-viewport>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LargeListComponent {
  items = signal<Item[]>([]);

  trackById(_index: number, item: Item): string {
    return item.id;
  }
}
```

## Signals Instead of Getters in Templates

```typescript
// BAD: getter re-evaluated every change detection cycle
get fullName(): string {
  return `${this.firstName} ${this.lastName}`;
}

// GOOD: computed signal, memoized automatically
fullName = computed(() => `${this.firstName()} ${this.lastName()}`);
```

## Subscription Cleanup

```typescript
import { DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export class MyComponent {
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.dataService.getData()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => this.data.set(data));
  }
}
```

## Preconnect for External Resources

```html
<!-- In index.html — preconnect to API and CDN origins -->
<link rel="preconnect" href="https://api.example.com" />
<link rel="preconnect" href="https://cdn.example.com" crossorigin />
```

## Bundle Analysis

```bash
# Build with stats for analysis
ng build --stats-json

# Analyze the bundle
npx webpack-bundle-analyzer dist/my-app/stats.json
```
