// Sample: Feature Component with hooks, types, props, and TailwindCSS
// Complete pattern for a feature component in React 19 + TypeScript.

import { useState, useCallback } from 'react';
import { Button } from '@/components/shared/Button';
import { Spinner } from '@/components/shared/Spinner';
import { useProducts } from '../hooks/useProducts';
import type { Product } from '../types/product.types';

// --- Props interface (suffixed with Props) ---

export interface ProductListProps {
  /** Optional filter for product category */
  category?: string;
  /** Callback when a product is selected */
  onSelect: (product: Product) => void;
}

// --- Feature component (named export, functional only) ---

export const ProductList = ({ category, onSelect }: ProductListProps) => {
  const { data: products, loading, error, refetch } = useProducts({ category });
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const handleSelect = useCallback(
    (product: Product) => {
      setSelectedId(product.id);
      onSelect(product);
    },
    [onSelect],
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12" role="status">
        <Spinner />
        <span className="sr-only">Loading products...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg bg-red-50 p-4 text-red-700" role="alert">
        <p className="font-medium">Failed to load products</p>
        <p className="mt-1 text-sm">{error.message}</p>
        <Button variant="outline" size="sm" className="mt-3" onClick={refetch}>
          Try again
        </Button>
      </div>
    );
  }

  if (products.length === 0) {
    return (
      <div className="py-12 text-center text-gray-500">
        <p>No products found{category ? ` in "${category}"` : ''}.</p>
      </div>
    );
  }

  return (
    <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3" role="list">
      {products.map((product) => (
        <li key={product.id}>
          <button
            type="button"
            className={`w-full rounded-lg border p-4 text-left transition
              hover:border-blue-500 hover:shadow-md focus:outline-none
              focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
              ${selectedId === product.id ? 'border-blue-500 bg-blue-50' : 'border-gray-200'}`}
            onClick={() => handleSelect(product)}
            aria-pressed={selectedId === product.id}
          >
            <h3 className="font-semibold text-gray-900">{product.name}</h3>
            {product.description && (
              <p className="mt-1 line-clamp-2 text-sm text-gray-600">
                {product.description}
              </p>
            )}
            <p className="mt-2 text-lg font-bold text-blue-600">
              ${product.price.toFixed(2)}
            </p>
          </button>
        </li>
      ))}
    </ul>
  );
};

// --- Types (typically in features/{feature}/types/{feature}.types.ts) ---
// export interface Product {
//   id: string;
//   name: string;
//   description?: string;
//   price: number;
//   createdAtUtc: string;
// }

// --- Hook (typically in features/{feature}/hooks/useProducts.ts) ---
// export const useProducts = ({ category }: { category?: string }) => {
//   const [data, setData] = useState<Product[]>([]);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState<Error | null>(null);
//   // ... fetch logic with useEffect and AbortController
//   return { data, loading, error, refetch };
// };
