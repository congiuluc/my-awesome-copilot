# Frontend Project Structure

## Stack Versions

- **React 19** with functional components only.
- **Vite** as build tool with TypeScript strict mode enabled.
- **TailwindCSS v4** for all styling — no CSS modules, no styled-components.
- **Node.js 22 LTS** runtime.

## Directory Layout

```
src/{frontend-app}/
  src/
    components/
      shared/         # Reusable UI: Button, Modal, Card, Input, Toast, Spinner
      layout/         # Layout: Header, Footer, Sidebar, PageContainer
    features/
      {feature-a}/    # Feature: components, hooks, types
      {feature-b}/
      auth/           # Feature: LoginForm, RegisterForm, AuthProvider, hooks
    hooks/            # Shared custom hooks: useApi, useDebounce, useMediaQuery
    services/         # API client functions
    types/            # Shared TypeScript types/interfaces: api.ts, models.ts
    utils/            # Pure utility functions: cn, formatDate, validators
    lib/              # Third-party integrations and wrappers
    App.tsx
    main.tsx
    app.css           # TailwindCSS imports and @theme config
  public/             # Static assets
  index.html
  vite.config.ts
  tsconfig.json
```

## Feature Folder Convention

Each feature folder is self-contained:

```
features/{feature-name}/
  components/
    ItemCard.tsx
    ItemList.tsx
    ItemDetail.tsx
    CreateItemForm.tsx
  hooks/
    useItems.ts
    useItem.ts
  types/
    item.types.ts
  index.ts              # Barrel export for public API
```

## Barrel Exports

Use `index.ts` to re-export public components/hooks from feature folders:

```tsx
// features/{feature}/index.ts
export { ItemCard } from './components/ItemCard';
export { ItemList } from './components/ItemList';
export { useItems } from './hooks/useItems';
```

Import from feature folder, never from deep paths outside the feature:

```tsx
// ✅ Correct
import { ItemCard } from '@/features/{feature}';

// ❌ Wrong (from outside the feature)
import { ItemCard } from '@/features/{feature}/components/ItemCard';
```
