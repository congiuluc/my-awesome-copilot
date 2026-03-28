// Sample: DataList component with search, column sorting, and pagination
// A fully featured, accessible, generic list component for displaying tabular data.

import { useState, useMemo, useCallback, useRef, type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Input } from './Input';
import { Button } from './Button';
import { Spinner } from './Spinner';

// ============================================================
// TYPES
// ============================================================

export type SortDirection = 'asc' | 'desc';

export interface DataListColumn<T> {
  /** Unique key for the column (must match a property of T or be a custom key). */
  key: string;
  /** Display header label. */
  header: string;
  /** Custom render function for the cell. If omitted, uses `item[key]`. */
  render?: (item: T) => ReactNode;
  /** Whether this column is sortable. Default: false. */
  sortable?: boolean;
  /** Custom sort comparator. If omitted, uses default string/number comparison. */
  compareFn?: (a: T, b: T) => number;
  /** Column width class (TailwindCSS). */
  className?: string;
}

export interface DataListProps<T> {
  /** The data items to display. */
  data: T[];
  /** Column definitions. */
  columns: DataListColumn<T>[];
  /** Unique key extractor for each item. */
  getKey: (item: T) => string | number;
  /** Whether data is loading. */
  loading?: boolean;
  /** Placeholder text for the search input. */
  searchPlaceholder?: string;
  /** Fields to search across (keys of T). If omitted, search is disabled. */
  searchFields?: (keyof T)[];
  /** Number of items per page. Default: 10. Set to 0 to disable pagination. */
  pageSize?: number;
  /** Page size options for the user to select. */
  pageSizeOptions?: number[];
  /** Empty state message. */
  emptyMessage?: string;
  /** No results message (after search filter). */
  noResultsMessage?: string;
  /** Callback when a row is clicked. */
  onRowClick?: (item: T) => void;
  /** Additional className for the container. */
  className?: string;
}

// ============================================================
// DATALIST COMPONENT
// ============================================================

export const DataList = <T extends Record<string, unknown>>({
  data,
  columns,
  getKey,
  loading = false,
  searchPlaceholder = 'Search...',
  searchFields,
  pageSize: initialPageSize = 10,
  pageSizeOptions = [5, 10, 25, 50],
  emptyMessage = 'No items to display.',
  noResultsMessage = 'No items match your search.',
  onRowClick,
  className,
}: DataListProps<T>) => {
  // --- State ---
  const [searchQuery, setSearchQuery] = useState('');
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const tableRef = useRef<HTMLTableElement>(null);

  // --- Search filtering ---
  const filteredData = useMemo(() => {
    if (!searchQuery.trim() || !searchFields?.length) return data;

    const query = searchQuery.toLowerCase();
    return data.filter((item) =>
      searchFields.some((field) => {
        const value = item[field];
        if (value == null) return false;
        return String(value).toLowerCase().includes(query);
      }),
    );
  }, [data, searchQuery, searchFields]);

  // --- Sorting ---
  const sortedData = useMemo(() => {
    if (!sortKey) return filteredData;

    const column = columns.find((c) => c.key === sortKey);
    if (!column?.sortable) return filteredData;

    const sorted = [...filteredData].sort((a, b) => {
      if (column.compareFn) {
        return column.compareFn(a, b);
      }

      const aVal = a[sortKey as keyof T];
      const bVal = b[sortKey as keyof T];

      if (aVal == null && bVal == null) return 0;
      if (aVal == null) return 1;
      if (bVal == null) return -1;

      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return aVal - bVal;
      }

      return String(aVal).localeCompare(String(bVal));
    });

    return sortDirection === 'desc' ? sorted.reverse() : sorted;
  }, [filteredData, sortKey, sortDirection, columns]);

  // --- Pagination ---
  const totalPages = pageSize > 0 ? Math.max(1, Math.ceil(sortedData.length / pageSize)) : 1;
  const paginatedData = useMemo(() => {
    if (pageSize <= 0) return sortedData;
    const start = (currentPage - 1) * pageSize;
    return sortedData.slice(start, start + pageSize);
  }, [sortedData, currentPage, pageSize]);

  // --- Handlers ---
  const handleSort = useCallback(
    (key: string) => {
      if (sortKey === key) {
        setSortDirection((prev) => (prev === 'asc' ? 'desc' : 'asc'));
      } else {
        setSortKey(key);
        setSortDirection('asc');
      }
      setCurrentPage(1);
    },
    [sortKey],
  );

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
    setCurrentPage(1);
  }, []);

  const handlePageSizeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setPageSize(Number(e.target.value));
    setCurrentPage(1);
  }, []);

  const goToPage = useCallback((page: number) => {
    setCurrentPage(page);
    tableRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }, []);

  // --- Render ---
  if (loading) {
    return (
      <div className="flex items-center justify-center py-12" role="status">
        <Spinner size="lg" />
        <span className="sr-only">Loading data...</span>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="py-12 text-center text-gray-500">
        <p>{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className={cn('space-y-4', className)}>
      {/* Search bar */}
      {searchFields?.length && (
        <div className="max-w-sm">
          <Input
            label="Search"
            value={searchQuery}
            onChange={handleSearch}
            placeholder={searchPlaceholder}
            type="search"
            aria-label={searchPlaceholder}
            icon={
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            }
          />
        </div>
      )}

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-gray-200">
        <table ref={tableRef} className="w-full text-left text-sm" role="grid">
          <thead className="bg-gray-50">
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={cn(
                    'px-4 py-3 font-semibold text-gray-700',
                    column.sortable && 'cursor-pointer select-none hover:bg-gray-100',
                    column.className,
                  )}
                  onClick={column.sortable ? () => handleSort(column.key) : undefined}
                  onKeyDown={
                    column.sortable
                      ? (e) => (e.key === 'Enter' || e.key === ' ') && handleSort(column.key)
                      : undefined
                  }
                  tabIndex={column.sortable ? 0 : undefined}
                  role={column.sortable ? 'columnheader button' : 'columnheader'}
                  aria-sort={
                    sortKey === column.key
                      ? sortDirection === 'asc'
                        ? 'ascending'
                        : 'descending'
                      : column.sortable
                        ? 'none'
                        : undefined
                  }
                >
                  <span className="inline-flex items-center gap-1">
                    {column.header}
                    {column.sortable && sortKey === column.key && (
                      <span aria-hidden="true">{sortDirection === 'asc' ? '▲' : '▼'}</span>
                    )}
                    {column.sortable && sortKey !== column.key && (
                      <span aria-hidden="true" className="text-gray-300">⇅</span>
                    )}
                  </span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {paginatedData.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-8 text-center text-gray-500">
                  {noResultsMessage}
                </td>
              </tr>
            ) : (
              paginatedData.map((item) => (
                <tr
                  key={getKey(item)}
                  className={cn(
                    'transition-colors hover:bg-gray-50',
                    onRowClick && 'cursor-pointer',
                  )}
                  onClick={onRowClick ? () => onRowClick(item) : undefined}
                  onKeyDown={
                    onRowClick
                      ? (e) => (e.key === 'Enter' || e.key === ' ') && onRowClick(item)
                      : undefined
                  }
                  tabIndex={onRowClick ? 0 : undefined}
                  role={onRowClick ? 'row button' : 'row'}
                >
                  {columns.map((column) => (
                    <td key={column.key} className={cn('px-4 py-3', column.className)}>
                      {column.render
                        ? column.render(item)
                        : (item[column.key as keyof T] as ReactNode) ?? '—'}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {pageSize > 0 && sortedData.length > 0 && (
        <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
          {/* Item count + page size selector */}
          <div className="flex items-center gap-3 text-sm text-gray-600">
            <span>
              Showing {Math.min((currentPage - 1) * pageSize + 1, sortedData.length)}–
              {Math.min(currentPage * pageSize, sortedData.length)} of {sortedData.length}
            </span>
            <label className="inline-flex items-center gap-1">
              <span className="sr-only">Items per page</span>
              <select
                value={pageSize}
                onChange={handlePageSizeChange}
                className="rounded border border-gray-300 px-2 py-1 text-sm
                  focus:outline-none focus:ring-2 focus:ring-blue-500"
                aria-label="Items per page"
              >
                {pageSizeOptions.map((opt) => (
                  <option key={opt} value={opt}>
                    {opt} / page
                  </option>
                ))}
              </select>
            </label>
          </div>

          {/* Page navigation */}
          <nav aria-label="Pagination" className="flex items-center gap-1">
            <Button
              variant="outline"
              size="sm"
              onClick={() => goToPage(currentPage - 1)}
              disabled={currentPage === 1}
              aria-label="Previous page"
            >
              ←
            </Button>

            {generatePageNumbers(currentPage, totalPages).map((page, i) =>
              page === '...' ? (
                <span key={`ellipsis-${i}`} className="px-2 text-gray-400">
                  …
                </span>
              ) : (
                <Button
                  key={page}
                  variant={currentPage === page ? 'primary' : 'outline'}
                  size="sm"
                  onClick={() => goToPage(page as number)}
                  aria-label={`Page ${page}`}
                  aria-current={currentPage === page ? 'page' : undefined}
                >
                  {page}
                </Button>
              ),
            )}

            <Button
              variant="outline"
              size="sm"
              onClick={() => goToPage(currentPage + 1)}
              disabled={currentPage === totalPages}
              aria-label="Next page"
            >
              →
            </Button>
          </nav>
        </div>
      )}
    </div>
  );
};

// ============================================================
// HELPERS
// ============================================================

/** Generates page numbers with ellipsis for large page counts. */
function generatePageNumbers(current: number, total: number): (number | '...')[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const pages: (number | '...')[] = [1];

  if (current > 3) pages.push('...');

  const start = Math.max(2, current - 1);
  const end = Math.min(total - 1, current + 1);

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  if (current < total - 2) pages.push('...');

  pages.push(total);
  return pages;
}

// ============================================================
// USAGE EXAMPLE
// ============================================================

// interface Product {
//   id: string;
//   name: string;
//   category: string;
//   price: number;
//   createdAt: string;
// }
//
// const columns: DataListColumn<Product>[] = [
//   { key: 'name', header: 'Name', sortable: true },
//   { key: 'category', header: 'Category', sortable: true },
//   {
//     key: 'price',
//     header: 'Price',
//     sortable: true,
//     render: (item) => `$${item.price.toFixed(2)}`,
//     className: 'text-right',
//   },
//   {
//     key: 'createdAt',
//     header: 'Created',
//     sortable: true,
//     render: (item) => new Date(item.createdAt).toLocaleDateString(),
//   },
//   {
//     key: 'actions',
//     header: '',
//     render: (item) => (
//       <Button variant="ghost" size="sm" onClick={() => handleEdit(item)}>
//         Edit
//       </Button>
//     ),
//   },
// ];
//
// <DataList
//   data={products}
//   columns={columns}
//   getKey={(p) => p.id}
//   searchFields={['name', 'category']}
//   searchPlaceholder="Search products..."
//   pageSize={10}
//   onRowClick={(product) => navigate(`/products/${product.id}`)}
// />
