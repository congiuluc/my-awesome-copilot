// Sample: React Error Boundary with fallback UI
// Shows ErrorBoundary component, useErrorBoundary hook, and API error handling.

import { Component, type ReactNode } from 'react';
import { Button } from '@/components/shared/Button';

// --- Error Boundary Props ---

export interface ErrorBoundaryProps {
  /** Content to render when no error. */
  children: ReactNode;
  /** Optional custom fallback component. */
  fallback?: ReactNode;
  /** Callback when an error is caught. */
  onError?: (error: Error, errorInfo: React.ErrorInfo) => void;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

// --- Error Boundary Component ---

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo): void {
    console.error('ErrorBoundary caught:', error, errorInfo);
    this.props.onError?.(error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div
          className="mx-auto max-w-md rounded-lg border border-red-200 bg-red-50 p-6 text-center"
          role="alert"
        >
          <h2 className="text-lg font-semibold text-red-800">Something went wrong</h2>
          <p className="mt-2 text-sm text-red-600">
            {this.state.error?.message || 'An unexpected error occurred.'}
          </p>
          <Button variant="outline" className="mt-4" onClick={this.handleReset}>
            Try again
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}

// --- API Error Handler Hook ---

export interface ApiError {
  success: false;
  data: null;
  error: string;
}

export const useApiError = () => {
  const handleApiError = (error: unknown): string => {
    if (error instanceof Response) {
      switch (error.status) {
        case 400:
          return 'Invalid request. Please check your input.';
        case 401:
          return 'Please log in to continue.';
        case 403:
          return 'You do not have permission for this action.';
        case 404:
          return 'The requested resource was not found.';
        case 429:
          return 'Too many requests. Please try again later.';
        default:
          return 'An unexpected error occurred.';
      }
    }

    if (error instanceof Error) {
      if (error.name === 'AbortError') {
        return 'Request was cancelled.';
      }
      return error.message;
    }

    return 'An unexpected error occurred.';
  };

  return { handleApiError };
};

// --- Usage in App ---
// <ErrorBoundary onError={(err) => logToService(err)}>
//   <App />
// </ErrorBoundary>
