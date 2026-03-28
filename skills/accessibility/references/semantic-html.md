# Semantic HTML Patterns

## Element Selection

| Need | Use | Not |
|------|-----|-----|
| Clickable action | `<button>` | `<div onClick>` |
| Navigation link | `<a href>` | `<span onClick>` |
| Page section | `<section>`, `<article>` | `<div>` |
| Navigation bar | `<nav aria-label>` | `<div class="nav">` |
| Page header | `<header>` | `<div class="header">` |
| Main content | `<main>` | `<div class="main">` |
| Page footer | `<footer>` | `<div class="footer">` |
| Form field | `<input>` + `<label>` | `<input placeholder="Name">` |
| List of items | `<ul>` / `<ol>` | `<div>` with child `<div>`s |
| Data table | `<table>` with `<th>` | CSS grid for tabular data |
| Heading hierarchy | `<h1>` → `<h2>` → `<h3>` | Skipping levels or styling-only |

## Keyboard Navigation Rules

- All interactive elements must work with **Tab**, **Enter**, **Space**, **Escape**.
- Use `tabIndex={0}` only for custom interactive elements that are not natively focusable.
- Use `tabIndex={-1}` to make elements programmatically focusable (e.g., modal container).
- Never use `tabIndex` greater than 0.
- Implement focus traps for modals and dropdowns.
- Visible focus indicators are mandatory: `focus:ring-2 focus:ring-offset-2 focus:ring-blue-500`.

## Skip Navigation Link

Include as the first focusable element on every page:

```tsx
<a
  href="#main-content"
  className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50
             focus:bg-white focus:px-4 focus:py-2 focus:rounded focus:shadow-lg"
>
  Skip to main content
</a>

<main id="main-content">
  {/* Page content */}
</main>
```

## Page Landmark Structure

```tsx
<body>
  {/* Skip link (first) */}
  <a href="#main-content" className="sr-only focus:not-sr-only ...">Skip to content</a>

  <header>
    <nav aria-label="Main navigation">{/* Primary nav */}</nav>
  </header>

  <main id="main-content">
    <h1>Page Title</h1>
    <section aria-labelledby="section-heading">
      <h2 id="section-heading">Section</h2>
      {/* Content */}
    </section>
  </main>

  <aside aria-label="Sidebar">{/* Side content */}</aside>

  <footer>{/* Footer content */}</footer>
</body>
```

## Images

```tsx
// Informative — descriptive alt text
<img src={photo} alt="Team celebrating product launch at the office" />

// Decorative — empty alt, hidden from assistive tech
<img src={decoration} alt="" aria-hidden="true" />

// Complex — extended description via figcaption
<figure>
  <img src={chart} alt="Bar chart showing sales by quarter" />
  <figcaption>Q3 had the highest sales at $1.2M, a 30% increase over Q2.</figcaption>
</figure>
```

## Headings

- One `<h1>` per page.
- Never skip heading levels (h1 → h3 without h2).
- Headings should describe the content structure, not visual size.
- Use CSS for visual sizing, semantic tags for structure.

## Official References

- [HTML Living Standard — Semantics](https://html.spec.whatwg.org/multipage/semantics.html)
- [WAI — Using Semantic HTML](https://www.w3.org/WAI/tutorials/)
- [MDN — HTML Elements Reference](https://developer.mozilla.org/en-US/docs/Web/HTML/Element)
