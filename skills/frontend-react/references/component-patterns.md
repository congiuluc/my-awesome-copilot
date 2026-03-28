# Component Patterns

## Functional Components Only

- **Named exports** — no default exports.
- Props interface suffixed with `Props` and defined above the component.
- One component per file. File name matches component name in PascalCase.
- Destructure props in the function signature.

```tsx
export interface ProductCardProps {
  title: string;
  imageUrl: string;
  description?: string;
  onSelect: (id: string) => void;
}

export const ProductCard = ({ title, imageUrl, description, onSelect }: ProductCardProps) => {
  return (
    <article className="rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow">
      <img
        src={imageUrl}
        alt={title}
        className="w-full h-48 object-cover rounded"
      />
      <h3 className="text-lg font-semibold mt-2">{title}</h3>
      {description && <p className="text-gray-600 mt-1 text-sm">{description}</p>}
    </article>
  );
};
```

## Styling (TailwindCSS v4)

- Use Tailwind utility classes directly in JSX — no custom CSS files.
- Use `@theme` in `app.css` for design tokens (colors, spacing, fonts).
- For complex repeated patterns, extract shared components rather than `@apply`.
- Use `cn()` helper (from `clsx` + `tailwind-merge`) for conditional class merging.

```tsx
import { cn } from '@/utils/cn';

export interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  children: React.ReactNode;
  onClick?: () => void;
}

export const Button = ({ variant = 'primary', size = 'md', disabled, children, onClick }: ButtonProps) => {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={cn(
        'rounded font-medium transition-colors focus:outline-2 focus:outline-offset-2',
        {
          'bg-blue-600 text-white hover:bg-blue-700 focus:outline-blue-600': variant === 'primary',
          'bg-gray-200 text-gray-800 hover:bg-gray-300 focus:outline-gray-600': variant === 'secondary',
          'bg-red-600 text-white hover:bg-red-700 focus:outline-red-600': variant === 'danger',
          'px-3 py-1.5 text-sm': size === 'sm',
          'px-4 py-2 text-base': size === 'md',
          'px-6 py-3 text-lg': size === 'lg',
          'opacity-50 cursor-not-allowed': disabled,
        }
      )}
    >
      {children}
    </button>
  );
};
```

## Responsive Design

- **Mobile-first** approach: start with base styles, add `sm:`, `md:`, `lg:`, `xl:` breakpoints.
- Test at breakpoints: 320px, 640px, 768px, 1024px, 1280px.
- Fluid typography and spacing where appropriate.
- Navigation must collapse to hamburger menu on mobile.

## State Management

- Use React's built-in state (`useState`, `useReducer`) for local component state.
- Use React Context for shared state that doesn't change frequently (theme, auth).
- For server state, use `fetch` with custom hooks that handle loading/error states.
- Avoid prop drilling beyond 2 levels — extract to Context or composition.

## API Integration

Centralize API calls in `services/` with typed request/response:

```tsx
import type { ApiResponse } from '@/types/api';
import type { ProductDto } from '@/features/products/types/product.types';

const API_BASE = import.meta.env.VITE_API_URL ?? '/api';

export const productService = {
  getAll: async (): Promise<ApiResponse<ProductDto[]>> => {
    const response = await fetch(`${API_BASE}/product`);
    if (!response.ok) {
      return { success: false, data: null, error: `HTTP ${response.status}` };
    }
    return response.json();
  },

  getById: async (id: string): Promise<ApiResponse<ProductDto>> => {
    const response = await fetch(`${API_BASE}/product/${encodeURIComponent(id)}`);
    if (!response.ok) {
      return { success: false, data: null, error: `HTTP ${response.status}` };
    }
    return response.json();
  },
};
```

Custom hook pattern for data fetching:

```tsx
export const useProducts = () => {
  const [data, setData] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    const fetchData = async () => {
      try {
        const response = await productService.getAll();
        if (response.success && response.data) {
          setData(response.data);
        } else {
          setError(response.error ?? 'Unknown error');
        }
      } catch {
        if (!controller.signal.aborted) {
          setError('Failed to fetch products');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchData();
    return () => controller.abort();
  }, []);

  return { data, loading, error };
};
```

## Image Preloading

Whenever a component fetches data that includes image URLs, **always preload the
images during the loading phase** so they render instantly once data arrives.

### `useImagePreload` Hook

```tsx
import { useEffect, useState } from 'react';

/**
 * Preloads an array of image URLs in the background.
 * Returns `true` once every image has loaded (or failed).
 */
export const useImagePreload = (urls: string[]): boolean => {
  const [ready, setReady] = useState(urls.length === 0);

  useEffect(() => {
    if (urls.length === 0) {
      setReady(true);
      return;
    }

    let cancelled = false;

    Promise.all(
      urls.map(
        (src) =>
          new Promise<void>((resolve) => {
            const img = new Image();
            img.onload = () => resolve();
            img.onerror = () => resolve(); // don't block on broken images
            img.src = src;
          })
      )
    ).then(() => {
      if (!cancelled) setReady(true);
    });

    return () => {
      cancelled = true;
    };
  }, [urls]);

  return ready;
};
```

### Using the Hook in Data-Fetching Components

```tsx
export const ProductList = () => {
  const { data, loading, error } = useProducts();
  const imageUrls = data.map((p) => p.imageUrl).filter(Boolean);
  const imagesReady = useImagePreload(imageUrls);

  if (loading || !imagesReady) return <Spinner aria-label="Loading products" />;
  if (error) return <ErrorMessage message={error} />;
  if (data.length === 0) return <EmptyState message="No products found" />;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {data.map(product => (
        <ProductCard key={product.id} {...product} />
      ))}
    </div>
  );
};
```

### Critical Above-the-Fold Images with `<link rel="preload">`

For hero images or first-visible content, add a preload link in the `<head>`:

```tsx
import { useEffect } from 'react';

export const usePreloadLink = (href: string, as: string = 'image') => {
  useEffect(() => {
    if (!href) return;
    const existing = document.querySelector(`link[href="${CSS.escape(href)}"]`);
    if (existing) return;

    const link = document.createElement('link');
    link.rel = 'preload';
    link.as = as;
    link.href = href;
    document.head.appendChild(link);

    return () => {
      link.remove();
    };
  }, [href, as]);
};
```

## Loading / Error / Empty States

Every data-fetching component must handle all three states.
When data includes images, **preload them during the loading phase**:

```tsx
export const ProductList = () => {
  const { data, loading, error } = useProducts();
  const imageUrls = data.map((p) => p.imageUrl).filter(Boolean);
  const imagesReady = useImagePreload(imageUrls);

  if (loading || !imagesReady) return <Spinner aria-label="Loading products" />;
  if (error) return <ErrorMessage message={error} />;
  if (data.length === 0) return <EmptyState message="No products found" />;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {data.map(product => (
        <ProductCard key={product.id} {...product} />
      ))}
    </div>
  );
};
```
