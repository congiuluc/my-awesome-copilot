// Sample: Angular LoggerService with correlation IDs and log shipping
// Place in src/app/core/services/logger.service.ts

import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

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
    const endpoint = (environment as Record<string, unknown>)['logEndpoint'] as string | undefined;
    if (!endpoint) return;
    fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(entry),
    }).catch(() => {
      // Silently fail — don't cause cascading errors from log shipping
    });
  }
}
