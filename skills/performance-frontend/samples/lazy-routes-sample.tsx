// Sample: Lazy-loaded routes with React.lazy and Suspense
// Shows code splitting, route-based lazy loading, and loading fallbacks.

import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Spinner } from '@/components/shared/Spinner';
import { PageContainer } from '@/components/layout/PageContainer';
import { ErrorBoundary } from '@/components/shared/ErrorBoundary';

// --- Lazy-loaded page components (code split per route) ---
const HomePage = lazy(() => import('@/features/home/HomePage'));
const ProductListPage = lazy(() => import('@/features/product/ProductListPage'));
const ProductDetailPage = lazy(() => import('@/features/product/ProductDetailPage'));
const DashboardPage = lazy(() => import('@/features/dashboard/DashboardPage'));
const NotFoundPage = lazy(() => import('@/features/errors/NotFoundPage'));

// --- Loading fallback ---
const PageLoader = () => (
  <div className="flex min-h-[50vh] items-center justify-center" role="status">
    <Spinner size="lg" />
    <span className="sr-only">Loading page...</span>
  </div>
);

// --- App Router ---
export const AppRouter = () => (
  <BrowserRouter>
    <ErrorBoundary>
      <PageContainer>
        <Suspense fallback={<PageLoader />}>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/products" element={<ProductListPage />} />
            <Route path="/products/:id" element={<ProductDetailPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Suspense>
      </PageContainer>
    </ErrorBoundary>
  </BrowserRouter>
);

// --- Prefetching strategy (optional) ---
// Preload a route when the user hovers over a link:
//
// const prefetchRoute = (importFn: () => Promise<unknown>) => {
//   return {
//     onMouseEnter: () => importFn(),
//     onFocus: () => importFn(),
//   };
// };
//
// <Link to="/dashboard" {...prefetchRoute(() => import('@/features/dashboard/DashboardPage'))}>
//   Dashboard
// </Link>

// --- Image optimization ---
// <img
//   src={imageUrl}
//   alt="Product preview"
//   className="w-full h-auto aspect-video object-cover"
//   loading="lazy"
//   decoding="async"
//   srcSet={`${imageUrl}?w=400 400w, ${imageUrl}?w=800 800w`}
//   sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
// />
