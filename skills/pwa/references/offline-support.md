# Offline Support

## Offline Detection

### React Hook

```typescript
// hooks/useOnlineStatus.ts
import { useState, useEffect, useCallback } from 'react';

export function useOnlineStatus() {
  const [isOnline, setIsOnline] = useState(navigator.onLine);

  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  return isOnline;
}
```

### Offline Indicator Component

```typescript
// components/OfflineIndicator.tsx
import { useOnlineStatus } from '../hooks/useOnlineStatus';

export function OfflineIndicator() {
  const isOnline = useOnlineStatus();

  if (isOnline) return null;

  return (
    <div
      role="status"
      aria-live="assertive"
      className="fixed top-0 left-0 right-0 bg-amber-500 text-white
        text-center py-2 px-4 text-sm font-medium z-50 shadow-md"
    >
      You are offline. Some features may be unavailable.
    </div>
  );
}
```

## Offline Fallback Page

Create a dedicated offline page that the service worker serves when the network is
unavailable and no cached version exists.

### HTML Fallback

```html
<!-- public/offline.html -->
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <meta name="theme-color" content="#ffffff" />
  <title>Offline — MyApp</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: #f9fafb;
      color: #374151;
      padding: 1rem;
    }
    .container { text-align: center; max-width: 400px; }
    .icon { font-size: 4rem; margin-bottom: 1rem; }
    h1 { font-size: 1.5rem; margin-bottom: 0.5rem; }
    p { color: #6b7280; margin-bottom: 1.5rem; line-height: 1.5; }
    button {
      background: #3b82f6;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 0.5rem;
      font-size: 1rem;
      cursor: pointer;
      min-height: 44px;
    }
    button:hover { background: #2563eb; }
  </style>
</head>
<body>
  <main class="container">
    <div class="icon" aria-hidden="true">📡</div>
    <h1>You're offline</h1>
    <p>Check your internet connection and try again. Previously visited pages may still be available.</p>
    <button onclick="window.location.reload()">Try Again</button>
  </main>
</body>
</html>
```

### Service Worker — Serve Offline Fallback

```javascript
// sw.js — in the fetch event handler
self.addEventListener('fetch', (event) => {
  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request).catch(() =>
        caches.match('/offline.html')
      )
    );
    return;
  }

  // Handle other requests normally...
});
```

## Offline Data with IndexedDB

For offline-capable data persistence, use IndexedDB. The `idb` library provides
a Promise-based wrapper:

```bash
npm install idb
```

```typescript
// utils/offlineDb.ts
import { openDB, DBSchema, IDBPDatabase } from 'idb';

interface AppDB extends DBSchema {
  pendingActions: {
    key: string;
    value: {
      id: string;
      url: string;
      method: string;
      body: string;
      timestamp: number;
    };
    indexes: { 'by-timestamp': number };
  };
  cachedData: {
    key: string;
    value: {
      key: string;
      data: unknown;
      lastUpdated: number;
    };
  };
}

let dbInstance: IDBPDatabase<AppDB> | null = null;

async function getDb(): Promise<IDBPDatabase<AppDB>> {
  if (dbInstance) return dbInstance;

  dbInstance = await openDB<AppDB>('app-offline', 1, {
    upgrade(db) {
      const pendingStore = db.createObjectStore('pendingActions', {
        keyPath: 'id',
      });
      pendingStore.createIndex('by-timestamp', 'timestamp');

      db.createObjectStore('cachedData', { keyPath: 'key' });
    },
  });

  return dbInstance;
}

// Queue a failed API request for later replay
export async function queueAction(
  url: string,
  method: string,
  body: unknown
): Promise<void> {
  const db = await getDb();
  await db.put('pendingActions', {
    id: crypto.randomUUID(),
    url,
    method,
    body: JSON.stringify(body),
    timestamp: Date.now(),
  });
}

// Replay all pending actions when back online
export async function replayPendingActions(): Promise<void> {
  const db = await getDb();
  const actions = await db.getAllFromIndex(
    'pendingActions',
    'by-timestamp'
  );

  for (const action of actions) {
    try {
      const response = await fetch(action.url, {
        method: action.method,
        headers: { 'Content-Type': 'application/json' },
        body: action.body,
      });

      if (response.ok) {
        await db.delete('pendingActions', action.id);
      }
    } catch {
      // Still offline — stop replaying
      break;
    }
  }
}

// Cache data locally for offline reads
export async function cacheData(key: string, data: unknown): Promise<void> {
  const db = await getDb();
  await db.put('cachedData', { key, data, lastUpdated: Date.now() });
}

// Retrieve cached data
export async function getCachedData<T>(key: string): Promise<T | null> {
  const db = await getDb();
  const entry = await db.get('cachedData', key);
  return entry ? (entry.data as T) : null;
}
```

## Offline-Resilient API Client

Wrap fetch calls to automatically queue on failure:

```typescript
// utils/offlineApi.ts
import { queueAction, replayPendingActions } from './offlineDb';

export async function offlineFetch(
  url: string,
  options: RequestInit = {}
): Promise<Response | null> {
  try {
    const response = await fetch(url, options);
    return response;
  } catch {
    // For write operations, queue for later
    if (options.method && options.method !== 'GET') {
      await queueAction(url, options.method, options.body);
      return null; // Callers should handle null as "queued for later"
    }
    throw new Error('Network unavailable');
  }
}

// Auto-replay when connectivity is restored
if (typeof window !== 'undefined') {
  window.addEventListener('online', () => {
    replayPendingActions().catch(console.error);
  });
}
```

## Optimistic UI Pattern

Update the UI immediately and sync with the server in the background:

```typescript
// hooks/useOptimisticAction.ts
import { useState, useCallback } from 'react';
import { offlineFetch } from '../utils/offlineApi';

interface OptimisticState<T> {
  data: T;
  isSynced: boolean;
  error: string | null;
}

export function useOptimisticAction<T>(initialData: T) {
  const [state, setState] = useState<OptimisticState<T>>({
    data: initialData,
    isSynced: true,
    error: null,
  });

  const execute = useCallback(
    async (
      optimisticData: T,
      url: string,
      method: string,
      body: unknown
    ) => {
      // Apply optimistic update immediately
      setState({ data: optimisticData, isSynced: false, error: null });

      const response = await offlineFetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      });

      if (response?.ok) {
        setState((prev) => ({ ...prev, isSynced: true }));
      } else if (response === null) {
        // Queued for offline sync
        setState((prev) => ({ ...prev, error: null }));
      } else {
        setState((prev) => ({
          ...prev,
          error: 'Failed to save',
          isSynced: false,
        }));
      }
    },
    []
  );

  return { ...state, execute };
}
```

## Offline-Ready Patterns Summary

| Pattern | When to Use |
|---------|-------------|
| Cache-then-network | Read-heavy pages — show cached content instantly, update in background |
| Optimistic UI | Write operations — update UI instantly, sync later |
| Background sync | Queued writes — replay when connectivity returns |
| IndexedDB + service worker | Complex offline data with structured storage |
| Offline fallback page | Navigation to uncached pages when offline |
| Stale data indicator | Show users when they're viewing cached (potentially outdated) data |

## Testing Offline

1. **Chrome DevTools** → Network tab → check "Offline"
2. **Application tab** → Service Workers → check "Offline"
3. **Lighthouse** → Run PWA audit (checks offline fallback)
4. Enable airplane mode on a real mobile device
5. Test with flaky connections — throttle via Network tab ("Slow 3G")
