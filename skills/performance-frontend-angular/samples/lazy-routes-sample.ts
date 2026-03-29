// Sample: Angular 19 lazy-loaded routes with standalone components
// Demonstrates route-level code splitting for optimal bundle size.

import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home.component')
        .then(m => m.HomeComponent),
    title: 'Home',
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list.component')
        .then(m => m.ProductListComponent),
    title: 'Products',
  },
  {
    path: 'products/:id',
    loadComponent: () =>
      import('./features/products/product-detail.component')
        .then(m => m.ProductDetailComponent),
    title: 'Product Detail',
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./features/admin/admin.routes')
        .then(m => m.ADMIN_ROUTES),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./features/not-found/not-found.component')
        .then(m => m.NotFoundComponent),
    title: 'Not Found',
  },
];
