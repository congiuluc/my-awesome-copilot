# Frontend Error Handling

## Error Boundary

Catches unhandled errors in the React component tree:

```tsx
import { Component, type ErrorInfo, type ReactNode } from 'react';

export interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('ErrorBoundary caught:', error, errorInfo);
    // Send to error reporting service
  }

  render(): ReactNode {
    if (this.state.hasError) {
      return this.props.fallback ?? (
        <div role="alert" className="p-6 text-center">
          <h2 className="text-xl font-semibold text-red-600">Something went wrong</h2>
          <p className="mt-2 text-gray-600">Please try refreshing the page.</p>
          <button
            onClick={() => this.setState({ hasError: false, error: null })}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Try again
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
```

Usage in App:

```tsx
export const App = () => (
  <ErrorBoundary>
    <Router />
  </ErrorBoundary>
);
```

## API Error Handling

### Typed Error State

```tsx
export type AsyncState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'error'; error: string };
```

### useApi Hook with Error Handling

```tsx
export const useApi = <T,>(fetcher: () => Promise<ApiResponse<T>>) => {
  const [state, setState] = useState<AsyncState<T>>({ status: 'idle' });

  const execute = useCallback(async () => {
    setState({ status: 'loading' });
    try {
      const response = await fetcher();
      if (response.success && response.data !== null) {
        setState({ status: 'success', data: response.data });
      } else {
        setState({ status: 'error', error: response.error ?? 'Unknown error' });
      }
    } catch {
      setState({ status: 'error', error: 'Network error. Please check your connection.' });
    }
  }, [fetcher]);

  return { state, execute };
};
```

## Reusable Error Components

```tsx
export interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export const ErrorMessage = ({ message, onRetry }: ErrorMessageProps) => (
  <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4">
    <p className="text-sm text-red-800">{message}</p>
    {onRetry && (
      <button
        onClick={onRetry}
        className="mt-2 text-sm text-red-600 underline hover:text-red-800"
      >
        Try again
      </button>
    )}
  </div>
);

export const EmptyState = ({ message }: { message: string }) => (
  <div className="py-12 text-center text-gray-500">
    <p>{message}</p>
  </div>
);

export const Spinner = ({ label = 'Loading...' }: { label?: string }) => (
  <div role="status" className="flex items-center justify-center py-8">
    <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent" />
    <span className="sr-only">{label}</span>
  </div>
);
```

## Toast Notifications

```tsx
export interface ToastProps {
  message: string;
  type: 'success' | 'error' | 'info';
  onDismiss: () => void;
}

export const Toast = ({ message, type, onDismiss }: ToastProps) => (
  <div
    role="alert"
    aria-live="assertive"
    className={cn(
      'fixed bottom-4 right-4 z-50 rounded-lg px-4 py-3 shadow-lg transition-all',
      {
        'bg-green-600 text-white': type === 'success',
        'bg-red-600 text-white': type === 'error',
        'bg-blue-600 text-white': type === 'info',
      }
    )}
  >
    <div className="flex items-center gap-2">
      <span>{message}</span>
      <button onClick={onDismiss} aria-label="Dismiss notification" className="ml-2">
        ×
      </button>
    </div>
  </div>
);
```

## Rules

- Every data-fetching component must handle: loading, success, error, empty states.
- Use `role="alert"` or `aria-live` for error messages that appear dynamically.
- Never show raw HTTP error codes or technical messages to users.
- Provide actionable recovery options: retry button, navigation to home, contact support.
- Log errors to console in development, to monitoring service in production.
- Use `ErrorBoundary` at route level to prevent full-app crashes.
