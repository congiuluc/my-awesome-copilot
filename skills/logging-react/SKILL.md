---
name: logging-react
description: >-
  Configure structured logging for React frontend applications. Covers
  client-side log collection, error boundary logging, API error logging,
  correlation IDs, and log shipping to backend services. Use when: setting up
  frontend logging, capturing errors, correlating frontend requests with backend
  traces, or debugging production issues in React.
argument-hint: 'Describe the logging requirement: error capture, log shipping, correlation, or structured context.'
---

# Frontend Logging (React)

## When to Use

- Setting up client-side logging in a React + Vite project
- Capturing and reporting errors from error boundaries
- Logging API call failures with correlation IDs
- Shipping client logs to a backend endpoint or monitoring service
- Debugging production issues with structured context

## Official Documentation

- [Error Boundaries](https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary)
- [Web Console API](https://developer.mozilla.org/en-US/docs/Web/API/console)

## Key Principles

- **Never use `console.log` in production** — use a structured logger that can be configured per environment.
- **Correlation IDs** — attach the same correlation ID from API responses to frontend logs.
- **Sensitive data never logged** — filter user PII, tokens, passwords before logging.
- **Levels are meaningful** — debug for dev, warn/error only in production.
- **Ship errors to backend** — critical errors should be reported to a collection endpoint.

## Procedure

1. Create a logger utility with configurable log levels
2. Review [logger utility sample](./samples/logger.ts)
3. Integrate with error boundaries to capture component errors
4. Attach correlation IDs from API response headers
5. Ship error-level logs to a backend collection endpoint
6. Disable debug/info logs in production builds

## Logger Utility

```typescript
type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
  level: LogLevel;
  message: string;
  context?: Record<string, unknown>;
  timestamp: string;
  correlationId?: string;
}

const LOG_LEVELS: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
};

const currentLevel: LogLevel = import.meta.env.PROD ? 'warn' : 'debug';

function shouldLog(level: LogLevel): boolean {
  return LOG_LEVELS[level] >= LOG_LEVELS[currentLevel];
}

export const logger = {
  debug: (message: string, context?: Record<string, unknown>) =>
    log('debug', message, context),
  info: (message: string, context?: Record<string, unknown>) =>
    log('info', message, context),
  warn: (message: string, context?: Record<string, unknown>) =>
    log('warn', message, context),
  error: (message: string, context?: Record<string, unknown>) =>
    log('error', message, context),
};

function log(level: LogLevel, message: string, context?: Record<string, unknown>): void {
  if (!shouldLog(level)) return;

  const entry: LogEntry = {
    level,
    message,
    context: filterSensitiveData(context),
    timestamp: new Date().toISOString(),
  };

  console[level](JSON.stringify(entry));

  if (level === 'error') {
    shipToBackend(entry);
  }
}
```

## Error Boundary Integration

```tsx
import { Component, type ErrorInfo, type ReactNode } from 'react';
import { logger } from '@/utils/logger';

interface ErrorBoundaryProps {
  fallback: ReactNode;
  children: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false };

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    logger.error('React error boundary caught error', {
      error: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack ?? undefined,
    });
  }

  render(): ReactNode {
    return this.state.hasError ? this.props.fallback : this.props.children;
  }
}
```

## Sensitive Data Filter

```typescript
const SENSITIVE_KEYS = new Set([
  'password', 'token', 'secret', 'apiKey', 'authorization', 'creditCard',
]);

function filterSensitiveData(
  context?: Record<string, unknown>,
): Record<string, unknown> | undefined {
  if (!context) return undefined;
  const filtered = { ...context };
  for (const key of Object.keys(filtered)) {
    if (SENSITIVE_KEYS.has(key.toLowerCase())) {
      filtered[key] = '***REDACTED***';
    }
  }
  return filtered;
}
```
