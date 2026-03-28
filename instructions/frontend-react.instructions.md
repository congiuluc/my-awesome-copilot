---
description: "Use when writing, modifying, or reviewing React TypeScript frontend code. Covers Vite, React 19, TailwindCSS v4, responsive design, component patterns, and state management."
applyTo: "src/web-app/**"
---
# Frontend React TypeScript Guidelines

## Stack Versions

- **React 19** with functional components only.
- **Vite** as build tool with TypeScript strict mode enabled.
- **TailwindCSS v4** for all styling — no CSS modules, no styled-components.
- **Node.js 22 LTS** runtime.

## Project Structure

```
src/web-app/
  src/
    components/
      shared/         # Reusable UI components (Button, Modal, Card, etc.)
      layout/         # Layout components (Header, Footer, Sidebar)
    features/
      product/        # Feature-specific components, hooks, types
      dashboard/
      auth/
    hooks/            # Shared custom hooks
    services/         # API client functions
    types/            # Shared TypeScript types/interfaces
    utils/            # Pure utility functions
    App.tsx
    main.tsx
```

## Component Conventions

- **Functional components only** — no class components.
- **Named exports** — no default exports.
- Props interface suffixed with `Props` and defined above the component.
- One component per file. File name matches component name in PascalCase.

```tsx
export interface ProductCardProps {
  title: string;
  imageUrl: string;
  onSelect: (id: string) => void;
}

export const ProductCard = ({ title, imageUrl, onSelect }: ProductCardProps) => {
  return (
    <div className="rounded-lg shadow-md p-4">
      <img src={imageUrl} alt={title} className="w-full h-48 object-cover rounded" />
      <h3 className="text-lg font-semibold mt-2">{title}</h3>
    </div>
  );
};
```

## Styling (TailwindCSS v4)

- Use Tailwind utility classes directly in JSX — no custom CSS files.
- Use `@theme` in `app.css` for design tokens (colors, spacing, fonts).
- For complex repeated patterns, extract Tailwind components via `@apply` in `app.css` or create shared components.
- Use `cn()` helper (from `clsx` + `tailwind-merge`) for conditional class merging.

## Responsive Design

- **Mobile-first** approach: start with base styles, add `sm:`, `md:`, `lg:`, `xl:` breakpoints.
- Test at breakpoints: 320px, 640px, 768px, 1024px, 1280px.
- Use Tailwind responsive variants: `className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3"`.
- Fluid typography and spacing where appropriate.
- Navigation must collapse to hamburger menu on mobile.

## Accessibility (a11y)

- All interactive elements must be keyboard accessible.
- Use semantic HTML: `<button>`, `<nav>`, `<main>`, `<section>`, `<article>`, `<header>`, `<footer>`.
- All images must have descriptive `alt` text (`alt=""` only for decorative images with `aria-hidden="true"`).
- Use ARIA attributes when semantic HTML is insufficient: `aria-label`, `aria-describedby`, `aria-live`.
- Ensure color contrast meets WCAG 2.1 AA (4.5:1 for text, 3:1 for large text).
- Focus indicators must be visible — never remove `outline` without replacement.
- Form inputs must have associated `<label>` elements.
- Use `role="alert"` or `aria-live="polite"` for dynamic status messages.

## State Management

- Use React's built-in state (`useState`, `useReducer`) for local component state.
- Use React Context for shared state that doesn't change frequently (theme, auth).
- For server state, use `fetch` with custom hooks that handle loading/error states.
- Avoid prop drilling beyond 2 levels — extract to Context or composition.

## API Integration

- Centralize API calls in `services/` folder with typed request/response.
- Always type API responses matching the backend envelope: `{ success, data, error }`.
- Handle loading, error, and empty states in every data-fetching component.

```tsx
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}
```

## TypeScript Conventions

- **Strict mode** enabled in `tsconfig.json` (`strict: true`).
- Use `interface` for object shapes, `type` for unions and intersections.
- No `any` — use `unknown` and narrow types with type guards.
- Use discriminated unions for state machines and variant types.
- Max line length: 120 characters.
