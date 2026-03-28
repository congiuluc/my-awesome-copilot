/**
 * Toast Notification System Sample
 *
 * Complete toast implementation using Sonner with:
 * - Accessible toast container with proper ARIA
 * - Custom styled toasts with icons
 * - Promise-based toasts for async operations
 * - SignalR real-time notification hook
 *
 * Prerequisites: npm install sonner @microsoft/signalr
 *
 * Official references:
 * - https://sonner.emilkowal.dev/
 * - https://www.w3.org/WAI/ARIA/apg/patterns/alert/
 */

import { Toaster, toast } from 'sonner';

// =============================================================================
// App Root — Toast Container
// =============================================================================

/**
 * Add this Toaster to your root layout (App.tsx or main layout component).
 */
export const AppToaster = () => (
  <Toaster
    position="top-right"
    richColors
    closeButton
    toastOptions={{
      duration: 5000,
      className: 'text-sm font-medium',
    }}
    // Accessibility
    visibleToasts={5}
  />
);

// =============================================================================
// Toast Utility — Type-Safe Wrapper
// =============================================================================

export const notify = {
  success: (message: string) => toast.success(message),
  error: (message: string) => toast.error(message, { duration: 8000 }),
  warning: (message: string) => toast.warning(message),
  info: (message: string) => toast.info(message),

  /** Show loading → success/error based on promise result */
  promise: <T,>(
    promise: Promise<T>,
    messages: { loading: string; success: string; error: string },
  ) => toast.promise(promise, messages),

  /** Toast with undo action */
  withUndo: (message: string, onUndo: () => void) =>
    toast(message, {
      action: { label: 'Undo', onClick: onUndo },
      duration: 6000,
    }),
};

// =============================================================================
// Usage Examples in Components
// =============================================================================

/**
 * Example: Save form with toast feedback.
 */
export const SaveProductExample = () => {
  const handleSave = async () => {
    notify.promise(
      fetch('/api/products', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: 'New Product' }),
      }).then(res => {
        if (!res.ok) throw new Error('Save failed');
        return res.json();
      }),
      {
        loading: 'Saving product...',
        success: 'Product saved successfully!',
        error: 'Failed to save product. Please try again.',
      },
    );
  };

  return (
    <button
      onClick={handleSave}
      className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white
                 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
    >
      Save Product
    </button>
  );
};

/**
 * Example: Delete with undo toast.
 */
export const DeleteWithUndoExample = () => {
  const handleDelete = (id: string) => {
    // Optimistically remove from UI
    notify.withUndo('Item deleted', () => {
      // Restore item on undo
      console.log('Restoring item', id);
    });
  };

  return (
    <button
      onClick={() => handleDelete('123')}
      className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white
                 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
    >
      Delete
    </button>
  );
};
