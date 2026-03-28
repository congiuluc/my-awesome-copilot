---
name: accessibility
description: >-
  Build accessible UI components following WCAG 2.1 AA guidelines. Use when:
  ensuring keyboard navigation, adding ARIA attributes, fixing accessibility issues,
  building forms with error messages, creating modals with focus traps, implementing
  skip navigation, screen reader support, color contrast compliance, or reviewing
  components for a11y.
argument-hint: 'Describe the component or pattern that needs accessibility work.'
---

# Accessibility (WCAG 2.1 AA)

## When to Use

- Building or reviewing any interactive UI component
- Adding keyboard navigation (Tab, Enter, Space, Escape)
- Implementing ARIA attributes for dynamic content
- Creating forms with accessible labels, errors, and validation
- Building modals, dropdowns, or menus with focus traps
- Ensuring color contrast compliance
- Adding screen reader support and skip navigation
- Auditing existing components for accessibility issues

## Official Documentation

- [WCAG 2.1 Guidelines](https://www.w3.org/TR/WCAG21/)
- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [MDN Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility)
- [React Accessibility](https://react.dev/reference/react-dom/components#form-components)
- [ARIA Roles Reference](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles)
- [Color Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [axe DevTools](https://www.deque.com/axe/devtools/)

## Procedure

1. Load [semantic HTML patterns](./references/semantic-html.md)
2. Load [ARIA patterns reference](./references/aria-patterns.md)
3. Load [forms accessibility reference](./references/forms-accessibility.md)
4. Review [accessible component samples](./samples/accessible-components.tsx)
5. Use semantic HTML first — ARIA is a supplement, not a replacement
6. Ensure all interactive elements are keyboard navigable
7. Verify color contrast ratios (4.5:1 normal text, 3:1 large text)
8. Test with screen reader queries (getByRole, getByLabelText)
9. Validate with axe DevTools or Lighthouse accessibility audit

## Quick Reference Rules

| Rule | Requirement |
|------|------------|
| Every `<img>` | Must have `alt` (empty `alt=""` for decorative) |
| Every form input | Must have an associated `<label>` |
| Icon-only buttons | Must have `aria-label` |
| Dynamic content | Use `aria-live="polite"` or `role="alert"` |
| Modals/dialogs | Must trap focus, close on Escape |
| Color alone | Never use color as only indicator — add text/icons |
| Focus indicators | Must be visible: `focus:ring-2 focus:ring-offset-2` |
| Touch targets | Minimum 44×44px: `min-h-11 min-w-11` |
| Skip navigation | Include skip link as first focusable element |
| Page structure | Use landmarks: `<main>`, `<nav>`, `<header>`, `<footer>` |

## Completion Checklist

- [ ] All interactive elements reachable and operable via keyboard
- [ ] Focus indicators visible on all focusable elements
- [ ] Correct ARIA attributes on custom widgets
- [ ] Form inputs have labels, errors announced to screen readers
- [ ] Modals trap focus and close on Escape
- [ ] Color contrast ≥ 4.5:1 (normal text), ≥ 3:1 (large text)
- [ ] Images have appropriate alt text
- [ ] Skip navigation link present
- [ ] Landmark regions used (`<main>`, `<nav>`, `<header>`)
- [ ] Lighthouse accessibility score ≥ 90
