# Responsive Layout Patterns

> Official reference: [web.dev Layout patterns](https://web.dev/patterns/layout/)

## Page Shell (Header / Main / Footer)

```tsx
export const PageShell = ({ children }: { children: React.ReactNode }) => (
  <div className="min-h-screen flex flex-col">
    <header className="sticky top-0 z-40 bg-white shadow-sm">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 h-16 flex items-center">
        {/* Header content */}
      </div>
    </header>

    <main className="flex-1">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-6 md:py-10">
        {children}
      </div>
    </main>

    <footer className="border-t bg-gray-50">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
        {/* Footer content */}
      </div>
    </footer>
  </div>
);
```

## Sidebar Layout

```tsx
export const SidebarLayout = ({
  sidebar,
  children,
}: {
  sidebar: React.ReactNode;
  children: React.ReactNode;
}) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="flex min-h-screen">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/50 lg:hidden"
          onClick={() => setSidebarOpen(false)}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-40 w-64 bg-white border-r transform transition-transform lg:static lg:translate-x-0',
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        )}
        aria-label="Sidebar"
      >
        <div className="h-full overflow-y-auto p-4">
          {sidebar}
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 min-w-0">
        {/* Mobile sidebar toggle */}
        <button
          className="lg:hidden p-4"
          onClick={() => setSidebarOpen(true)}
          aria-label="Open sidebar"
        >
          <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <main className="p-4 sm:p-6 lg:p-8">
          {children}
        </main>
      </div>
    </div>
  );
};
```

## Card Grid

```tsx
export const CardGrid = ({ children }: { children: React.ReactNode }) => (
  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 md:gap-6">
    {children}
  </div>
);

// Card that fills grid cell
export const Card = ({ title, description, image }: CardProps) => (
  <article className="flex flex-col overflow-hidden rounded-lg border bg-white shadow-sm hover:shadow-md transition-shadow">
    {image && (
      <img
        src={image}
        alt=""
        className="h-40 sm:h-48 w-full object-cover"
        loading="lazy"
        decoding="async"
      />
    )}
    <div className="flex flex-col flex-1 p-4">
      <h3 className="font-semibold text-base lg:text-lg">{title}</h3>
      <p className="mt-1 text-sm text-gray-600 flex-1">{description}</p>
    </div>
  </article>
);
```

## Split Layout (Hero + Content)

```tsx
export const SplitLayout = ({ left, right }: { left: React.ReactNode; right: React.ReactNode }) => (
  <div className="flex flex-col lg:flex-row lg:items-center gap-8 lg:gap-16 py-8 lg:py-16">
    <div className="flex-1">{left}</div>
    <div className="flex-1">{right}</div>
  </div>
);
```

## Responsive Table

For tables that overflow on mobile, wrap in a scrollable container:

```tsx
export const ResponsiveTable = ({ children }: { children: React.ReactNode }) => (
  <div className="overflow-x-auto -mx-4 sm:mx-0">
    <div className="inline-block min-w-full align-middle">
      <table className="min-w-full divide-y divide-gray-200">
        {children}
      </table>
    </div>
  </div>
);
```

Alternative: stack rows on mobile:

```tsx
// Desktop: table row | Mobile: stacked card
export const ResponsiveRow = ({ label, value }: { label: string; value: string }) => (
  <>
    {/* Mobile: label-value pairs */}
    <div className="md:hidden flex justify-between py-2 border-b">
      <span className="font-medium text-gray-500">{label}</span>
      <span>{value}</span>
    </div>
    {/* Desktop: table cell (handled by parent <table>) */}
  </>
);
```

## Official References

- [web.dev Layout Patterns](https://web.dev/patterns/layout/)
- [CSS Grid Layout (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_grid_layout)
- [CSS Flexbox (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_flexible_box_layout)
- [TailwindCSS Grid](https://tailwindcss.com/docs/grid-template-columns)
- [TailwindCSS Flexbox](https://tailwindcss.com/docs/flex)
