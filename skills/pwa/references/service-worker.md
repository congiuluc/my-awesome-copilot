# Service Worker Patterns

## Registration

Register the service worker early in your app entry point. Always check for support first.

### Basic Registration

```typescript
// src/sw-register.ts
export async function registerServiceWorker(): Promise<ServiceWorkerRegistration | null> {
  if (!('serviceWorker' in navigator)) {
    console.warn('Service workers not supported');
    return null;
  }

  try {
    const registration = await navigator.serviceWorker.register('/sw.js', {
      scope: '/',
    });

    registration.addEventListener('updatefound', () => {
      const newWorker = registration.installing;
      if (!newWorker) return;

      newWorker.addEventListener('statechange', () => {
        if (
          newWorker.state === 'activated' &&
          navigator.serviceWorker.controller
        ) {
          // New content available — notify user to refresh
          dispatchEvent(new CustomEvent('sw-update-available'));
        }
      });
    });

    return registration;
  } catch (error) {
    console.error('Service worker registration failed:', error);
    return null;
  }
}
```

### React Registration Hook

```typescript
// hooks/useServiceWorker.ts
import { useEffect, useState, useCallback } from 'react';

interface ServiceWorkerState {
  isSupported: boolean;
  isRegistered: boolean;
  isUpdateAvailable: boolean;
  registration: ServiceWorkerRegistration | null;
  update: () => Promise<void>;
}

export function useServiceWorker(): ServiceWorkerState {
  const [state, setState] = useState<Omit<ServiceWorkerState, 'update'>>({
    isSupported: 'serviceWorker' in navigator,
    isRegistered: false,
    isUpdateAvailable: false,
    registration: null,
  });

  useEffect(() => {
    if (!state.isSupported) return;

    let registration: ServiceWorkerRegistration;

    navigator.serviceWorker
      .register('/sw.js', { scope: '/' })
      .then((reg) => {
        registration = reg;
        setState((prev) => ({ ...prev, isRegistered: true, registration: reg }));

        reg.addEventListener('updatefound', () => {
          const newWorker = reg.installing;
          if (!newWorker) return;
          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'activated' && navigator.serviceWorker.controller) {
              setState((prev) => ({ ...prev, isUpdateAvailable: true }));
            }
          });
        });
      })
      .catch(console.error);

    return () => {
      registration?.unregister();
    };
  }, [state.isSupported]);

  const update = useCallback(async () => {
    if (state.registration) {
      await state.registration.update();
    }
  }, [state.registration]);

  return { ...state, update };
}
```

## Service Worker Lifecycle

```
┌──────────┐    ┌────────────┐    ┌───────────┐
│ Install   │───▶│  Waiting    │───▶│ Activate  │
│ (precache)│    │ (new SW     │    │ (cleanup  │
│           │    │  ready)     │    │  old cache)│
└──────────┘    └────────────┘    └───────────┘
                                        │
                                        ▼
                                  ┌───────────┐
                                  │   Fetch    │
                                  │ (intercept │
                                  │  requests) │
                                  └───────────┘
```

### Install Event — Precache Static Assets

```javascript
// sw.js
const CACHE_NAME = 'app-v1';
const PRECACHE_URLS = [
  '/',
  '/index.html',
  '/manifest.json',
  '/offline.html',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(PRECACHE_URLS))
  );
  // Activate immediately without waiting for old SW to stop
  self.skipWaiting();
});
```

### Activate Event — Clean Up Old Caches

```javascript
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) =>
      Promise.all(
        cacheNames
          .filter((name) => name !== CACHE_NAME)
          .map((name) => caches.delete(name))
      )
    )
  );
  // Take control of all open pages immediately
  self.clients.claim();
});
```

### Fetch Event — Intercept Requests

```javascript
self.addEventListener('fetch', (event) => {
  const { request } = event;

  // Skip non-GET requests
  if (request.method !== 'GET') return;

  // API requests: network-first
  if (request.url.includes('/api/')) {
    event.respondWith(networkFirst(request));
    return;
  }

  // Static assets: cache-first
  event.respondWith(cacheFirst(request));
});
```

## Update Strategies

### Auto-Update (Recommended for Most Apps)

The service worker updates automatically. Show a toast when a new version is available:

```typescript
// components/UpdateNotification.tsx
import { useServiceWorker } from '../hooks/useServiceWorker';

export function UpdateNotification() {
  const { isUpdateAvailable } = useServiceWorker();

  if (!isUpdateAvailable) return null;

  return (
    <div role="alert" className="fixed bottom-4 right-4 bg-blue-600 text-white
      rounded-lg p-4 shadow-lg z-50 flex items-center gap-3">
      <span>A new version is available!</span>
      <button
        onClick={() => window.location.reload()}
        className="bg-white text-blue-600 px-3 py-1 rounded font-medium
          hover:bg-blue-50 transition-colors min-h-11 min-w-11"
      >
        Refresh
      </button>
    </div>
  );
}
```

### Skip Waiting (Force Immediate Activation)

Use `skipWaiting()` in the install event and `clients.claim()` in activate. This makes the new SW take control immediately — suitable for apps where stale content is unacceptable.

## Workbox Integration

[Workbox](https://developer.chrome.com/docs/workbox/) simplifies service worker authoring:

```javascript
// sw.js (with Workbox)
import { precacheAndRoute } from 'workbox-precaching';
import { registerRoute } from 'workbox-routing';
import { NetworkFirst, CacheFirst, StaleWhileRevalidate } from 'workbox-strategies';
import { ExpirationPlugin } from 'workbox-expiration';
import { CacheableResponsePlugin } from 'workbox-cacheable-response';

// Precache static assets (injected by build tool)
precacheAndRoute(self.__WB_MANIFEST);

// API calls — network first
registerRoute(
  ({ url }) => url.pathname.startsWith('/api/'),
  new NetworkFirst({
    cacheName: 'api-responses',
    plugins: [
      new ExpirationPlugin({ maxEntries: 100, maxAgeSeconds: 60 * 60 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
    networkTimeoutSeconds: 10,
  })
);

// Images — cache first
registerRoute(
  ({ request }) => request.destination === 'image',
  new CacheFirst({
    cacheName: 'images',
    plugins: [
      new ExpirationPlugin({ maxEntries: 60, maxAgeSeconds: 30 * 24 * 60 * 60 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
  })
);

// Google Fonts — stale while revalidate
registerRoute(
  ({ url }) => url.origin === 'https://fonts.googleapis.com' ||
    url.origin === 'https://fonts.gstatic.com',
  new StaleWhileRevalidate({
    cacheName: 'google-fonts',
    plugins: [
      new ExpirationPlugin({ maxEntries: 30, maxAgeSeconds: 365 * 24 * 60 * 60 }),
    ],
  })
);
```

## Background Sync

Queue failed requests and replay them when connectivity is restored:

```javascript
// sw.js
import { BackgroundSyncPlugin } from 'workbox-background-sync';
import { registerRoute } from 'workbox-routing';
import { NetworkOnly } from 'workbox-strategies';

const bgSyncPlugin = new BackgroundSyncPlugin('api-queue', {
  maxRetentionTime: 24 * 60, // 24 hours in minutes
  onSync: async ({ queue }) => {
    let entry;
    while ((entry = await queue.shiftRequest())) {
      try {
        await fetch(entry.request);
      } catch (error) {
        await queue.unshiftRequest(entry);
        throw error;
      }
    }
  },
});

// POST/PUT/DELETE requests — network only with background sync
registerRoute(
  ({ url }) => url.pathname.startsWith('/api/'),
  new NetworkOnly({ plugins: [bgSyncPlugin] }),
  'POST'
);
```

## Security Considerations

- Service workers require HTTPS (localhost is exempt for development)
- Only serve the SW from the app's origin — never a CDN
- Set `scope` explicitly to limit the SW's control
- Validate all messages received via `postMessage`
- Use `importScripts` only for trusted, same-origin scripts
- Keep the SW file at the root to maximize scope
