// Sample: Modal component with focus trap, Escape to close, and portal rendering

import { useEffect, useRef, useCallback, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/utils/cn';
import { Button } from './Button';

// ============================================================
// MODAL
// ============================================================

export interface ModalProps {
  /** Whether the modal is open. */
  open: boolean;
  /** Callback to close the modal. */
  onClose: () => void;
  /** Modal title displayed in the header. */
  title: string;
  /** Modal content. */
  children: ReactNode;
  /** Optional footer content (buttons, actions). */
  footer?: ReactNode;
  /** Additional className for the modal panel. */
  className?: string;
  /** Max width of the modal panel. */
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

const modalSizes = {
  sm: 'max-w-sm',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
} as const;

export const Modal = ({
  open,
  onClose,
  title,
  children,
  footer,
  className,
  size = 'md',
}: ModalProps) => {
  const panelRef = useRef<HTMLDivElement>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  // Save and restore focus
  useEffect(() => {
    if (open) {
      previousFocusRef.current = document.activeElement as HTMLElement;
      // Focus the panel on open
      requestAnimationFrame(() => panelRef.current?.focus());
    } else {
      previousFocusRef.current?.focus();
    }
  }, [open]);

  // Close on Escape
  useEffect(() => {
    if (!open) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [open, onClose]);

  // Lock body scroll
  useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [open]);

  // Focus trap
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key !== 'Tab' || !panelRef.current) return;

      const focusable = panelRef.current.querySelectorAll<HTMLElement>(
        'a[href], button:not([disabled]), textarea, input, select, [tabindex]:not([tabindex="-1"])',
      );

      if (focusable.length === 0) return;

      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault();
        last.focus();
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault();
        first.focus();
      }
    },
    [],
  );

  if (!open) return null;

  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 transition-opacity"
        aria-hidden="true"
        onClick={onClose}
      />

      {/* Panel */}
      <div
        ref={panelRef}
        tabIndex={-1}
        onKeyDown={handleKeyDown}
        className={cn(
          'relative z-10 w-full rounded-xl bg-white shadow-xl',
          'max-h-[90vh] overflow-auto',
          'focus:outline-none',
          modalSizes[size],
          className,
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 id="modal-title" className="text-lg font-semibold text-gray-900">
            {title}
          </h2>
          <button
            type="button"
            className="min-h-8 min-w-8 rounded-full p-1 text-gray-400
              hover:bg-gray-100 hover:text-gray-600
              focus:outline-none focus:ring-2 focus:ring-blue-500"
            onClick={onClose}
            aria-label="Close modal"
          >
            <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="px-6 py-4">{children}</div>

        {/* Footer */}
        {footer && (
          <div className="flex justify-end gap-3 border-t px-6 py-4">
            {footer}
          </div>
        )}
      </div>
    </div>,
    document.body,
  );
};

// --- Usage Example ---
// const [open, setOpen] = useState(false);
//
// <Button onClick={() => setOpen(true)}>Open Modal</Button>
//
// <Modal
//   open={open}
//   onClose={() => setOpen(false)}
//   title="Confirm Action"
//   footer={
//     <>
//       <Button variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
//       <Button onClick={handleConfirm}>Confirm</Button>
//     </>
//   }
// >
//   <p>Are you sure you want to proceed?</p>
// </Modal>
