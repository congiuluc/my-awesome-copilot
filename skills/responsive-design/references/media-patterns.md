# Responsive Media Patterns

> Official reference: [web.dev Responsive Images](https://web.dev/learn/design/responsive-images)

## Responsive Images

### Basic Lazy Loading

```tsx
<img
  src="/images/photo.jpg"
  alt="Description of the image"
  className="w-full h-auto rounded"
  loading="lazy"
  decoding="async"
/>
```

### Aspect Ratio Containers

```tsx
// 16:9 video/image container
<div className="aspect-video overflow-hidden rounded-lg">
  <img
    src="/images/hero.jpg"
    alt="Hero banner"
    className="h-full w-full object-cover"
    loading="lazy"
    decoding="async"
  />
</div>

// Square (1:1)
<div className="aspect-square overflow-hidden rounded-full">
  <img src={avatar} alt={`${name}'s avatar`} className="h-full w-full object-cover" />
</div>

// 4:3
<div className="aspect-[4/3] overflow-hidden rounded">
  <img src={photo} alt={description} className="h-full w-full object-cover" />
</div>
```

### srcSet and sizes

For art direction and resolution switching:

```tsx
<img
  src="/images/photo-800.jpg"
  srcSet="/images/photo-400.jpg 400w, /images/photo-800.jpg 800w, /images/photo-1200.jpg 1200w"
  sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
  alt="Product photo"
  loading="lazy"
  decoding="async"
  className="w-full h-auto rounded"
/>
```

### Art Direction with `<picture>`

```tsx
<picture>
  {/* Desktop: wide crop */}
  <source media="(min-width: 1024px)" srcSet="/images/hero-wide.jpg" />
  {/* Tablet: medium crop */}
  <source media="(min-width: 640px)" srcSet="/images/hero-medium.jpg" />
  {/* Mobile: tall crop (default) */}
  <img
    src="/images/hero-mobile.jpg"
    alt="Hero banner"
    className="w-full h-auto"
    loading="lazy"
    decoding="async"
  />
</picture>
```

## Responsive Video

```tsx
// Responsive embed (YouTube, Vimeo, etc.)
<div className="aspect-video overflow-hidden rounded-lg">
  <iframe
    src="https://www.youtube-nocookie.com/embed/VIDEO_ID"
    title="Video title for accessibility"
    className="h-full w-full"
    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
    allowFullScreen
    loading="lazy"
  />
</div>
```

## Fluid Typography

Using `clamp()` for typography that scales with viewport:

```css
/* In app.css with TailwindCSS v4 */
@theme {
  --font-size-fluid-sm: clamp(0.75rem, 1.5vw, 0.875rem);
  --font-size-fluid-base: clamp(0.875rem, 2vw, 1rem);
  --font-size-fluid-lg: clamp(1.125rem, 2.5vw, 1.25rem);
  --font-size-fluid-xl: clamp(1.25rem, 3vw, 1.5rem);
  --font-size-fluid-2xl: clamp(1.5rem, 4vw, 2.25rem);
  --font-size-fluid-3xl: clamp(1.875rem, 5vw, 3rem);
}
```

Usage:

```tsx
<h1 className="text-[length:var(--font-size-fluid-3xl)] font-bold">
  Page Title
</h1>
```

Or with standard Tailwind responsive classes:

```tsx
<h1 className="text-2xl sm:text-3xl lg:text-4xl xl:text-5xl font-bold">
  Page Title
</h1>
```

## useMediaQuery Hook

For JavaScript-driven responsive behavior:

```tsx
import { useState, useEffect } from 'react';

export const useMediaQuery = (query: string): boolean => {
  const [matches, setMatches] = useState(
    () => typeof window !== 'undefined' && window.matchMedia(query).matches
  );

  useEffect(() => {
    const mql = window.matchMedia(query);
    const handler = (e: MediaQueryListEvent) => setMatches(e.matches);
    mql.addEventListener('change', handler);
    return () => mql.removeEventListener('change', handler);
  }, [query]);

  return matches;
};

// Preset hooks
export const useIsMobile = () => useMediaQuery('(max-width: 767px)');
export const useIsTablet = () => useMediaQuery('(min-width: 768px) and (max-width: 1023px)');
export const useIsDesktop = () => useMediaQuery('(min-width: 1024px)');
```

## Touch Targets

All interactive elements on mobile must be ≥ 44×44px:

```tsx
// Button — touch-safe
<button className="min-h-11 min-w-11 px-4 py-2 rounded-md">
  Action
</button>

// Link in a list — touch-safe
<a href="/page" className="min-h-11 flex items-center px-3 py-2 rounded-md">
  Link text
</a>

// Icon button — touch-safe
<button aria-label="Settings" className="min-h-11 min-w-11 flex items-center justify-center rounded-full">
  <svg className="h-5 w-5" aria-hidden="true">{/* ... */}</svg>
</button>
```

## Official References

- [web.dev Responsive Images](https://web.dev/learn/design/responsive-images)
- [MDN Responsive Images](https://developer.mozilla.org/en-US/docs/Learn/HTML/Multimedia_and_embedding/Responsive_images)
- [MDN picture Element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/picture)
- [TailwindCSS Aspect Ratio](https://tailwindcss.com/docs/aspect-ratio)
- [WCAG 1.4.4 — Resize Text](https://www.w3.org/TR/WCAG21/#resize-text)
