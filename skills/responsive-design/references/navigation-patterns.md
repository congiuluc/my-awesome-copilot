# Responsive Navigation Patterns

> Official reference: [WAI-ARIA Navigation (APG)](https://www.w3.org/WAI/ARIA/apg/patterns/navigation/)

## Responsive Header with Hamburger Menu

```tsx
import { useState } from 'react';

export interface NavLink {
  label: string;
  href: string;
}

export interface ResponsiveNavProps {
  brand: string;
  links: NavLink[];
  currentPath?: string;
}

export const ResponsiveNav = ({ brand, links, currentPath }: ResponsiveNavProps) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <nav aria-label="Main navigation" className="bg-white shadow-sm">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          {/* Brand */}
          <a href="/" className="text-xl font-bold text-gray-900">
            {brand}
          </a>

          {/* Desktop links */}
          <div className="hidden md:flex md:items-center md:gap-1">
            {links.map(link => (
              <a
                key={link.href}
                href={link.href}
                aria-current={currentPath === link.href ? 'page' : undefined}
                className={cn(
                  'rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  'hover:bg-gray-100 focus:ring-2 focus:ring-blue-500',
                  currentPath === link.href
                    ? 'text-blue-600 bg-blue-50'
                    : 'text-gray-700'
                )}
              >
                {link.label}
              </a>
            ))}
          </div>

          {/* Mobile hamburger — 44x44 touch target */}
          <button
            className="md:hidden inline-flex items-center justify-center min-h-11 min-w-11
                       rounded-md hover:bg-gray-100 focus:ring-2 focus:ring-blue-500"
            onClick={() => setIsOpen(!isOpen)}
            aria-expanded={isOpen}
            aria-controls="mobile-menu"
            aria-label={isOpen ? 'Close navigation menu' : 'Open navigation menu'}
          >
            {isOpen ? (
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            ) : (
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            )}
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      {isOpen && (
        <div id="mobile-menu" className="md:hidden border-t">
          <div className="flex flex-col px-4 py-2">
            {links.map(link => (
              <a
                key={link.href}
                href={link.href}
                aria-current={currentPath === link.href ? 'page' : undefined}
                className={cn(
                  'min-h-11 flex items-center rounded-md px-3 text-base font-medium',
                  'hover:bg-gray-100 focus:ring-2 focus:ring-blue-500',
                  currentPath === link.href
                    ? 'text-blue-600 bg-blue-50'
                    : 'text-gray-700'
                )}
              >
                {link.label}
              </a>
            ))}
          </div>
        </div>
      )}
    </nav>
  );
};
```

## Breadcrumbs

```tsx
export interface BreadcrumbItem {
  label: string;
  href?: string;
}

export const Breadcrumbs = ({ items }: { items: BreadcrumbItem[] }) => (
  <nav aria-label="Breadcrumb" className="py-2">
    <ol className="flex flex-wrap items-center gap-1 text-sm text-gray-500">
      {items.map((item, index) => (
        <li key={index} className="flex items-center gap-1">
          {index > 0 && <span aria-hidden="true" className="text-gray-300">/</span>}
          {item.href && index < items.length - 1 ? (
            <a href={item.href} className="hover:text-gray-700 hover:underline">
              {item.label}
            </a>
          ) : (
            <span aria-current="page" className="font-medium text-gray-900">
              {item.label}
            </span>
          )}
        </li>
      ))}
    </ol>
  </nav>
);
```

## Bottom Navigation (Mobile)

```tsx
export const BottomNav = ({ links, currentPath }: ResponsiveNavProps) => (
  <nav
    aria-label="Mobile navigation"
    className="fixed bottom-0 inset-x-0 z-40 bg-white border-t md:hidden"
  >
    <div className="flex justify-around">
      {links.slice(0, 5).map(link => (
        <a
          key={link.href}
          href={link.href}
          aria-current={currentPath === link.href ? 'page' : undefined}
          className={cn(
            'flex flex-col items-center min-h-14 min-w-14 justify-center px-2 py-1 text-xs',
            currentPath === link.href ? 'text-blue-600' : 'text-gray-500'
          )}
        >
          {/* Icon would go here */}
          <span className="mt-0.5">{link.label}</span>
        </a>
      ))}
    </div>
  </nav>
);
```

## Rules

- Hamburger button must have `aria-expanded` and `aria-controls`.
- Hamburger button touch target must be ≥ 44×44px.
- Current page indicated with `aria-current="page"`.
- Desktop nav links should have hover and focus states.
- Mobile menu items must be ≥ 44px tall for touch.
- Use `<nav aria-label="...">` to differentiate multiple navs.

## Official References

- [WAI-ARIA Navigation Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/navigation/)
- [MDN — nav Element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/nav)
- [WCAG 2.4.8 — Location](https://www.w3.org/TR/WCAG21/#location)
