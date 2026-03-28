---
description: "Use when building shared UI components with TailwindCSS v4 and React 19. Covers component API patterns, accessibility, design tokens, and the cn() utility."
applyTo: "src/web-app/src/components/ui/**,src/web-app/src/components/shared/**"
---
# TailwindCSS Component Guidelines

## Component API

- Use **named exports only** — no default exports.
- Props interfaces suffixed with `Props` (e.g., `ButtonProps`, `CardProps`).
- Extend native HTML element props for proper attribute forwarding.
- Use `React.forwardRef` for components that need ref access.

## Class Merging

- Use the `cn()` utility (clsx + twMerge) for conditional and overridable classes:
  ```tsx
  import { cn } from '@/lib/utils';
  
  export function Button({ className, variant = 'primary', ...props }: ButtonProps) {
      return (
          <button
              className={cn(
                  'px-4 py-2 rounded-lg font-medium transition-colors',
                  variant === 'primary' && 'bg-blue-600 text-white hover:bg-blue-700',
                  variant === 'secondary' && 'bg-gray-200 text-gray-800 hover:bg-gray-300',
                  className
              )}
              {...props}
          />
      );
  }
  ```
- Always accept `className` prop for consumer overrides.

## Design Tokens

- Define theme tokens in `app.css` using TailwindCSS v4 `@theme` — avoid `tailwind.config.ts`.
- Use CSS custom properties for colors, spacing, and typography.

## Accessibility

- All interactive elements: minimum **44×44px** touch targets on mobile.
- Buttons need accessible labels (`aria-label` for icon-only buttons).
- Modals need focus traps and Escape key dismissal.
- Inputs need associated `<label>` or `aria-label`.

## Responsiveness

- Components are **mobile-first** by default.
- Use Tailwind responsive prefixes: `sm:`, `md:`, `lg:`, `xl:`.
- Test at 375px, 768px, and 1280px breakpoints.
