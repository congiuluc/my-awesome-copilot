// Sample: React Component Test with Vitest and React Testing Library
// Shows rendering, user interactions, accessibility queries, and hook testing.

import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ProductList } from '@/features/product';

// --- Mock data ---
const mockProducts = [
  { id: '1', name: 'Widget', description: 'A useful widget', price: 29.99 },
  { id: '2', name: 'Gadget', description: 'A cool gadget', price: 49.99 },
];

// --- Mock hook ---
const mockUseProducts = vi.fn();
vi.mock('@/features/product/hooks/useProducts', () => ({
  useProducts: (...args: unknown[]) => mockUseProducts(...args),
}));

describe('ProductList', () => {
  const defaultProps = {
    onSelect: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseProducts.mockReturnValue({
      data: mockProducts,
      loading: false,
      error: null,
      refetch: vi.fn(),
    });
  });

  // --- Rendering tests ---

  it('renders all products', () => {
    render(<ProductList {...defaultProps} />);

    expect(screen.getByText('Widget')).toBeInTheDocument();
    expect(screen.getByText('Gadget')).toBeInTheDocument();
  });

  it('renders product prices', () => {
    render(<ProductList {...defaultProps} />);

    expect(screen.getByText('$29.99')).toBeInTheDocument();
    expect(screen.getByText('$49.99')).toBeInTheDocument();
  });

  // --- Loading state ---

  it('shows loading spinner while fetching', () => {
    mockUseProducts.mockReturnValue({
      data: [],
      loading: true,
      error: null,
      refetch: vi.fn(),
    });

    render(<ProductList {...defaultProps} />);

    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.getByText('Loading products...')).toBeInTheDocument();
  });

  // --- Error state ---

  it('shows error message and retry button on failure', () => {
    mockUseProducts.mockReturnValue({
      data: [],
      loading: false,
      error: new Error('Network error'),
      refetch: vi.fn(),
    });

    render(<ProductList {...defaultProps} />);

    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText('Failed to load products')).toBeInTheDocument();
    expect(screen.getByText('Network error')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
  });

  it('calls refetch when retry button is clicked', async () => {
    const refetch = vi.fn();
    mockUseProducts.mockReturnValue({
      data: [],
      loading: false,
      error: new Error('Failed'),
      refetch,
    });

    const user = userEvent.setup();
    render(<ProductList {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: /try again/i }));
    expect(refetch).toHaveBeenCalledOnce();
  });

  // --- Empty state ---

  it('shows empty message when no products exist', () => {
    mockUseProducts.mockReturnValue({
      data: [],
      loading: false,
      error: null,
      refetch: vi.fn(),
    });

    render(<ProductList {...defaultProps} />);

    expect(screen.getByText('No products found.')).toBeInTheDocument();
  });

  // --- User interaction ---

  it('calls onSelect when a product is clicked', async () => {
    const onSelect = vi.fn();
    const user = userEvent.setup();
    render(<ProductList {...defaultProps} onSelect={onSelect} />);

    await user.click(screen.getByText('Widget'));
    expect(onSelect).toHaveBeenCalledWith(mockProducts[0]);
  });

  it('highlights selected product', async () => {
    const user = userEvent.setup();
    render(<ProductList {...defaultProps} />);

    const widgetButton = screen.getByText('Widget').closest('button')!;
    await user.click(widgetButton);

    expect(widgetButton).toHaveAttribute('aria-pressed', 'true');
  });

  // --- Keyboard navigation ---

  it('supports keyboard activation via Enter', async () => {
    const onSelect = vi.fn();
    const user = userEvent.setup();
    render(<ProductList {...defaultProps} onSelect={onSelect} />);

    await user.tab(); // Focus first product
    await user.keyboard('{Enter}');

    expect(onSelect).toHaveBeenCalledOnce();
  });

  // --- Accessibility ---

  it('renders products in an accessible list', () => {
    render(<ProductList {...defaultProps} />);

    expect(screen.getByRole('list')).toBeInTheDocument();
    expect(screen.getAllByRole('listitem')).toHaveLength(2);
  });

  // --- Category filtering ---

  it('passes category to useProducts hook', () => {
    render(<ProductList {...defaultProps} category="electronics" />);

    expect(mockUseProducts).toHaveBeenCalledWith({ category: 'electronics' });
  });
});
