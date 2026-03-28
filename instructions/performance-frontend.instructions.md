---
description: "Use when optimizing frontend React performance: code splitting, lazy loading, image preloading, memoization, virtualization, and Core Web Vitals."
applyTo: "src/web-app/src/routes/**,src/web-app/src/pages/**,src/web-app/vite.config.*"
---
# Frontend Performance Guidelines

## Bundle Size

- Initial bundle: **< 200 KB gzipped** target.
- Lazy load route-level components with `React.lazy()`.
- Use Vite's automatic code splitting — each route gets its own chunk.
- Tree-shake unused imports — prefer named exports over barrel files.

## Image Optimization

- **Always preload images** during the data-loading phase:
  ```tsx
  const imagesReady = useImagePreload(data.map(p => p.imageUrl));
  ```
- Use `loading="lazy"` and `decoding="async"` on `<img>` elements.
- Serve images in WebP/AVIF format with appropriate `srcset` sizes.
- Use `fetchpriority="high"` on LCP images only.

## Code Splitting

```tsx
const Dashboard = React.lazy(() => import('./pages/Dashboard'));

<Suspense fallback={<Spinner />}>
    <Dashboard />
</Suspense>
```

- Wrap lazy components in `Suspense` with a loading fallback.
- Split at route boundaries first, then large feature components.

## Memoization

- Use `useMemo` for **expensive computations** (filtering/sorting large lists).
- Use `useCallback` for callbacks passed to memoized children.
- **Don't over-memoize** — premature optimization adds complexity with no benefit.
- Profile before memoizing — only optimize measured bottlenecks.

## Virtualization

- Lists with **100+ items** must use virtualization (e.g., `@tanstack/react-virtual`).
- Never render thousands of DOM nodes — virtualize or paginate.

## Core Web Vitals Targets

| Metric | Target |
|--------|--------|
| LCP (Largest Contentful Paint) | < 2.5s |
| CLS (Cumulative Layout Shift) | < 0.1 |
| INP (Interaction to Next Paint) | < 200ms |

- Fonts: use `font-display: swap` to prevent invisible text.
- Third-party scripts: load with `async` or `defer`.
