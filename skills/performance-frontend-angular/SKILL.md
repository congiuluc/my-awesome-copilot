---
name: performance-frontend-angular
description: >-
  Optimize Angular 19 frontend performance with lazy loading, OnPush change detection,
  trackBy functions, bundle optimization, image optimization, and Core Web Vitals.
  Use when: reducing bundle size, lazy loading routes, optimizing change detection,
  virtualizing long lists, or improving Core Web Vitals in Angular applications.
argument-hint: 'Describe the Angular frontend performance issue or optimization needed.'
---

# Frontend Performance Optimization (Angular)

## When to Use

- Reducing initial bundle size with route-level lazy loading
- Optimizing change detection with `OnPush` strategy
- Adding `trackBy` functions to `@for` loops / `*ngFor`
- Virtualizing large lists with `@angular/cdk/scrolling`
- Optimizing images with `NgOptimizedImage`
- Improving Core Web Vitals (LCP, CLS, INP)

## Official Documentation

- [Angular Lazy Loading](https://angular.dev/guide/routing/lazy-loading)
- [OnPush Change Detection](https://angular.dev/guide/components/advanced-configuration#changedetectionstrategy)
- [NgOptimizedImage](https://angular.dev/guide/image-optimization)
- [CDK Virtual Scrolling](https://material.angular.io/cdk/scrolling/overview)
- [Angular Performance Checklist](https://angular.dev/best-practices/runtime-performance)
- [Web Vitals](https://web.dev/vitals/)

## Procedure

1. Identify bottleneck type: bundle size, change detection, image loading, list rendering
2. Apply [Angular performance patterns](./references/frontend-performance-angular.md)
3. Review [lazy loading sample](./samples/lazy-routes-sample.ts)
4. Ensure all components use `OnPush` change detection
5. Add `trackBy` to every `@for` / `*ngFor` loop
6. Use `NgOptimizedImage` for all `<img>` tags
7. Virtualize lists with 100+ items using CDK `VirtualScrollViewport`
8. Measure with Lighthouse / Angular DevTools before and after

## Performance Review Checklist

| # | Check | Tool / Method |
|---|-------|---------------|
| 1 | Bundle size < 200 KB gzipped (initial load) | `ng build --stats-json` + webpack-bundle-analyzer |
| 2 | All routes use `loadComponent` lazy loading | Code review |
| 3 | All components use `ChangeDetectionStrategy.OnPush` | Code review |
| 4 | Every `@for` / `*ngFor` has a `trackBy` function | Code review |
| 5 | Images use `NgOptimizedImage` with `width`/`height` | Code review |
| 6 | Above-the-fold images have `priority` attribute | Code review |
| 7 | Lists > 100 items use `cdk-virtual-scroll-viewport` | Code review |
| 8 | No unnecessary subscriptions (use `takeUntilDestroyed`) | Code review |
| 9 | Signals used instead of getters in templates | Code review |
| 10 | LCP < 2.5s, CLS < 0.1, INP < 200ms | Lighthouse / Web Vitals |
| 11 | Third-party scripts loaded async/deferred | HTML review |
| 12 | Fonts use `font-display: swap` | CSS review |

## Profiling Workflow

1. Run Lighthouse: `npx lighthouse http://localhost:4200 --view`
2. Use Angular DevTools Profiler to inspect change detection cycles
3. Run `ng build --stats-json` then `npx webpack-bundle-analyzer dist/stats.json`
4. Check Chrome DevTools → Performance tab for runtime bottlenecks
5. Compare before/after metrics for any optimization
