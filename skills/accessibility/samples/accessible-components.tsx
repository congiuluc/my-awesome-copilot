/**
 * Accessible Component Samples
 *
 * Ready-to-use examples of accessible React components following WCAG 2.1 AA.
 * Each component demonstrates proper ARIA usage, keyboard navigation, and focus management.
 *
 * Official references:
 * - https://www.w3.org/WAI/ARIA/apg/
 * - https://developer.mozilla.org/en-US/docs/Web/Accessibility
 */

import { useState, useRef, useEffect, useId } from 'react';

// =============================================================================
// Accessible Modal Dialog
// Uses native <dialog> with focus trap and Escape key handling.
// Ref: https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/
// =============================================================================

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
}

export const Modal = ({ isOpen, onClose, title, children }: ModalProps) => {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;

    if (isOpen) {
      dialog.showModal();
    } else {
      dialog.close();
    }
  }, [isOpen]);

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={titleId}
      onClose={onClose}
      className="rounded-lg shadow-xl p-0 backdrop:bg-black/50 max-w-lg w-full"
    >
      <div className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 id={titleId} className="text-xl font-semibold">
            {title}
          </h2>
          <button
            onClick={onClose}
            aria-label="Close dialog"
            className="p-1 rounded hover:bg-gray-100 focus:ring-2 focus:ring-blue-500"
          >
            <svg aria-hidden="true" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
            </svg>
          </button>
        </div>
        {children}
      </div>
    </dialog>
  );
};

// =============================================================================
// Accessible Accordion / Disclosure
// Ref: https://www.w3.org/WAI/ARIA/apg/patterns/disclosure/
// =============================================================================

export interface AccordionItem {
  title: string;
  content: React.ReactNode;
}

export const Accordion = ({ items }: { items: AccordionItem[] }) => {
  const [openIndex, setOpenIndex] = useState<number | null>(null);

  return (
    <div className="divide-y divide-gray-200 border rounded-lg">
      {items.map((item, index) => {
        const isOpen = openIndex === index;
        const contentId = `accordion-content-${index}`;
        const headerId = `accordion-header-${index}`;

        return (
          <div key={index}>
            <h3>
              <button
                id={headerId}
                aria-expanded={isOpen}
                aria-controls={contentId}
                onClick={() => setOpenIndex(isOpen ? null : index)}
                className="flex w-full items-center justify-between p-4 text-left
                           font-medium hover:bg-gray-50 focus:ring-2 focus:ring-inset
                           focus:ring-blue-500"
              >
                <span>{item.title}</span>
                <svg
                  aria-hidden="true"
                  className={`h-5 w-5 transition-transform ${isOpen ? 'rotate-180' : ''}`}
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path fillRule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z" />
                </svg>
              </button>
            </h3>
            <div
              id={contentId}
              role="region"
              aria-labelledby={headerId}
              hidden={!isOpen}
              className="p-4 text-gray-700"
            >
              {item.content}
            </div>
          </div>
        );
      })}
    </div>
  );
};

// =============================================================================
// Accessible Alert / Toast
// Uses role="alert" for immediate screen reader announcement.
// Ref: https://www.w3.org/WAI/ARIA/apg/patterns/alert/
// =============================================================================

export interface AlertProps {
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  onDismiss?: () => void;
}

const alertStyles = {
  success: 'bg-green-50 border-green-200 text-green-800',
  error: 'bg-red-50 border-red-200 text-red-800',
  warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
  info: 'bg-blue-50 border-blue-200 text-blue-800',
} as const;

export const Alert = ({ type, message, onDismiss }: AlertProps) => (
  <div
    role="alert"
    className={`rounded-lg border p-4 flex items-center justify-between ${alertStyles[type]}`}
  >
    <p className="text-sm">{message}</p>
    {onDismiss && (
      <button
        onClick={onDismiss}
        aria-label="Dismiss alert"
        className="p-1 rounded hover:bg-black/10 focus:ring-2 focus:ring-current"
      >
        <svg aria-hidden="true" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
          <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
        </svg>
      </button>
    )}
  </div>
);

// =============================================================================
// Accessible Skip Navigation Link
// Must be the first focusable element on the page.
// Ref: https://www.w3.org/WAI/WCAG21/Techniques/general/G1
// =============================================================================

export const SkipNavLink = ({ targetId = 'main-content' }: { targetId?: string }) => (
  <a
    href={`#${targetId}`}
    className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50
               focus:bg-white focus:text-blue-700 focus:px-4 focus:py-2 focus:rounded-md
               focus:shadow-lg focus:ring-2 focus:ring-blue-500 focus:font-medium"
  >
    Skip to main content
  </a>
);
