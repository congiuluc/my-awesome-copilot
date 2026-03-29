---
description: "Use when configuring PWA features: service workers, web app manifest, offline support, caching strategies, install prompts, push notifications, or running Lighthouse PWA audits."
applyTo: "**/manifest.json,**/manifest.webmanifest,**/sw.js,**/sw.ts,**/service-worker.js,**/service-worker.ts,**/ngsw-config.json,**/workbox-config.*,**/vite.config.*"
---
# Progressive Web App (PWA) Guidelines

## PWA Requirements

- Serve the app over **HTTPS** (required for service workers).
- Include a **web app manifest** (`manifest.json`) linked in `<head>`.
- Register a **service worker** that intercepts fetch events.
- Provide icons at **192×192** and **512×512** minimum, plus a maskable variant.
- Set `<meta name="theme-color">` and `<meta name="viewport">` in HTML.
- Implement an **offline fallback page** returned by the service worker when the network is unavailable.

## Manifest Rules

- Always set `name`, `short_name`, `start_url`, `display`, `theme_color`, `background_color`, and `icons`.
- Use `"display": "standalone"` for native app feel.
- Use separate icon entries for `any` and `maskable` purposes — never combine them.
- Add `screenshots` with `form_factor: "wide"` and `"narrow"` for richer install UI.
- Limit `shortcuts` to 3–4 entries.

## Service Worker Rules

- Precache the **app shell** (HTML, CSS, JS, fonts, icons) during the install event.
- Use **NetworkFirst** for API responses — cache as fallback.
- Use **CacheFirst** for static assets with hashed filenames, images, and fonts.
- Use **StaleWhileRevalidate** for semi-static resources (avatars, configs).
- Clean up old caches during the activate event.
- Always call `self.skipWaiting()` and `self.clients.claim()` for immediate activation.

## Caching Rules

- Set `maxEntries` and `maxAgeSeconds` on every runtime cache to prevent unbounded growth.
- Use `purgeOnQuotaError: true` in Workbox expiration plugins.
- Version cache names and delete stale caches on activation.
- Set `networkTimeoutSeconds` on NetworkFirst strategies (5–10 seconds).

## Offline Support

- Include a static `offline.html` in precache for navigation fallback.
- Use IndexedDB (via `idb` library) for structured offline data persistence.
- Queue failed write operations and replay them on reconnection (background sync).
- Show an accessible offline indicator (`role="status"`, `aria-live="assertive"`).

## Install Prompt

- Intercept the `beforeinstallprompt` event and show a custom install UI.
- Never show the install banner if the app is already installed (check `display-mode: standalone`).
- Log install acceptance/dismissal for analytics.

## Framework Integration

- **React + Vite**: Use `vite-plugin-pwa` with `registerType: 'autoUpdate'`.
- **Angular**: Use `@angular/service-worker` (`ng add @angular/pwa`) with `ngsw-config.json`.
- Configure runtime caching rules in the plugin/config rather than writing raw SW code when possible.

## Testing

- Run **Lighthouse PWA audit** — target score 100.
- Test offline by toggling "Offline" in Chrome DevTools → Network tab.
- Verify install prompt fires on supported browsers.
- Test on real mobile devices (iOS Safari, Android Chrome).
- Validate manifest in Chrome DevTools → Application → Manifest.

## Performance

- Keep the service worker file small — minimize logic in the fetch handler.
- Use lazy loading for non-critical cached resources.
- Monitor cache storage usage with `navigator.storage.estimate()`.
- Use Workbox `globPatterns` to precache only essential assets.
