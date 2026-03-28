# ARIA Patterns Reference

> Official reference: [WAI-ARIA Authoring Practices Guide](https://www.w3.org/WAI/ARIA/apg/)

## ARIA Roles Quick Reference

### Landmark Roles

| Role | HTML Equivalent | Use Case |
|------|----------------|----------|
| `banner` | `<header>` | Site-wide header with logo/nav |
| `navigation` | `<nav>` | Navigation links |
| `main` | `<main>` | Primary content area |
| `complementary` | `<aside>` | Supporting content |
| `contentinfo` | `<footer>` | Site-wide footer |
| `search` | `<search>` | Search functionality |
| `region` | `<section>` | Named landmark via `aria-labelledby` |

**Rule**: Prefer native HTML elements over ARIA roles. Only add roles when using generic elements like `<div>`.

### Widget Roles

| Role | When to Use |
|------|------------|
| `alert` | Urgent messages (errors, warnings) — announced immediately |
| `alertdialog` | Alert that requires user response |
| `dialog` | Modal dialog window |
| `status` | Advisory info (e.g., "Saved", "3 results found") |
| `tab`, `tabpanel`, `tablist` | Tabbed interfaces |
| `tooltip` | Popup description on hover/focus |
| `menu`, `menuitem` | Application-style menus (not page navigation) |

## ARIA States & Properties

### Commonly Used

```tsx
// Dynamic visibility
aria-expanded="true"         // Dropdown/accordion is open
aria-hidden="true"           // Hidden from assistive tech (decorative)

// Form states
aria-required="true"         // Field is required
aria-invalid="true"          // Field has validation error
aria-describedby="error-id"  // Links to error/help text

// Loading states
aria-busy="true"             // Content is updating
aria-live="polite"           // Announce changes when idle
aria-live="assertive"        // Announce changes immediately (errors)

// Selection
aria-selected="true"         // Selected tab, option, or item
aria-checked="true"          // Checkbox or toggle state
aria-pressed="true"          // Toggle button pressed state

// Relationships
aria-labelledby="heading-id" // Label from another element
aria-controls="panel-id"     // This element controls another
aria-owns="list-id"          // DOM ownership (e.g., portals)
```

## Common Widget Patterns

### Disclosure (Accordion)

```tsx
export interface DisclosureProps {
  title: string;
  children: React.ReactNode;
}

export const Disclosure = ({ title, children }: DisclosureProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const contentId = useId();

  return (
    <div>
      <button
        aria-expanded={isOpen}
        aria-controls={contentId}
        onClick={() => setIsOpen(!isOpen)}
        className="flex w-full items-center justify-between p-4 text-left
                   font-medium hover:bg-gray-50 focus:ring-2 focus:ring-blue-500"
      >
        {title}
        <ChevronIcon className={cn('transition-transform', isOpen && 'rotate-180')} />
      </button>
      <div
        id={contentId}
        role="region"
        aria-labelledby={undefined}
        hidden={!isOpen}
        className="p-4"
      >
        {children}
      </div>
    </div>
  );
};
```

### Tabs

```tsx
export const Tabs = ({ tabs }: { tabs: { label: string; content: React.ReactNode }[] }) => {
  const [activeIndex, setActiveIndex] = useState(0);

  const handleKeyDown = (e: React.KeyboardEvent, index: number) => {
    if (e.key === 'ArrowRight') {
      setActiveIndex((index + 1) % tabs.length);
    } else if (e.key === 'ArrowLeft') {
      setActiveIndex((index - 1 + tabs.length) % tabs.length);
    } else if (e.key === 'Home') {
      setActiveIndex(0);
    } else if (e.key === 'End') {
      setActiveIndex(tabs.length - 1);
    }
  };

  return (
    <div>
      <div role="tablist" aria-label="Content tabs" className="flex border-b">
        {tabs.map((tab, i) => (
          <button
            key={i}
            role="tab"
            id={`tab-${i}`}
            aria-selected={i === activeIndex}
            aria-controls={`tabpanel-${i}`}
            tabIndex={i === activeIndex ? 0 : -1}
            onClick={() => setActiveIndex(i)}
            onKeyDown={(e) => handleKeyDown(e, i)}
            className={cn(
              'px-4 py-2 font-medium focus:ring-2 focus:ring-blue-500',
              i === activeIndex
                ? 'border-b-2 border-blue-600 text-blue-600'
                : 'text-gray-500 hover:text-gray-700'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>
      {tabs.map((tab, i) => (
        <div
          key={i}
          role="tabpanel"
          id={`tabpanel-${i}`}
          aria-labelledby={`tab-${i}`}
          hidden={i !== activeIndex}
          tabIndex={0}
          className="p-4"
        >
          {tab.content}
        </div>
      ))}
    </div>
  );
};
```

### Focus Trap (for Modals)

```tsx
export const useFocusTrap = (containerRef: React.RefObject<HTMLElement | null>, isActive: boolean) => {
  useEffect(() => {
    if (!isActive || !containerRef.current) return;

    const container = containerRef.current;
    const focusableSelectors =
      'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), ' +
      'select:not([disabled]), [tabindex]:not([tabindex="-1"])';

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return;

      const focusable = container.querySelectorAll<HTMLElement>(focusableSelectors);
      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault();
        last?.focus();
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault();
        first?.focus();
      }
    };

    container.addEventListener('keydown', handleKeyDown);
    // Focus first element
    const focusable = container.querySelectorAll<HTMLElement>(focusableSelectors);
    focusable[0]?.focus();

    return () => container.removeEventListener('keydown', handleKeyDown);
  }, [containerRef, isActive]);
};
```

## Live Regions

| Type | Behavior | Use Case |
|------|----------|----------|
| `aria-live="polite"` | Announced when user is idle | Status updates, search results count |
| `aria-live="assertive"` | Announced immediately | Error messages, urgent alerts |
| `role="alert"` | Implicit `aria-live="assertive"` | Form validation errors |
| `role="status"` | Implicit `aria-live="polite"` | "3 items found", "Saved" |
| `aria-atomic="true"` | Read entire region, not just change | Timer, counter |

```tsx
// Search results count — polite
<div role="status" aria-live="polite" className="sr-only">
  {results.length} results found for "{query}"
</div>

// Form error — assertive
<div role="alert" className="text-red-600 text-sm mt-1">
  {errorMessage}
</div>
```

## Official References

- [WAI-ARIA Authoring Practices Guide (APG)](https://www.w3.org/WAI/ARIA/apg/)
- [ARIA States and Properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Attributes)
- [ARIA Roles (MDN)](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles)
- [Using ARIA (W3C Note)](https://www.w3.org/TR/using-aria/)
