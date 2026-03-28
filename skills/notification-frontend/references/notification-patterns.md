# Frontend Notification Patterns

> Official reference: [Sonner Docs](https://sonner.emilkowal.dev/), [WAI-ARIA Alert](https://www.w3.org/WAI/ARIA/apg/patterns/alert/)

## Option 1: Sonner (Recommended)

Lightweight, accessible, styled or unstyled toast library.

### Setup

```bash
npm install sonner
```

```tsx
// App.tsx or root layout
import { Toaster } from 'sonner';

export const App = () => (
  <>
    <Toaster
      position="top-right"
      richColors
      closeButton
      toastOptions={{
        duration: 5000,
        className: 'text-sm',
      }}
    />
    <RouterProvider router={router} />
  </>
);
```

### Usage

```tsx
import { toast } from 'sonner';

// In event handlers or API callbacks
const handleSave = async () => {
  try {
    await api.saveProduct(data);
    toast.success('Product saved successfully');
  } catch (error) {
    toast.error('Failed to save product. Please try again.');
  }
};

// Promise toast (shows loading → success/error automatically)
const handleDelete = () => {
  toast.promise(api.deleteProduct(id), {
    loading: 'Deleting product...',
    success: 'Product deleted',
    error: 'Could not delete product',
  });
};

// With action button
toast('Product updated', {
  action: {
    label: 'Undo',
    onClick: () => api.undoUpdate(id),
  },
});

// Persistent error (no auto-dismiss)
toast.error('Connection lost', { duration: Infinity });
```

## Option 2: Custom Toast System

For zero dependencies — build a minimal toast context:

```tsx
import { createContext, useCallback, useContext, useState } from 'react';

type ToastType = 'success' | 'error' | 'warning' | 'info';

interface Toast {
  id: string;
  type: ToastType;
  message: string;
}

interface ToastContextValue {
  toasts: Toast[];
  addToast: (type: ToastType, message: string) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be inside ToastProvider');
  return ctx;
};

export const ToastProvider = ({ children }: { children: React.ReactNode }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const addToast = useCallback((type: ToastType, message: string) => {
    const id = crypto.randomUUID();
    setToasts(prev => [...prev, { id, type, message }]);

    // Auto-dismiss (errors stay longer)
    const duration = type === 'error' ? 8000 : 5000;
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, duration);
  }, []);

  const removeToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
      <ToastContainer toasts={toasts} onDismiss={removeToast} />
    </ToastContext.Provider>
  );
};
```

## Real-Time Notifications (SignalR)

```tsx
import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';

export const useSignalRNotifications = (hubUrl: string) => {
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', (message: string, type: string) => {
      switch (type) {
        case 'success': toast.success(message); break;
        case 'warning': toast.warning(message); break;
        case 'error':   toast.error(message);   break;
        default:        toast.info(message);
      }
    });

    connection.start().catch(err => {
      console.error('SignalR connection failed:', err);
    });

    return () => { connection.stop(); };
  }, [hubUrl]);
};
```

## Official References

- [Sonner](https://sonner.emilkowal.dev/)
- [React Hot Toast](https://react-hot-toast.com/)
- [WAI-ARIA Alert Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/alert/)
- [MDN ARIA live regions](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions)
- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
