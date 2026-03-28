// Sample: Shared UI Components — Button, Card, Input, Badge, Toast, Spinner
// All components use named exports, Props suffix, cn() utility, and are accessible.

import { forwardRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';

// ============================================================
// BUTTON
// ============================================================

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  icon?: ReactNode;
}

const buttonVariants = {
  primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
  secondary: 'bg-gray-600 text-white hover:bg-gray-700 focus:ring-gray-500',
  outline: 'border border-gray-300 text-gray-700 hover:bg-gray-50 focus:ring-blue-500',
  ghost: 'text-gray-700 hover:bg-gray-100 focus:ring-blue-500',
  danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
} as const;

const buttonSizes = {
  sm: 'px-3 py-1.5 text-sm min-h-9',
  md: 'px-4 py-2 text-base min-h-11',
  lg: 'px-6 py-3 text-lg min-h-12',
} as const;

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, icon, className, children, disabled, ...props }, ref) => (
    <button
      ref={ref}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-lg font-medium',
        'transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2',
        buttonVariants[variant],
        buttonSizes[size],
        (disabled || loading) && 'cursor-not-allowed opacity-50',
        className,
      )}
      disabled={disabled || loading}
      aria-busy={loading}
      {...props}
    >
      {loading ? <Spinner size="sm" /> : icon}
      {children}
    </button>
  ),
);
Button.displayName = 'Button';

// ============================================================
// CARD
// ============================================================

export interface CardProps {
  children: ReactNode;
  className?: string;
  onClick?: () => void;
  header?: ReactNode;
  footer?: ReactNode;
}

export const Card = ({ children, className, onClick, header, footer }: CardProps) => (
  <div
    className={cn(
      'rounded-lg border border-gray-200 bg-white shadow-sm',
      onClick && 'cursor-pointer transition-shadow hover:shadow-md',
      className,
    )}
    onClick={onClick}
    onKeyDown={onClick ? (e) => e.key === 'Enter' && onClick() : undefined}
    role={onClick ? 'button' : undefined}
    tabIndex={onClick ? 0 : undefined}
  >
    {header && <div className="border-b border-gray-200 px-4 py-3 font-semibold">{header}</div>}
    <div className="p-4">{children}</div>
    {footer && <div className="border-t border-gray-200 px-4 py-3">{footer}</div>}
  </div>
);

// ============================================================
// INPUT
// ============================================================

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
  icon?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, icon, className, id, ...props }, ref) => {
    const inputId = id || label.toLowerCase().replace(/\s+/g, '-');
    const errorId = error ? `${inputId}-error` : undefined;
    const helperId = helperText ? `${inputId}-helper` : undefined;

    return (
      <div className="space-y-1">
        <label htmlFor={inputId} className="block text-sm font-medium text-gray-700">
          {label}
        </label>
        <div className="relative">
          {icon && (
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
              {icon}
            </span>
          )}
          <input
            ref={ref}
            id={inputId}
            className={cn(
              'block w-full rounded-lg border px-3 py-2 min-h-11',
              'focus:outline-none focus:ring-2 focus:ring-offset-0 focus:ring-blue-500',
              'placeholder:text-gray-400',
              icon && 'pl-10',
              error ? 'border-red-500 text-red-900' : 'border-gray-300',
              className,
            )}
            aria-invalid={!!error}
            aria-describedby={[errorId, helperId].filter(Boolean).join(' ') || undefined}
            {...props}
          />
        </div>
        {error && (
          <p id={errorId} className="text-sm text-red-600" role="alert">
            {error}
          </p>
        )}
        {helperText && !error && (
          <p id={helperId} className="text-sm text-gray-500">
            {helperText}
          </p>
        )}
      </div>
    );
  },
);
Input.displayName = 'Input';

// ============================================================
// BADGE
// ============================================================

export interface BadgeProps {
  children: ReactNode;
  color?: 'gray' | 'blue' | 'green' | 'red' | 'yellow';
  onRemove?: () => void;
}

const badgeColors = {
  gray: 'bg-gray-100 text-gray-700',
  blue: 'bg-blue-100 text-blue-700',
  green: 'bg-green-100 text-green-700',
  red: 'bg-red-100 text-red-700',
  yellow: 'bg-yellow-100 text-yellow-800',
} as const;

export const Badge = ({ children, color = 'gray', onRemove }: BadgeProps) => (
  <span
    className={cn(
      'inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-sm font-medium',
      badgeColors[color],
    )}
  >
    {children}
    {onRemove && (
      <button
        type="button"
        className="ml-0.5 inline-flex h-4 w-4 items-center justify-center rounded-full
          hover:bg-black/10 focus:outline-none focus:ring-2 focus:ring-offset-1"
        onClick={onRemove}
        aria-label={`Remove ${children}`}
      >
        <span aria-hidden="true">&times;</span>
      </button>
    )}
  </span>
);

// ============================================================
// TOAST
// ============================================================

export interface ToastProps {
  message: string;
  type?: 'success' | 'error' | 'warning' | 'info';
  onDismiss?: () => void;
}

const toastStyles = {
  success: 'bg-green-50 border-green-200 text-green-800',
  error: 'bg-red-50 border-red-200 text-red-800',
  warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
  info: 'bg-blue-50 border-blue-200 text-blue-800',
} as const;

export const Toast = ({ message, type = 'info', onDismiss }: ToastProps) => (
  <div
    className={cn(
      'flex items-center gap-3 rounded-lg border p-4 shadow-sm',
      toastStyles[type],
    )}
    role="alert"
    aria-live="polite"
  >
    <p className="flex-1 text-sm font-medium">{message}</p>
    {onDismiss && (
      <button
        type="button"
        className="min-h-8 min-w-8 rounded-full p-1 hover:bg-black/10
          focus:outline-none focus:ring-2 focus:ring-offset-1"
        onClick={onDismiss}
        aria-label="Dismiss"
      >
        <span aria-hidden="true">&times;</span>
      </button>
    )}
  </div>
);

// ============================================================
// SPINNER
// ============================================================

export interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const spinnerSizes = {
  sm: 'h-4 w-4',
  md: 'h-6 w-6',
  lg: 'h-10 w-10',
} as const;

export const Spinner = ({ size = 'md', className }: SpinnerProps) => (
  <svg
    className={cn('animate-spin text-blue-600', spinnerSizes[size], className)}
    xmlns="http://www.w3.org/2000/svg"
    fill="none"
    viewBox="0 0 24 24"
    aria-hidden="true"
  >
    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
    <path
      className="opacity-75"
      fill="currentColor"
      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
    />
  </svg>
);
