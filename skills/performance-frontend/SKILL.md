---
name: performance-frontend
description: >-
  Optimize frontend React performance with code splitting, lazy loading, image
  preloading, memoization, and virtualization. Use when: reducing bundle size,
  lazy loading routes, preloading images during data fetching, optimizing
  re-renders, virtualizing long lists, or improving Core Web Vitals.
argument-hint: 'Describe the frontend performance issue or optimization needed.'
---

# Frontend Performance Optimization (React)

## When to Use

- Reducing initial bundle size with code splitting
- Lazy loading routes or heavy components
- Preloading images during data loading phase
- Optimizing re-renders with `useMemo` / `useCallback`
- Virtualizing large lists (100+ items)
- Improving Core Web Vitals (LCP, CLS, INP)

## Official Documentation

- [React Code Splitting](https://react.dev/reference/react/lazy)
- [Lighthouse](https://developer.chrome.com/docs/lighthouse)
- [Web Vitals](https://web.dev/vitals/)
- [Vite Build Optimization](https://vite.dev/guide/build)

## Procedure

1. Identify bottleneck type: bundle size, render performance, image loading, list length
2. Apply [code splitting and optimization patterns](./references/frontend-performance.md)
3. Review [lazy routes sample](./samples/lazy-routes-sample.tsx)
4. **Always preload images** during the data loading phase (see image preloading pattern)
5. Measure with Lighthouse / Chrome DevTools before and after
6. Lazy load route-level components with `React.lazy`
7. Memoize expensive computations — but don't over-memoize simple operations
8. Virtualize lists with 100+ items

## Image Preloading (Mandatory)

When data contains image URLs, **preload images during loading state** so they
render instantly when the spinner is dismissed:

```tsx
const { data, loading } = useProducts();
const imagesReady = useImagePreload(data.map(p => p.imageUrl).filter(Boolean));

if (loading || !imagesReady) return <Spinner />;
```

See the `frontend-react` skill for the `useImagePreload` hook implementation.

## Performance Review Checklist

Use this checklist when reviewing frontend code for performance issues:

| # | Check | Tool / Method |
|---|-------|---------------|
| 1 | Bundle size < 200 KB gzipped (initial load) | `npx vite-bundle-visualizer` |
| 2 | No unused dependencies in `package.json` | `npx depcheck` |
| 3 | Route-level code splitting with `React.lazy` | Manual review |
| 4 | Images use `loading="lazy"` and `decoding="async"` | Lighthouse |
| 5 | Images preloaded during data fetch | Manual review |
| 6 | Lists > 100 items use virtualization | Manual review |
| 7 | No unnecessary re-renders (React DevTools Profiler) | React DevTools |
| 8 | `useMemo`/`useCallback` only for expensive operations | Manual review |
| 9 | LCP < 2.5s, CLS < 0.1, INP < 200ms | Lighthouse / Web Vitals |
| 10 | No layout thrashing (forced synchronous layouts) | Chrome DevTools Performance |
| 11 | Fonts use `font-display: swap` | CSS review |
| 12 | Third-party scripts loaded async/deferred | HTML review |

## Profiling Workflow

1. Run Lighthouse: `npx lighthouse http://localhost:5173 --view`
2. Open Chrome DevTools → Performance tab → Record user flow
3. Check React DevTools Profiler for wasted renders
4. Run `npx vite-bundle-visualizer` to find large chunks
5. Compare before/after metrics for any optimization
