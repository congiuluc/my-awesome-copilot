---
description: "Use when managing frontend state: TanStack Query for server state, Context for global UI, URL state for filters/pagination, and state composition patterns."
applyTo: "src/web-app/src/hooks/**,src/web-app/src/context/**,src/web-app/src/providers/**"
---
# State Management Guidelines

## State Categories

| Category | Tool | When |
|----------|------|------|
| **Server state** | TanStack Query | API data, caching, pagination |
| **Local UI** | `useState` / `useReducer` | Form inputs, open/close, selection |
| **Global UI** | Context + `useReducer` | Theme, auth status, sidebar |
| **URL state** | `useSearchParams` | Filters, sort, page number |

## Server State (TanStack Query)

- Use `queryKey` arrays for cache identity (e.g., `['products', { page, sort }]`).
- Set `staleTime` based on data freshness needs — don't leave it at 0.
- Invalidate queries after mutations: `queryClient.invalidateQueries({ queryKey: ['products'] })`.
- Extract queries into custom hooks: `useProducts()`, `useCreateProduct()`.

## Context

- Use Context for **truly global** state only (theme, auth, sidebar).
- Wrap provider value in `useMemo` to prevent re-renders.
- Always create a custom hook with null check: `useTheme()`.
- Never use Context for server data — use TanStack Query.

## URL State

- Filters, pagination, and sort order belong in the URL.
- Use `useSearchParams` for read/write.
- This enables shareable links and browser navigation.

## Rules

- Collocate state — keep it as close to usage as possible.
- Derive, don't duplicate — compute values from existing state.
- Don't use global state when local state suffices.
