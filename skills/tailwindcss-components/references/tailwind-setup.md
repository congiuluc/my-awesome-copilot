# TailwindCSS v4 Setup

## Installation (Vite + React)

```bash
npm install tailwindcss @tailwindcss/vite
```

Configure Vite plugin:

```ts
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { '@': '/src' },
  },
});
```

## CSS Entry Point

```css
/* app.css */
@import 'tailwindcss';

@theme {
  /* Color tokens */
  --color-primary: #2563eb;
  --color-primary-hover: #1d4ed8;
  --color-secondary: #4b5563;
  --color-danger: #dc2626;
  --color-success: #16a34a;
  --color-warning: #d97706;

  /* Spacing / sizing */
  --spacing-touch: 2.75rem; /* 44px — minimum touch target */

  /* Typography */
  --font-size-heading: clamp(1.5rem, 4vw, 2.5rem);
  --font-size-body: clamp(0.875rem, 2vw, 1rem);

  /* Shadows */
  --shadow-card: 0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1);
  --shadow-modal: 0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1);

  /* Border radius */
  --radius-default: 0.5rem;
  --radius-full: 9999px;
}
```

Import in `main.tsx`:

```tsx
import './app.css';
```

## TailwindCSS v4 Key Changes

- **No `tailwind.config.ts`** — use `@theme` in CSS for customization.
- **No `@tailwind` directives** — use `@import 'tailwindcss'`.
- **CSS-first configuration** — all tokens defined via CSS custom properties.
- Automatic content detection — no `content` array needed.
- Oxide engine — faster compilation.

## PostCSS (if not using Vite plugin)

```bash
npm install tailwindcss @tailwindcss/postcss postcss
```

```js
// postcss.config.js
export default {
  plugins: {
    '@tailwindcss/postcss': {},
  },
};
```

## Dark Mode

TailwindCSS v4 uses `prefers-color-scheme` by default:

```css
@theme {
  --color-bg: #ffffff;
  --color-text: #111827;
}

@media (prefers-color-scheme: dark) {
  @theme {
    --color-bg: #111827;
    --color-text: #f9fafb;
  }
}
```

Or use the `dark:` variant with a class strategy:

```css
@import 'tailwindcss';
@variant dark (&:where(.dark, .dark *));
```
