# Breakpoint System

> Official reference: [TailwindCSS Responsive Design](https://tailwindcss.com/docs/responsive-design)

## Mobile-First Principle

**Always write base styles for mobile, then add breakpoint prefixes for larger screens.**

TailwindCSS breakpoints are **min-width** — they apply at that width **and above**:

```tsx
// Base (mobile) → sm (640px+) → md (768px+) → lg (1024px+) → xl (1280px+)
<div className="text-sm md:text-base lg:text-lg">
  This text grows with the viewport.
</div>
```

## Breakpoint Reference

| Prefix | Min Width | CSS | Typical Target |
|--------|-----------|-----|----------------|
| (none) | 0px | Default styles | Small phones (320px–639px) |
| `sm:` | 640px | `@media (min-width: 640px)` | Large phones landscape |
| `md:` | 768px | `@media (min-width: 768px)` | Tablets |
| `lg:` | 1024px | `@media (min-width: 1024px)` | Laptops |
| `xl:` | 1280px | `@media (min-width: 1280px)` | Desktops |
| `2xl:` | 1536px | `@media (min-width: 1536px)` | Wide desktops |

## Common Patterns

### Grid Columns

```tsx
// 1 column on mobile → 2 on tablet → 3 on laptop → 4 on desktop
<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 md:gap-6">
  {items.map(item => <Card key={item.id} {...item} />)}
</div>
```

### Stack to Row

```tsx
// Stacked on mobile → side by side on tablet
<div className="flex flex-col md:flex-row gap-4">
  <div className="flex-1">{/* Main content */}</div>
  <div className="w-full md:w-80">{/* Sidebar */}</div>
</div>
```

### Show / Hide

```tsx
// Hidden on mobile, visible on desktop
<div className="hidden lg:block">Desktop sidebar</div>

// Visible on mobile, hidden on desktop
<button className="lg:hidden">Open mobile menu</button>
```

### Responsive Padding

```tsx
// Tighter on mobile, more spacious on desktop
<div className="px-4 sm:px-6 lg:px-8 py-4 md:py-8">
  {/* Content */}
</div>
```

## Container

```tsx
// Centered max-width container with responsive padding
<div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
  {/* Page content */}
</div>
```

## Custom Breakpoints (TailwindCSS v4)

In `app.css` using TailwindCSS v4's CSS-first config:

```css
@theme {
  --breakpoint-xs: 475px;
  --breakpoint-3xl: 1800px;
}
```

Then use: `xs:text-base 3xl:text-xl`.

## Container Queries

For component-level responsiveness (independent of viewport):

```css
/* app.css */
.card-container {
  container-type: inline-size;
  container-name: card;
}

@container card (min-width: 400px) {
  .card-layout {
    display: grid;
    grid-template-columns: 200px 1fr;
  }
}
```

Or with Tailwind's `@container` plugin:

```tsx
<div className="@container">
  <div className="flex flex-col @md:flex-row gap-4">
    {/* Responds to container width, not viewport */}
  </div>
</div>
```

## Official References

- [TailwindCSS Responsive Design](https://tailwindcss.com/docs/responsive-design)
- [TailwindCSS Container Queries](https://tailwindcss.com/docs/container-queries)
- [MDN Using Media Queries](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_media_queries/Using_media_queries)
