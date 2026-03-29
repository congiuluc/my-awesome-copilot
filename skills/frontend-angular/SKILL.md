---
name: frontend-angular
description: "Build Angular 19 + TypeScript frontend with standalone components, signals, and reactive patterns. Use when: creating components, pages, services, routing, forms, state management, API integration, or Angular project structure."
argument-hint: 'Describe the component, page, service, or feature to build.'
---

# Frontend Angular TypeScript

## When to Use

- Creating or modifying Angular standalone components
- Building new features, pages, or shared components
- Configuring Angular CLI, TypeScript, or styling
- Integrating with backend API endpoints using HttpClient
- Building reactive forms with typed form controls

## Official Documentation

- [Angular 19](https://angular.dev/)
- [Angular Signals](https://angular.dev/guide/signals)
- [Angular Standalone Components](https://angular.dev/guide/components)
- [Angular HttpClient](https://angular.dev/guide/http)
- [Angular Reactive Forms](https://angular.dev/guide/forms/reactive-forms)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/)
- [Angular CDK](https://material.angular.io/cdk/categories)

## Procedure

1. Determine file location using [project structure](./references/project-structure.md)
2. Follow [component patterns](./references/component-patterns.md) for all UI code
3. Apply [TypeScript conventions](./references/typescript-conventions.md)
4. Review [sample component](./samples/feature-component-sample.ts) for complete pattern
5. Style with TailwindCSS or Angular-compatible CSS — mobile-first responsive
6. Ensure accessibility (see `accessibility` skill)
7. Handle loading, error, and empty states for data-fetching components
8. Use Angular signals for reactive state management
9. Use `OnPush` change detection on all components
10. Use `takeUntilDestroyed()` for subscription cleanup
11. Create corresponding tests (see `testing-frontend-angular` skill)
