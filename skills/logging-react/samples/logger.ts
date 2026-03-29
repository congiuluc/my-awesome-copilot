// Sample: Structured logger utility for React + Vite
// Configurable log levels, sensitive data filtering, backend shipping.

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
  level: LogLevel;
  message: string;
  context?: Record<string, unknown>;
  timestamp: string;
  correlationId?: string;
}

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
const LOG_LEVELS: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
};

const currentLevel: LogLevel = import.meta.env.PROD ? 'warn' : 'debug';
const LOG_ENDPOINT = import.meta.env.VITE_LOG_ENDPOINT as string | undefined;

let _correlationId: string | undefined;

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------
export const logger = {
  debug: (message: string, context?: Record<string, unknown>) =>
    log('debug', message, context),
  info: (message: string, context?: Record<string, unknown>) =>
    log('info', message, context),
  warn: (message: string, context?: Record<string, unknown>) =>
    log('warn', message, context),
  error: (message: string, context?: Record<string, unknown>) =>
    log('error', message, context),
  setCorrelationId: (id: string) => {
    _correlationId = id;
  },
};

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------
function shouldLog(level: LogLevel): boolean {
  return LOG_LEVELS[level] >= LOG_LEVELS[currentLevel];
}

function log(
  level: LogLevel,
  message: string,
  context?: Record<string, unknown>,
): void {
  if (!shouldLog(level)) return;

  const entry: LogEntry = {
    level,
    message,
    context: filterSensitiveData(context),
    timestamp: new Date().toISOString(),
    correlationId: _correlationId,
  };

  // Dev: console output
  console[level](`[${entry.level.toUpperCase()}]`, entry.message, entry.context ?? '');

  // Prod: ship error-level logs to backend
  if (level === 'error') {
    shipToBackend(entry);
  }
}

// ---------------------------------------------------------------------------
// Sensitive data filter
// ---------------------------------------------------------------------------
const SENSITIVE_KEYS = new Set([
  'password',
  'token',
  'secret',
  'apikey',
  'authorization',
  'creditcard',
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

// ---------------------------------------------------------------------------
// Backend log shipping (fire-and-forget for errors)
// ---------------------------------------------------------------------------
function shipToBackend(entry: LogEntry): void {
  if (!LOG_ENDPOINT) return;
  try {
    void fetch(LOG_ENDPOINT, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(entry),
      keepalive: true,
    });
  } catch {
    // Silently fail — logging should never break the app.
  }
}
