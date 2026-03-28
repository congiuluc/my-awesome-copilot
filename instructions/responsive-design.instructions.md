---
description: "Use when building page layouts, grids, navigation structures, or responsive containers. Covers mobile-first design, TailwindCSS breakpoints, touch targets, responsive images, and fluid typography."
applyTo: "src/web-app/src/components/**,src/web-app/src/features/**,src/web-app/src/layouts/**"
---
# Responsive Design Guidelines

## Mobile-First Approach

Always write base styles for mobile, then layer on larger breakpoints:

```tsx
<div className="px-4 py-2 md:px-8 md:py-4 lg:px-12">
  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
    {/* Cards */}
  </div>
</div>
```

## Breakpoints (Tailwind defaults)

| Prefix | Min Width | Target |
|--------|-----------|--------|
| (base) | 0px | Mobile phones |
| `sm:` | 640px | Large phones |
| `md:` | 768px | Tablets |
| `lg:` | 1024px | Laptops |
| `xl:` | 1280px | Desktops |
| `2xl:` | 1536px | Large desktops |

## Touch Targets

- Minimum 44×44px for all interactive elements on mobile.
- Use `min-h-11 min-w-11` (44px) on buttons and links for touch.
- Add sufficient spacing between adjacent tap targets.

## Responsive Container

```tsx
<div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
  {/* Page content */}
</div>
```

## Responsive Navigation

- Desktop: horizontal link bar.
- Mobile: hamburger toggle with collapsible menu.
- Hamburger button must have `aria-expanded` and `aria-controls`.
- Mobile menu items must be ≥ 44px tall.

## Responsive Images

```tsx
<img
  src="/images/photo.jpg"
  alt="Description"
  className="w-full h-auto rounded"
  loading="lazy"
  decoding="async"
/>
```

Use `aspect-video` or `aspect-square` for consistent aspect ratios.

## Responsive Typography

```tsx
<h1 className="text-2xl sm:text-3xl lg:text-4xl font-bold">
  Page Title
</h1>
```

## Responsive Tables

Wrap tables in a scrollable container on mobile:

```tsx
<div className="overflow-x-auto -mx-4 sm:mx-0">
  <table className="min-w-full divide-y divide-gray-200">
    {/* Table content */}
  </table>
</div>
```

## Testing Responsiveness

- Test at breakpoints: 320px, 640px, 768px, 1024px, 1280px.
- No horizontal scrollbar at any breakpoint.
- Content must not be cut off or overlapping.
