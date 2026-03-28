---
name: frontend-react
description: "Build React 19 + TypeScript + Vite frontend with TailwindCSS v4. Use when: creating components, pages, hooks, services, routing, state management, API integration, or frontend project structure."
argument-hint: 'Describe the component, page, hook, or feature to build.'
---

# Frontend React TypeScript

## When to Use

- Creating or modifying React components
- Building new features, pages, or shared components
- Configuring Vite, TypeScript, or TailwindCSS
- Integrating with backend API endpoints

## Official Documentation

- [React 19](https://react.dev/)
- [Vite](https://vite.dev/guide/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/)
- [TailwindCSS v4](https://tailwindcss.com/docs)
- [React Router](https://reactrouter.com/)

## Procedure

1. Determine file location using [project structure](./references/project-structure.md)
2. Follow [component patterns](./references/component-patterns.md) for all UI code
3. Apply [TypeScript conventions](./references/typescript-conventions.md)
4. Review [sample component](./samples/feature-component-sample.tsx) for complete pattern
5. Style with TailwindCSS v4 — mobile-first responsive
6. Ensure accessibility (see `accessibility` skill)
7. Handle loading, error, and empty states for data-fetching components
8. **Preload images** during data loading — see [image preloading](#image-preloading-during-data-loading)
9. Create corresponding tests (see `testing` skill)

## Image Preloading During Data Loading

Whenever a component fetches data that contains image URLs, **always preload images
during the loading phase** so they display instantly when the skeleton/spinner is dismissed.

Use the `useImagePreload` hook (below) or `<link rel="preload">` for critical above-the-fold images.

See [component patterns – image preloading](./references/component-patterns.md#image-preloading) for the hook and usage examples.
