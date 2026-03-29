// Sample: React audit service with batching, consent, and page view tracking
// Referenced by the audit-frontend SKILL.md

import { v4 as uuidv4 } from 'uuid';

export interface AuditEvent {
  eventId: string;
  timestamp: string;
  action: string;
  page: string;
  context?: Record<string, unknown>;
  sessionId: string;
}

const SESSION_ID = uuidv4();
let consentGiven = false;
let eventQueue: AuditEvent[] = [];
let flushTimer: ReturnType<typeof setInterval> | null = null;

/** Call once when the user grants tracking consent. */
export function setAuditConsent(granted: boolean): void {
  consentGiven = granted;
  if (granted && !flushTimer) {
    flushTimer = setInterval(flushEvents, 5000);
  }
  if (!granted && flushTimer) {
    clearInterval(flushTimer);
    flushTimer = null;
  }
}

/** Track a user action. No-op if consent not given. */
export function trackAction(
  action: string,
  page: string,
  context?: Record<string, unknown>,
): void {
  if (!consentGiven) return;

  eventQueue.push({
    eventId: uuidv4(),
    timestamp: new Date().toISOString(),
    action,
    page,
    context,
    sessionId: SESSION_ID,
  });
}

/** Track a page view. Call from your router's navigation listener. */
export function trackPageView(url: string): void {
  trackAction('page_view', url, { url });
}

async function flushEvents(): Promise<void> {
  if (eventQueue.length === 0) return;

  const batch = [...eventQueue];
  eventQueue = [];

  try {
    await fetch('/api/audit/events', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(batch),
    });
  } catch {
    // Silently discard — audit failures must never break the UI
  }
}
