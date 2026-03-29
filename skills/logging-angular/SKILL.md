---
name: logging-angular
description: >-
  Configure structured logging for Angular frontend applications. Covers
  custom LoggerService, ErrorHandler integration, HTTP interceptor logging,
  correlation IDs, and log shipping to backend services. Use when: setting up
  frontend logging, capturing errors, correlating frontend requests with backend
  traces, or debugging production issues in Angular.
argument-hint: 'Describe the logging requirement: error capture, log shipping, correlation, or structured context.'
---

# Frontend Logging (Angular)

## When to Use

- Setting up client-side logging in an Angular 19 project
- Capturing and reporting errors from the global ErrorHandler
- Logging HTTP call failures with correlation IDs
- Shipping client logs to a backend endpoint or monitoring service
- Debugging production issues with structured context

## Official Documentation

- [Angular ErrorHandler](https://angular.dev/api/core/ErrorHandler)
- [Angular HttpInterceptorFn](https://angular.dev/guide/http/interceptors)

## Key Principles

- **Never use `console.log` in production** — use a `LoggerService` that can be configured per environment.
- **Correlation IDs** — attach the same correlation ID from API responses to frontend logs.
- **Sensitive data never logged** — filter user PII, tokens, passwords before logging.
- **Levels are meaningful** — debug for dev, warn/error only in production.
- **Ship errors to backend** — critical errors should be reported to a collection endpoint.

## Procedure

1. Create a `LoggerService` with configurable log levels
2. Review [LoggerService sample](./samples/logger.service.ts)
3. Implement a custom `ErrorHandler` that delegates to the logger
4. Add an HTTP interceptor for API error logging
5. Attach correlation IDs from API response headers
6. Ship error-level logs to a backend collection endpoint
7. Disable debug/info logs in production builds via `environment.ts`

## LoggerService

```typescript
import { Injectable, inject } from '@angular/core';
import { environment } from '../environments/environment';

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
  level: LogLevel;
  message: string;
  context?: Record<string, unknown>;
  timestamp: string;
  correlationId?: string;
}

const LOG_LEVELS: Record<LogLevel, number> = { debug: 0, info: 1, warn: 2, error: 3 };

@Injectable({ providedIn: 'root' })
export class LoggerService {
  private readonly minLevel: LogLevel = environment.production ? 'warn' : 'debug';
  private correlationId?: string;

  setCorrelationId(id: string): void {
    this.correlationId = id;
  }

  debug(message: string, context?: Record<string, unknown>): void {
    this.log('debug', message, context);
  }
  info(message: string, context?: Record<string, unknown>): void {
    this.log('info', message, context);
  }
  warn(message: string, context?: Record<string, unknown>): void {
    this.log('warn', message, context);
  }
  error(message: string, context?: Record<string, unknown>): void {
    this.log('error', message, context);
  }

  private log(level: LogLevel, message: string, context?: Record<string, unknown>): void {
    if (LOG_LEVELS[level] < LOG_LEVELS[this.minLevel]) return;

    const entry: LogEntry = {
      level,
      message,
      context: this.filterSensitive(context),
      timestamp: new Date().toISOString(),
      correlationId: this.correlationId,
    };

    console[level](`[${level.toUpperCase()}]`, entry.message, entry.context ?? '');

    if (level === 'error') {
      this.shipToBackend(entry);
    }
  }

  private filterSensitive(ctx?: Record<string, unknown>): Record<string, unknown> | undefined {
    if (!ctx) return undefined;
    const filtered = { ...ctx };
    const sensitiveKeys = new Set(['password', 'token', 'secret', 'apikey', 'authorization']);
    for (const key of Object.keys(filtered)) {
      if (sensitiveKeys.has(key.toLowerCase())) {
        filtered[key] = '***REDACTED***';
      }
    }
    return filtered;
  }

  private shipToBackend(entry: LogEntry): void {
    const endpoint = environment.logEndpoint;
    if (!endpoint) return;
    fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(entry),
      keepalive: true,
    }).catch(() => { /* logging should never break the app */ });
  }
}
```

## Global ErrorHandler

```typescript
import { ErrorHandler, Injectable, inject } from '@angular/core';
import { LoggerService } from './logger.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly logger = inject(LoggerService);

  handleError(error: unknown): void {
    const message = error instanceof Error ? error.message : String(error);
    const stack = error instanceof Error ? error.stack : undefined;
    this.logger.error('Unhandled error', { message, stack });
  }
}

// Register in app.config.ts:
// providers: [{ provide: ErrorHandler, useClass: GlobalErrorHandler }]
```

## HTTP Logging Interceptor

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap, catchError, throwError } from 'rxjs';
import { LoggerService } from './logger.service';

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  const logger = inject(LoggerService);
  const correlationId = req.headers.get('X-Correlation-ID');
  if (correlationId) {
    logger.setCorrelationId(correlationId);
  }

  return next(req).pipe(
    catchError((error) => {
      logger.error('HTTP request failed', {
        url: req.url,
        method: req.method,
        status: error.status,
        message: error.message,
      });
      return throwError(() => error);
    }),
  );
};
```
