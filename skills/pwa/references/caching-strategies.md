# Caching Strategies

## Strategy Overview

| Strategy | Best For | Freshness | Offline | Speed |
|----------|----------|-----------|---------|-------|
| Cache First | Static assets, fonts, images | Low | Yes | Fast |
| Network First | API data, user content | High | Fallback | Variable |
| Stale While Revalidate | Semi-static (avatars, config) | Medium | Yes | Fast |
| Network Only | Auth, analytics, real-time | Always fresh | No | Variable |
| Cache Only | Precached assets | Static | Yes | Instant |

## Decision Tree

```
Is the resource static and versioned (hashed filenames)?
  → YES: Cache First (or precache)
  → NO: Does the user need the freshest data?
    → YES: Can the app tolerate stale data briefly?
      → YES: Stale While Revalidate
      → NO: Network First (with cache fallback)
    → NO: Does it need to work offline?
      → YES: Cache First
      → NO: Network Only
```

## Cache First (Cache Falling Back to Network)

Returns cached response immediately. Falls back to network on cache miss, then updates cache.

**Use for**: Static assets, fonts, images, versioned files (hashed filenames).

```javascript
async function cacheFirst(request) {
  const cache = await caches.open('static-v1');
  const cached = await cache.match(request);

  if (cached) return cached;

  const response = await fetch(request);
  if (response.ok) {
    cache.put(request, response.clone());
  }
  return response;
}
```

**Workbox:**

```javascript
import { CacheFirst } from 'workbox-strategies';
import { ExpirationPlugin } from 'workbox-expiration';
import { CacheableResponsePlugin } from 'workbox-cacheable-response';

registerRoute(
  ({ request }) => request.destination === 'image',
  new CacheFirst({
    cacheName: 'images',
    plugins: [
      new ExpirationPlugin({ maxEntries: 100, maxAgeSeconds: 30 * 24 * 60 * 60 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
  })
);
```

## Network First (Network Falling Back to Cache)

Tries network first. On failure (timeout or error), falls back to cache.

**Use for**: API data, frequently updated content, HTML pages.

```javascript
async function networkFirst(request) {
  const cache = await caches.open('api-v1');

  try {
    const response = await fetchWithTimeout(request, 5000);
    if (response.ok) {
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    const cached = await cache.match(request);
    return cached || new Response('Network error', { status: 503 });
  }
}

function fetchWithTimeout(request, timeoutMs) {
  return new Promise((resolve, reject) => {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);

    fetch(request, { signal: controller.signal })
      .then((response) => {
        clearTimeout(timer);
        resolve(response);
      })
      .catch(reject);
  });
}
```

**Workbox:**

```javascript
import { NetworkFirst } from 'workbox-strategies';

registerRoute(
  ({ url }) => url.pathname.startsWith('/api/'),
  new NetworkFirst({
    cacheName: 'api-responses',
    networkTimeoutSeconds: 5,
    plugins: [
      new ExpirationPlugin({ maxEntries: 50, maxAgeSeconds: 60 * 60 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
  })
);
```

## Stale While Revalidate

Returns cached response immediately while fetching an updated version in the background.
The cache is updated for the next request.

**Use for**: Assets that change occasionally — user avatars, non-critical config, social feeds.

```javascript
async function staleWhileRevalidate(request) {
  const cache = await caches.open('swr-v1');
  const cached = await cache.match(request);

  const fetchPromise = fetch(request).then((response) => {
    if (response.ok) {
      cache.put(request, response.clone());
    }
    return response;
  });

  return cached || fetchPromise;
}
```

**Workbox:**

```javascript
import { StaleWhileRevalidate } from 'workbox-strategies';

registerRoute(
  ({ url }) => url.pathname.startsWith('/config/'),
  new StaleWhileRevalidate({
    cacheName: 'config',
    plugins: [
      new ExpirationPlugin({ maxEntries: 20, maxAgeSeconds: 24 * 60 * 60 }),
    ],
  })
);
```

## Precaching

Cache specific URLs during the service worker install event. These are available
immediately on the next page load — guaranteed offline.

**Use for**: App shell (HTML, CSS, JS), offline fallback page, critical fonts.

```javascript
const PRECACHE_URLS = [
  '/',
  '/index.html',
  '/offline.html',
  '/styles/main.css',
  '/scripts/app.js',
  '/fonts/inter.woff2',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open('precache-v1').then((cache) => cache.addAll(PRECACHE_URLS))
  );
});
```

**Workbox** (auto-injected by build tools):

```javascript
import { precacheAndRoute } from 'workbox-precaching';
precacheAndRoute(self.__WB_MANIFEST);
```

## Cache Management

### Cache Size Limits

Browsers have storage limits (varies by browser/device). Use expiration to prevent
unbounded growth:

```javascript
// Workbox expiration
new ExpirationPlugin({
  maxEntries: 100,          // Max number of cached entries
  maxAgeSeconds: 7 * 24 * 60 * 60,  // 7 days
  purgeOnQuotaError: true,  // Auto-purge on storage pressure
})
```

### Cache Versioning

Use versioned cache names and clean up old caches on activation:

```javascript
const CACHE_VERSION = 'v2';

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((names) =>
      Promise.all(
        names
          .filter((name) => !name.endsWith(CACHE_VERSION))
          .map((name) => caches.delete(name))
      )
    )
  );
});
```

### Storage Estimate

Check available storage before large cache operations:

```javascript
async function checkStorageQuota() {
  if ('storage' in navigator && 'estimate' in navigator.storage) {
    const { usage, quota } = await navigator.storage.estimate();
    const percentUsed = ((usage ?? 0) / (quota ?? 1)) * 100;
    console.log(`Storage: ${percentUsed.toFixed(1)}% used (${usage} / ${quota})`);
    return { usage: usage ?? 0, quota: quota ?? 0, percentUsed };
  }
  return null;
}
```

## Recommended Strategy Map

| Resource Type | Strategy | Cache Name | Max Entries | Max Age |
|--------------|----------|------------|-------------|---------|
| App shell (HTML, CSS, JS) | Precache | `precache` | — | Until update |
| API responses | Network First | `api-responses` | 100 | 1 hour |
| Images | Cache First | `images` | 100 | 30 days |
| Fonts | Cache First | `fonts` | 20 | 1 year |
| User avatars | Stale While Revalidate | `avatars` | 50 | 7 days |
| Third-party scripts | Stale While Revalidate | `third-party` | 30 | 1 day |
| Analytics / Auth | Network Only | — | — | — |
