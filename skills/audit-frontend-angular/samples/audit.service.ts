import { inject, Injectable, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, NavigationEnd } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { filter, bufferTime } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';

export interface AuditEvent {
  eventId: string;
  timestamp: string;
  action: string;
  page: string;
  context?: Record<string, unknown>;
  sessionId: string;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);

  private readonly eventQueue = new Subject<AuditEvent>();
  private readonly sessionId = uuidv4();
  private consentGiven = false;

  constructor() {
    // Batch events every 5 seconds, ship if non-empty
    this.eventQueue
      .pipe(
        bufferTime(5000),
        filter((batch) => batch.length > 0),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((batch) => this.shipEvents(batch));

    // Auto-track page views on navigation
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((e) => this.trackPageView(e.urlAfterRedirects));
  }

  /** Call this when the user grants tracking consent. */
  setConsent(granted: boolean): void {
    this.consentGiven = granted;
  }

  /** Track a user action. No-op if consent not given. */
  trackAction(action: string, context?: Record<string, unknown>): void {
    if (!this.consentGiven) return;

    this.eventQueue.next({
      eventId: uuidv4(),
      timestamp: new Date().toISOString(),
      action,
      page: this.router.url,
      context,
      sessionId: this.sessionId,
    });
  }

  private trackPageView(url: string): void {
    this.trackAction('page_view', { url });
  }

  private shipEvents(batch: AuditEvent[]): void {
    this.http
      .post('/api/audit/events', batch)
      .subscribe({
        error: () => {
          // Silently discard — audit failures must never break the UI
        },
      });
  }
}
