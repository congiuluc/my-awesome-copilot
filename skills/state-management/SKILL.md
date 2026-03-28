---
name: state-management
description: "Manage frontend state in React 19 applications. Covers local state, Context API, server state with TanStack Query, URL state, and state composition patterns. Use when: choosing a state management approach, implementing data fetching with caching, managing global UI state, or optimizing re-renders."
argument-hint: 'Describe the state requirement: global state, server state, form state, or caching pattern.'
---

# Frontend State Management

## When to Use

- Choosing a state management approach for a new feature
- Setting up TanStack Query for server state
- Managing global UI state (theme, sidebar, modals)
- Implementing form state with validation
- Optimizing unnecessary re-renders from state changes
- Sharing state across components without prop drilling

## Official Documentation

- [React State](https://react.dev/learn/managing-state)
- [TanStack Query](https://tanstack.com/query/latest)
- [React Context](https://react.dev/reference/react/useContext)

## Key Principles

- **Server state ≠ client state** — treat them separately.
- **Collocate state** — keep state as close to where it's used as possible.
- **URL is state** — search params, filters, pagination belong in the URL.
- **Derive, don't duplicate** — compute values from existing state instead of storing copies.

## State Categories

| Category | Tool | Examples |
|----------|------|---------|
| **Local UI** | `useState`, `useReducer` | Form inputs, open/close, selection |
| **Server** | TanStack Query | API data, pagination, caching |
| **Global UI** | Context + `useReducer` | Theme, auth status, sidebar |
| **URL** | `useSearchParams`, router | Filters, sort, page number |
| **Form** | React Hook Form or controlled | Validation, submission, field state |

## Procedure

### 1. Server State (TanStack Query)

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

export function useProducts() {
    return useQuery({
        queryKey: ['products'],
        queryFn: () => api.get<Product[]>('/api/products'),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
}

export function useCreateProduct() {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (data: CreateProductRequest) => api.post('/api/products', data),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
    });
}
```

- Use `queryKey` arrays for cache identity.
- Set `staleTime` based on data freshness requirements.
- Invalidate queries after mutations.

### 2. Global UI State (Context)

```tsx
const ThemeContext = createContext<ThemeContextValue | null>(null);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
    const [theme, setTheme] = useState<'light' | 'dark'>('dark');
    const value = useMemo(() => ({ theme, setTheme }), [theme]);
    return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
    const context = useContext(ThemeContext);
    if (!context) throw new Error('useTheme must be used within ThemeProvider');
    return context;
}
```

- Wrap provider value in `useMemo` to prevent unnecessary re-renders.
- Create a custom hook that validates the context exists.
- Keep Context for truly global state — not for every shared value.

### 3. URL State

```tsx
const [searchParams, setSearchParams] = useSearchParams();
const page = Number(searchParams.get('page')) || 1;
const sort = searchParams.get('sort') || 'name';
```

- Filters, pagination, and sort order belong in the URL.
- This enables shareable links and browser back/forward support.

## Anti-Patterns

- ❌ Using Context for server data (use TanStack Query instead)
- ❌ Storing derived data in state (compute it in render)
- ❌ Global state for everything (use local state first)
- ❌ Prop drilling more than 2 levels (use Context or composition)
- ❌ Not setting `staleTime` (causes unnecessary refetches)
