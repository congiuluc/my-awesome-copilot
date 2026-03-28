---
name: responsive-design
description: >-
  Build responsive layouts and components using mobile-first CSS with TailwindCSS.
  Use when: creating page layouts, responsive grids, navigation menus, responsive
  images, fluid typography, touch-friendly interfaces, or implementing
  mobile/tablet/desktop breakpoints.
argument-hint: 'Describe the layout, page, or component that needs responsive design.'
---

# Responsive Design

## When to Use

- Creating page layouts that work across mobile, tablet, and desktop
- Building responsive navigation (hamburger menus, collapsible sidebars)
- Implementing responsive grids and card layouts
- Optimizing images and media for different screen sizes
- Making touch targets large enough for mobile interaction
- Adding fluid typography that scales with viewport
- Creating responsive forms and data tables

## Official Documentation

- [TailwindCSS Responsive Design](https://tailwindcss.com/docs/responsive-design)
- [MDN Responsive Design](https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design)
- [web.dev Responsive Design](https://web.dev/learn/design/)
- [Container Queries (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_container_queries)
- [Viewport Units (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/length#viewport-percentage_lengths)

## Procedure

1. Start mobile-first: write base styles for smallest screens
2. Layer breakpoints progressively — see [breakpoint system](./references/breakpoints.md)
3. Build layouts with [responsive patterns](./references/layout-patterns.md)
4. Make navigation responsive — see [navigation patterns](./references/navigation-patterns.md)
5. Optimize images and media — see [media patterns](./references/media-patterns.md)
6. Review [complete samples](./samples/responsive-layout-sample.tsx)
7. Test at all breakpoints: 320px, 640px, 768px, 1024px, 1280px, 1536px
8. Verify touch targets ≥ 44×44px on mobile

## Quick Reference — Tailwind Breakpoints

| Prefix | Min Width | Target Device |
|--------|-----------|---------------|
| (base) | 0px | Mobile phones (portrait) |
| `sm:` | 640px | Large phones (landscape) |
| `md:` | 768px | Tablets |
| `lg:` | 1024px | Laptops / small desktops |
| `xl:` | 1280px | Desktops |
| `2xl:` | 1536px | Large desktops |

## Completion Checklist

- [ ] Base styles target mobile (no prefix)
- [ ] Content readable at 320px width without horizontal scroll
- [ ] Grid columns adjust across breakpoints
- [ ] Navigation collapses on mobile with toggle button
- [ ] Touch targets ≥ 44×44px on mobile
- [ ] Images use `loading="lazy"` and responsive sizing
- [ ] Typography scales appropriately (not too large or small)
- [ ] No content is hidden or inaccessible on any viewport
- [ ] Tested on at least 3 breakpoints (320px, 768px, 1280px)
