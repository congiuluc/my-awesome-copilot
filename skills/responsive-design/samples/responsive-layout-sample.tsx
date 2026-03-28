/**
 * Responsive Layout Sample
 *
 * Complete responsive page layout demonstrating mobile-first design with
 * TailwindCSS breakpoints. Includes responsive header, card grid, sidebar,
 * and footer.
 *
 * Official references:
 * - https://tailwindcss.com/docs/responsive-design
 * - https://web.dev/patterns/layout/
 */

import { useState } from 'react';

// =============================================================================
// Responsive Page Container
// =============================================================================

export const ResponsivePage = ({ children }: { children: React.ReactNode }) => (
  <div className="min-h-screen flex flex-col bg-gray-50">
    <ResponsiveHeader />
    <main className="flex-1">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-6 md:py-10">
        {children}
      </div>
    </main>
    <ResponsiveFooter />
  </div>
);

// =============================================================================
// Responsive Header with Mobile Menu
// =============================================================================

const navLinks = [
  { label: 'Home', href: '/' },
  { label: 'Features', href: '/features' },
  { label: 'Pricing', href: '/pricing' },
  { label: 'About', href: '/about' },
];

export const ResponsiveHeader = () => {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <header className="sticky top-0 z-40 bg-white/95 backdrop-blur-sm shadow-sm">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <a href="/" className="text-xl font-bold text-gray-900">
            MyApp
          </a>

          {/* Desktop nav */}
          <nav aria-label="Main navigation" className="hidden md:flex md:gap-1">
            {navLinks.map(link => (
              <a
                key={link.href}
                href={link.href}
                className="rounded-md px-3 py-2 text-sm font-medium text-gray-700
                           hover:bg-gray-100 hover:text-gray-900
                           focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {link.label}
              </a>
            ))}
          </nav>

          {/* Mobile toggle */}
          <button
            className="md:hidden min-h-11 min-w-11 inline-flex items-center justify-center
                       rounded-md hover:bg-gray-100 focus:ring-2 focus:ring-blue-500"
            onClick={() => setMenuOpen(!menuOpen)}
            aria-expanded={menuOpen}
            aria-controls="mobile-nav"
            aria-label={menuOpen ? 'Close menu' : 'Open menu'}
          >
            <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d={menuOpen ? 'M6 18L18 6M6 6l12 12' : 'M4 6h16M4 12h16M4 18h16'}
              />
            </svg>
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      {menuOpen && (
        <nav id="mobile-nav" aria-label="Mobile navigation" className="md:hidden border-t bg-white">
          <div className="flex flex-col px-4 py-2">
            {navLinks.map(link => (
              <a
                key={link.href}
                href={link.href}
                className="min-h-11 flex items-center rounded-md px-3 text-base
                           font-medium text-gray-700 hover:bg-gray-100"
              >
                {link.label}
              </a>
            ))}
          </div>
        </nav>
      )}
    </header>
  );
};

// =============================================================================
// Responsive Card Grid
// =============================================================================

interface CardData {
  id: string;
  title: string;
  description: string;
  imageUrl: string;
}

export const ResponsiveCardGrid = ({ items }: { items: CardData[] }) => (
  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6">
    {items.map(item => (
      <article
        key={item.id}
        className="flex flex-col overflow-hidden rounded-lg border bg-white shadow-sm
                   hover:shadow-md transition-shadow"
      >
        <div className="aspect-video overflow-hidden">
          <img
            src={item.imageUrl}
            alt=""
            className="h-full w-full object-cover"
            loading="lazy"
            decoding="async"
          />
        </div>
        <div className="flex flex-col flex-1 p-4">
          <h3 className="font-semibold text-base lg:text-lg">{item.title}</h3>
          <p className="mt-1 text-sm text-gray-600 flex-1">{item.description}</p>
          <a
            href={`/items/${item.id}`}
            className="mt-3 inline-flex items-center text-sm font-medium text-blue-600
                       hover:text-blue-800 focus:ring-2 focus:ring-blue-500 rounded"
          >
            Learn more →
          </a>
        </div>
      </article>
    ))}
  </div>
);

// =============================================================================
// Responsive Footer
// =============================================================================

export const ResponsiveFooter = () => (
  <footer className="border-t bg-white">
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8 md:py-12">
      {/* Stacked on mobile, row on desktop */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
        <div>
          <h3 className="text-sm font-semibold text-gray-900">Product</h3>
          <ul className="mt-3 space-y-2 text-sm text-gray-600">
            <li><a href="/features" className="hover:text-gray-900">Features</a></li>
            <li><a href="/pricing" className="hover:text-gray-900">Pricing</a></li>
          </ul>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-900">Company</h3>
          <ul className="mt-3 space-y-2 text-sm text-gray-600">
            <li><a href="/about" className="hover:text-gray-900">About</a></li>
            <li><a href="/contact" className="hover:text-gray-900">Contact</a></li>
          </ul>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-900">Legal</h3>
          <ul className="mt-3 space-y-2 text-sm text-gray-600">
            <li><a href="/privacy" className="hover:text-gray-900">Privacy</a></li>
            <li><a href="/terms" className="hover:text-gray-900">Terms</a></li>
          </ul>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-900">Support</h3>
          <ul className="mt-3 space-y-2 text-sm text-gray-600">
            <li><a href="/docs" className="hover:text-gray-900">Documentation</a></li>
            <li><a href="/faq" className="hover:text-gray-900">FAQ</a></li>
          </ul>
        </div>
      </div>

      <div className="mt-8 pt-8 border-t text-center text-sm text-gray-500">
        © {new Date().getFullYear()} MyApp. All rights reserved.
      </div>
    </div>
  </footer>
);
