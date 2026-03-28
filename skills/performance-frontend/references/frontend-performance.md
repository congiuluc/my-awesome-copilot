# Frontend Performance

## Code Splitting & Lazy Loading

Split routes and heavy components to reduce initial bundle:

```tsx
import { lazy, Suspense } from 'react';
import { Spinner } from '@/components/shared/Spinner';

// Lazy load route-level components
const ProductDetailPage = lazy(() =>
  import('@/features/products/pages/ProductDetailPage')
    .then(m => ({ default: m.ProductDetailPage }))
);

const AdminPage = lazy(() =>
  import('@/features/admin/pages/AdminPage')
    .then(m => ({ default: m.AdminPage }))
);

export const Router = () => (
  <Suspense fallback={<Spinner label="Loading page..." />}>
    <Routes>
      <Route path="/product/:id" element={<ProductDetailPage />} />
      <Route path="/admin" element={<AdminPage />} />
    </Routes>
  </Suspense>
);
```

## Image Optimization

```tsx
// Lazy load images with native loading attribute
<img
  src={imageUrl}
  alt={title}
  loading="lazy"
  decoding="async"
  className="w-full h-48 object-cover rounded"
/>

// Responsive images with srcset
<img
  src={imageUrl}
  srcSet={`${imageUrl}?w=400 400w, ${imageUrl}?w=800 800w`}
  sizes="(max-width: 768px) 100vw, 50vw"
  alt={title}
  loading="lazy"
  decoding="async"
/>
```

## Image Preloading During Data Loading

Whenever you fetch data that includes image URLs, **preload the images during the
loading phase** so they render instantly when the skeleton/spinner is dismissed.

```tsx
import { useEffect, useState } from 'react';

export const useImagePreload = (urls: string[]): boolean => {
  const [ready, setReady] = useState(urls.length === 0);

  useEffect(() => {
    if (urls.length === 0) { setReady(true); return; }
    let cancelled = false;
    Promise.all(
      urls.map(src => new Promise<void>(resolve => {
        const img = new Image();
        img.onload = img.onerror = () => resolve();
        img.src = src;
      }))
    ).then(() => { if (!cancelled) setReady(true); });
    return () => { cancelled = true; };
  }, [urls]);

  return ready;
};
```

Usage in components:

```tsx
const { data, loading } = useProducts();
const imagesReady = useImagePreload(data.map(p => p.imageUrl).filter(Boolean));

if (loading || !imagesReady) return <Spinner />;
```

For critical above-the-fold hero images, inject `<link rel="preload" as="image">`
into the `<head>` early so the browser starts downloading immediately.

## Memoization

Use `useMemo` and `useCallback` for expensive computations and stable references:

```tsx
// ✅ Memoize expensive computation
const sortedProducts = useMemo(
  () => products.toSorted((a, b) =>
    new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime()
  ),
  [products]
);

// ✅ Stable callback reference for child components
const handleSelect = useCallback((id: string) => {
  navigate(`/product/${id}`);
}, [navigate]);
```

**Do NOT memoize** simple computations or components that always re-render with new props.

## List Virtualization

For long lists (100+ items), consider virtualization:

```tsx
// Use a library like @tanstack/react-virtual for large lists
import { useVirtualizer } from '@tanstack/react-virtual';

export const VirtualizedList = ({ items }: { items: ProductDto[] }) => {
  const parentRef = useRef<HTMLDivElement>(null);
  const virtualizer = useVirtualizer({
    count: items.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 80,
  });

  return (
    <div ref={parentRef} className="h-96 overflow-auto">
      <div style={{ height: `${virtualizer.getTotalSize()}px`, position: 'relative' }}>
        {virtualizer.getVirtualItems().map(virtualItem => (
          <div
            key={virtualItem.key}
            style={{
              position: 'absolute',
              top: 0,
              transform: `translateY(${virtualItem.start}px)`,
              width: '100%',
            }}
          >
            <ProductCard {...items[virtualItem.index]} />
          </div>
        ))}
      </div>
    </div>
  );
};
```

## Vite Build Optimization

```typescript
// vite.config.ts
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
        },
      },
    },
    sourcemap: true,
    target: 'es2022',
  },
});
```

## Debouncing

```tsx
export const useDebounce = <T,>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
};

// Usage for search
const SearchBar = () => {
  const [query, setQuery] = useState('');
  const debouncedQuery = useDebounce(query, 300);

  useEffect(() => {
    if (debouncedQuery) {
      searchProducts(debouncedQuery);
    }
  }, [debouncedQuery]);

  return <input value={query} onChange={e => setQuery(e.target.value)} />;
};
```

## Rules

- Measure first: use Lighthouse, DevTools Performance tab, `React.Profiler`.
- Lazy load routes and heavy components — not small shared components.
- Optimize images: `loading="lazy"`, `decoding="async"`, appropriate sizes.
- Debounce search and filter inputs (300ms default).
- Virtualize lists with 100+ items.
- Split vendor chunks from application code.
- Don't over-optimize: premature `useMemo`/`useCallback` adds complexity without benefit.
