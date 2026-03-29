import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductService } from '../services/product.service';
import { Product } from '../models/product.model';
import { SpinnerComponent } from '@shared/components/spinner/spinner.component';

/**
 * Displays a searchable list of products with loading, error, and empty states.
 */
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <app-spinner aria-label="Loading products" />
    } @else if (error(); as errorMessage) {
      <div role="alert" class="rounded-lg border border-red-200 bg-red-50 p-4">
        <p class="text-red-800">{{ errorMessage }}</p>
        <button
          (click)="retry()"
          type="button"
          class="mt-2 rounded bg-red-600 px-4 py-2 text-white hover:bg-red-700
                 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
        >
          Try Again
        </button>
      </div>
    } @else if (filteredProducts().length === 0) {
      <div class="text-center text-gray-500 py-8">
        <p>No products found.</p>
      </div>
    } @else {
      <div class="mb-4">
        <label for="search" class="sr-only">Search products</label>
        <input
          id="search"
          type="search"
          placeholder="Search products..."
          [value]="searchTerm()"
          (input)="onSearch($event)"
          class="w-full rounded-lg border border-gray-300 px-4 py-2
                 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
        />
      </div>

      <p class="mb-2 text-sm text-gray-600">
        Showing {{ filteredProducts().length }} of {{ products().length }} products
      </p>

      <ul role="list" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        @for (product of filteredProducts(); track product.id) {
          <li>
            <a
              [routerLink]="['/products', product.id]"
              class="block rounded-lg border border-gray-200 p-4 shadow-sm
                     hover:shadow-md focus:outline-none focus:ring-2
                     focus:ring-blue-500 focus:ring-offset-2 transition-shadow"
            >
              @if (product.imageUrl) {
                <img
                  [src]="product.imageUrl"
                  [alt]="product.name"
                  loading="lazy"
                  class="mb-3 h-48 w-full rounded object-cover"
                />
              }
              <h3 class="text-lg font-semibold text-gray-900">
                {{ product.name }}
              </h3>
              <p class="mt-1 text-sm text-gray-600 line-clamp-2">
                {{ product.description }}
              </p>
              <p class="mt-2 text-lg font-bold text-blue-600">
                {{ product.price | currency }}
              </p>
            </a>
          </li>
        }
      </ul>
    }
  `,
})
export class ProductListComponent {
  private readonly productService = inject(ProductService);
  private readonly destroyRef = inject(DestroyRef);

  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly searchTerm = signal('');

  readonly filteredProducts = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.products();
    return this.products().filter(
      (p) =>
        p.name.toLowerCase().includes(term) ||
        p.description.toLowerCase().includes(term),
    );
  });

  constructor() {
    this.loadProducts();
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  retry(): void {
    this.loading.set(true);
    this.error.set(null);
    this.loadProducts();
  }

  private loadProducts(): void {
    this.productService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.products.set(response.data);
          } else {
            this.error.set(response.error ?? 'Failed to load products');
          }
          this.loading.set(false);
        },
        error: () => {
          this.error.set('An unexpected error occurred');
          this.loading.set(false);
        },
      });
  }
}
