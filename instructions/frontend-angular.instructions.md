---
description: "Angular 19 frontend conventions: standalone components, signals, OnPush change detection, typed forms, inject() function, lazy-loaded routes."
applyTo: "src/web-app/src/app/**"
---

# Angular Frontend Conventions

## Component Rules
- Use **standalone components** — no NgModules for component declarations
- Use **`ChangeDetectionStrategy.OnPush`** on all components
- Use **Angular signals** (`signal`, `computed`) for component state
- Use **`inject()`** function for dependency injection — no constructor injection
- Use **`DestroyRef` + `takeUntilDestroyed()`** for subscription cleanup
- Use **named exports** only

## Routing
- Use **`loadComponent`** for route-level lazy loading
- Use **functional guards** — no class-based guards
- Use **functional HTTP interceptors** — no class-based interceptors

## Forms
- Use **typed reactive forms** with `FormGroup<T>` and `FormControl<T>`
- Always use `nonNullable: true` for form controls
- Mark all as touched before showing validation errors on submit

## Services
- Use `@Injectable({ providedIn: 'root' })` for app-wide services
- Use `HttpClient` with typed responses
- Return `Observable<ApiResponse<T>>` from service methods

## TypeScript
- No `any` type — use proper interfaces
- camelCase for parameters and private fields
- PascalCase for classes and interfaces
- Max line length: 120 characters

## Templates
- Use `@if`/`@else`/`@for` control flow (Angular 17+ syntax)
- Use `track` in `@for` loops
- Handle loading, error, and empty states in every data-fetching component

## Accessibility
- Semantic HTML elements
- `aria-label` on icon-only buttons
- `aria-live` for dynamic content updates
- All form inputs must have associated labels
