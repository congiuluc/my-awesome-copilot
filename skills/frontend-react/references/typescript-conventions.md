# TypeScript Conventions

## Strict Mode

Enable all strict checks in `tsconfig.json`:

```json
{
  "compilerOptions": {
    "strict": true,
    "noUncheckedIndexedAccess": true,
    "noImplicitOverride": true,
    "forceConsistentCasingInFileNames": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    }
  }
}
```

## Types vs Interfaces

- Use `interface` for object shapes (extendable).
- Use `type` for unions, intersections, mapped types, and utility types.

```tsx
// Interface for object shapes
export interface ProductDto {
  id: string;
  name: string;
  description: string | null;
  drawDate: string;
  createdAtUtc: string;
}

// Type for unions
export type ProductStatus = 'draft' | 'active' | 'drawn' | 'cancelled';

// Type for discriminated unions
export type AsyncState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'error'; error: string };
```

## API Response Type

Match the backend envelope exactly:

```tsx
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}
```

## Rules

- No `any` — use `unknown` and narrow types with type guards.
- No type assertions (`as`) except when interfacing with untyped libraries — add a comment explaining why.
- Use discriminated unions for state machines and variant types.
- Use `satisfies` operator for type validation without widening.
- Prefer `readonly` arrays and properties for immutable data.
- Max line length: 120 characters.
- Import types with `import type` syntax.

```tsx
import type { ProductDto } from '@/features/products/types/product.types';
```

## Utility Patterns

```tsx
// cn() helper for conditional classes
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export const cn = (...inputs: ClassValue[]): string => {
  return twMerge(clsx(inputs));
};
```
