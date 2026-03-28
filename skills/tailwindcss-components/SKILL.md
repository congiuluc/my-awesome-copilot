---
name: tailwindcss-components
description: >-
  Provide ready-to-use TailwindCSS v4 components for React 19 + TypeScript.
  Includes Button, Card, Input, Modal, Badge, Toast, Spinner, and DataList
  (with search, sorting, and pagination). Use when: building UI components,
  creating forms, adding modals, implementing data tables, lists with search
  and paging, or composing layouts with TailwindCSS.
argument-hint: 'Which component do you need? Button, Card, Input, Modal, Badge, Toast, Spinner, DataList, or custom.'
---

# TailwindCSS v4 Component Library

## When to Use

- Building shared UI components with TailwindCSS v4 and React 19
- Creating buttons, cards, inputs, modals, badges, toasts, or spinners
- Implementing data lists with search, column sorting, and pagination
- Composing consistent, accessible UI across features
- Setting up TailwindCSS v4 with custom theme tokens

## Official Documentation

- [TailwindCSS v4](https://tailwindcss.com/docs)
- [TailwindCSS v4 Installation](https://tailwindcss.com/docs/installation/using-vite)
- [TailwindCSS Theme Configuration](https://tailwindcss.com/docs/theme)
- [React 19](https://react.dev/reference/react)
- [WAI-ARIA Practices](https://www.w3.org/WAI/ARIA/apg/)

## Procedure

1. Load [component patterns reference](./references/component-patterns.md)
2. Load [TailwindCSS setup reference](./references/tailwind-setup.md)
3. Review standard components: [Button, Card, Input, Badge, Toast, Spinner](./samples/shared-components.tsx)
4. Review [Modal component](./samples/modal-sample.tsx) for dialog pattern
5. Review [DataList component](./samples/data-list.tsx) for search, sorting, and pagination
6. Choose the component(s) needed and adapt to feature requirements
7. Ensure all components follow accessibility guidelines (ARIA, keyboard navigation)

## Component Inventory

| Component | File | Features |
|-----------|------|----------|
| `Button` | shared/Button.tsx | Variants, sizes, loading state, icons |
| `Card` | shared/Card.tsx | Header, body, footer, clickable variant |
| `Input` | shared/Input.tsx | Label, error, helper text, icons |
| `Badge` | shared/Badge.tsx | Color variants, removable |
| `Toast` | shared/Toast.tsx | Success/error/warning/info, auto-dismiss |
| `Spinner` | shared/Spinner.tsx | Size variants, accessible label |
| `Modal` | shared/Modal.tsx | Focus trap, Escape to close, portal |
| `DataList` | shared/DataList.tsx | Search, column sorting, pagination |

## Rules

- All components must be **accessible**: correct ARIA roles, keyboard navigation, focus management.
- Use **named exports** only — no default exports.
- Props interfaces suffixed with `Props` (e.g., `ButtonProps`).
- Use `cn()` utility (clsx + twMerge) for conditional class merging.
- All interactive elements must have minimum **44×44px** touch targets on mobile.
- Use TailwindCSS v4 `@theme` in `app.css` for design tokens — avoid `tailwind.config.ts`.
- Components must be responsive by default (mobile-first).
