# Component Patterns

## cn() Utility

All components use a `cn()` utility for conditional class merging:

```tsx
// utils/cn.ts
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export const cn = (...inputs: ClassValue[]) => twMerge(clsx(inputs));
```

Install dependencies:

```bash
npm install clsx tailwind-merge
```

## Component Structure

```
components/
  shared/
    Button.tsx
    Card.tsx
    Input.tsx
    Badge.tsx
    Toast.tsx
    Spinner.tsx
    Modal.tsx
    DataList.tsx
    index.ts          # Barrel export
```

Barrel export:

```tsx
// components/shared/index.ts
export { Button } from './Button';
export { Card } from './Card';
export { Input } from './Input';
export { Badge } from './Badge';
export { Toast } from './Toast';
export { Spinner } from './Spinner';
export { Modal } from './Modal';
export { DataList } from './DataList';
```

## Props Pattern

```tsx
export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  icon?: React.ReactNode;
}
```

Rules:
- Extend native HTML element attributes for proper type inference.
- Destructure known props, spread the rest to the root element.
- Use `React.forwardRef` when refs need to be passed through.

## Variant Mapping Pattern

```tsx
const variants = {
  primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
  secondary: 'bg-gray-600 text-white hover:bg-gray-700 focus:ring-gray-500',
  outline: 'border border-gray-300 text-gray-700 hover:bg-gray-50 focus:ring-blue-500',
  ghost: 'text-gray-700 hover:bg-gray-100 focus:ring-blue-500',
  danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
} as const;

const sizes = {
  sm: 'px-3 py-1.5 text-sm min-h-9',
  md: 'px-4 py-2 text-base min-h-11',
  lg: 'px-6 py-3 text-lg min-h-12',
} as const;
```

## Focus Ring Pattern

All interactive components use consistent focus rings:

```tsx
className="focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
```

## Disabled Pattern

```tsx
className={cn(
  'transition-colors',
  disabled && 'cursor-not-allowed opacity-50'
)}
aria-disabled={disabled}
```

## Responsive Design Rules

- Mobile-first: base styles for mobile, then `sm:`, `md:`, `lg:` breakpoints.
- Touch targets: `min-h-11 min-w-11` (44px) on all interactive elements.
- Spacing: `gap-4` (mobile), `gap-6` (tablet+).
- Typography: use `clamp()` for fluid text sizing.
