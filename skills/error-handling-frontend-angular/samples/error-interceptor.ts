// Sample: Angular functional HTTP error interceptor with toast notifications

import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';

/**
 * Functional HTTP interceptor that catches HTTP errors,
 * shows user-friendly toast messages, and re-throws for
 * component-level handling.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message: string;

      switch (true) {
        case error.status === 0:
          message = 'Network error. Please check your connection.';
          break;
        case error.status === 401:
          message = 'Session expired. Please log in again.';
          break;
        case error.status === 403:
          message = 'You do not have permission to perform this action.';
          break;
        case error.status === 404:
          message = 'The requested resource was not found.';
          break;
        case error.status === 409:
          message = 'A conflict occurred. Please refresh and try again.';
          break;
        case error.status === 422:
          message = error.error?.error ?? 'The request could not be processed.';
          break;
        case error.status >= 500:
          message = 'Server error. Please try again later.';
          break;
        default:
          message = 'An unexpected error occurred.';
      }

      toast.error(message);
      console.error(`[HTTP ${error.status}] ${req.method} ${req.url}`, error);

      return throwError(() => new Error(message));
    })
  );
};
