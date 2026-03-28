---
description: "Use when building UI components, forms, modals, or interactive elements. Covers WCAG 2.1 AA compliance, semantic HTML, ARIA attributes, keyboard navigation, screen reader support, and focus management."
applyTo: "src/web-app/src/components/**,src/web-app/src/features/**"
---
# Accessibility Guidelines (WCAG 2.1 AA)

## Semantic HTML

Use the correct element for the job — never style a `<div>` as a button:

```tsx
// ✅ Correct
<button onClick={handleClick}>Submit</button>
<a href="/about">About</a>
<nav aria-label="Main navigation">{/* links */}</nav>

// ❌ Wrong
<div onClick={handleClick}>Submit</div>
<span className="cursor-pointer" onClick={navigate}>About</span>
```

## Keyboard Navigation

- All interactive elements must work with Tab, Enter, Space, Escape.
- Implement focus traps for modals and dropdowns.
- Use `tabIndex={0}` only for custom interactive elements.
- Visible focus indicators: `focus:outline-2 focus:outline-offset-2 focus:outline-blue-600`.
- Restore focus to trigger element when modal/popover closes.

## ARIA Attributes

| Situation | Attribute |
|-----------|-----------|
| Icon-only button | `aria-label="Close dialog"` |
| Loading state | `aria-busy="true"` |
| Dynamic content update | `aria-live="polite"` |
| Error message | `role="alert"` |
| Expanded/collapsed | `aria-expanded="true/false"` |
| Current page in nav | `aria-current="page"` |

## Color & Contrast

- Text contrast ratio: ≥ 4.5:1 (normal text), ≥ 3:1 (large text 18px+).
- Never convey information through color alone — add icons, text, or patterns.

## Images

```tsx
// Informative image — descriptive alt
<img src={photo} alt="Person smiling at the camera" />

// Decorative image — empty alt + aria-hidden
<img src={decoration} alt="" aria-hidden="true" />
```

## Forms

Every input must have a visible label:

```tsx
<div>
  <label htmlFor="email" className="block text-sm font-medium">
    Email address
  </label>
  <input
    id="email"
    type="email"
    required
    aria-describedby="email-error"
    className="mt-1 block w-full rounded border-gray-300"
  />
  {error && (
    <p id="email-error" role="alert" className="mt-1 text-sm text-red-600">
      {error}
    </p>
  )}
</div>
```

## Skip Navigation

Include a skip link as the first focusable element:

```tsx
<a href="#main-content" className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2">
  Skip to main content
</a>
```

## Testing Accessibility

- Use `@testing-library/jest-dom` matchers: `toBeVisible()`, `toHaveAccessibleName()`.
- Query by role: `screen.getByRole('button', { name: 'Submit' })`.
- Verify keyboard interaction with `userEvent.tab()` and `userEvent.keyboard('{Enter}')`.
