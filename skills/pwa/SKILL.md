---
name: pwa
description: >-
  Build Progressive Web Apps with service workers, web app manifest, caching
  strategies, and offline support. Use when: making app installable, adding
  offline mode, configuring service worker, caching API responses, push
  notifications, background sync, creating web manifest, PWA audit, Lighthouse
  PWA score, workbox configuration, app shell architecture, precaching,
  runtime caching, install prompt, or converting SPA to PWA.
argument-hint: 'Describe the PWA feature to implement (e.g., offline support, install prompt, caching strategy).'
---

# Progressive Web App (PWA)

## When to Use

- Making a web app installable on desktop and mobile devices
- Adding offline support and resilience to network failures
- Configuring a service worker for caching and background tasks
- Implementing caching strategies (cache-first, network-first, stale-while-revalidate)
- Setting up a web app manifest with icons, theme, and display mode
- Adding push notifications or background sync
- Implementing an app shell architecture for instant loading
- Customizing the install prompt experience
- Auditing and improving Lighthouse PWA score
- Converting an existing SPA (React, Angular) to a PWA

## Official Documentation

- [web.dev PWA Guide](https://web.dev/progressive-web-apps/)
- [MDN Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Workbox Documentation](https://developer.chrome.com/docs/workbox/)
- [Web App Manifest Spec](https://www.w3.org/TR/appmanifest/)
- [Service Worker API (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Push API (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Push_API)
- [Background Sync API](https://developer.mozilla.org/en-US/docs/Web/API/Background_Synchronization_API)
- [Vite PWA Plugin](https://vite-pwa-org.netlify.app/)
- [Lighthouse PWA Audits](https://developer.chrome.com/docs/lighthouse/pwa/)

## Procedure

1. Add [web app manifest](./references/manifest.md) to the project
2. Implement the [service worker](./references/service-worker.md) with appropriate registration
3. Choose and apply [caching strategies](./references/caching-strategies.md) for assets and API calls
4. Implement [offline support](./references/offline-support.md) with fallback pages and queued actions
5. Review [sample PWA setup](./samples/pwa-setup-react.tsx) for a complete React + Vite example
6. Add install prompt handling (see [Install Prompt](#install-prompt) below)
7. Test with Lighthouse PWA audit — target 100 score
8. Validate on multiple devices (mobile, tablet, desktop)

## Core Architecture

```
┌────────────────────────────────────────────┐
│                  Browser                    │
│  ┌──────────────────────────────────────┐  │
│  │            App Shell                  │  │
│  │  (HTML + CSS + JS — cached locally)   │  │
│  └──────────────┬───────────────────────┘  │
│                 │                           │
│  ┌──────────────▼───────────────────────┐  │
│  │         Service Worker                │  │
│  │  ┌───────────┐  ┌─────────────────┐  │  │
│  │  │ Precache  │  │ Runtime Cache   │  │  │
│  │  │ (static)  │  │ (API + dynamic) │  │  │
│  │  └───────────┘  └─────────────────┘  │  │
│  └──────────────┬───────────────────────┘  │
│                 │                           │
└─────────────────┼──────────────────────────┘
                  │
          ┌───────▼───────┐
          │    Network     │
          │  (origin API)  │
          └───────────────┘
```

## Quick Reference — PWA Checklist

| Requirement | How to Meet |
|-------------|-------------|
| HTTPS | Serve over HTTPS (required for service workers) |
| Web App Manifest | `manifest.json` linked in `<head>` with name, icons, display |
| Service Worker | Register SW that controls fetch events |
| Installable | Valid manifest + registered SW + HTTPS |
| Offline Fallback | SW returns cached page when network unavailable |
| Icons | At least 192×192 and 512×512 PNG icons |
| Splash Screen | `theme_color` + `background_color` + icon in manifest |
| Viewport Meta | `<meta name="viewport" content="width=device-width, initial-scale=1">` |
| Start URL | `start_url` in manifest resolves when offline |
| Status Bar | `theme_color` in manifest and `<meta name="theme-color">` |

## Install Prompt

Handle the `beforeinstallprompt` event to show a custom install UI:

```typescript
// hooks/useInstallPrompt.ts
import { useState, useEffect, useCallback } from 'react';

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

export function useInstallPrompt() {
  const [installPrompt, setInstallPrompt] =
    useState<BeforeInstallPromptEvent | null>(null);
  const [isInstalled, setIsInstalled] = useState(false);

  useEffect(() => {
    const handler = (e: Event) => {
      e.preventDefault();
      setInstallPrompt(e as BeforeInstallPromptEvent);
    };

    const installedHandler = () => setIsInstalled(true);

    window.addEventListener('beforeinstallprompt', handler);
    window.addEventListener('appinstalled', installedHandler);

    // Check if already installed
    if (window.matchMedia('(display-mode: standalone)').matches) {
      setIsInstalled(true);
    }

    return () => {
      window.removeEventListener('beforeinstallprompt', handler);
      window.removeEventListener('appinstalled', installedHandler);
    };
  }, []);

  const promptInstall = useCallback(async () => {
    if (!installPrompt) return false;
    await installPrompt.prompt();
    const { outcome } = await installPrompt.userChoice;
    setInstallPrompt(null);
    return outcome === 'accepted';
  }, [installPrompt]);

  return { canInstall: !!installPrompt, isInstalled, promptInstall };
}
```

## Push Notifications

```typescript
// utils/pushNotifications.ts
export async function subscribeToPush(
  vapidPublicKey: string
): Promise<PushSubscription | null> {
  if (!('PushManager' in window)) return null;

  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: urlBase64ToUint8Array(vapidPublicKey),
  });

  return subscription;
}

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding)
    .replace(/-/g, '+')
    .replace(/_/g, '/');
  const rawData = atob(base64);
  return Uint8Array.from(rawData, (char) => char.charCodeAt(0));
}
```

## Framework Integration

### React + Vite (vite-plugin-pwa)

```bash
npm install -D vite-plugin-pwa
```

```typescript
// vite.config.ts
import { VitePWA } from 'vite-plugin-pwa';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
      manifest: {
        name: 'My App',
        short_name: 'App',
        description: 'My Progressive Web App',
        theme_color: '#ffffff',
        background_color: '#ffffff',
        display: 'standalone',
        scope: '/',
        start_url: '/',
        icons: [
          { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'any maskable',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/api\./i,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'api-cache',
              expiration: { maxEntries: 100, maxAgeSeconds: 60 * 60 },
              networkTimeoutSeconds: 10,
            },
          },
        ],
      },
    }),
  ],
});
```

### Angular (@angular/service-worker)

```bash
ng add @angular/pwa
```

This generates `ngsw-config.json`, `manifest.webmanifest`, and registers the SW in `app.module.ts`. Customize caching groups in `ngsw-config.json`.

## Completion Checklist

- [ ] `manifest.json` present with name, short_name, icons, display, start_url, theme_color
- [ ] Service worker registered and controlling the page
- [ ] Static assets precached (HTML, CSS, JS, fonts, icons)
- [ ] API responses cached with appropriate strategy
- [ ] Offline fallback page served when network is unavailable
- [ ] Install prompt handled with custom UI
- [ ] Icons provided: 192×192 and 512×512 (plus maskable variant)
- [ ] `<meta name="theme-color">` set in HTML `<head>`
- [ ] `<meta name="viewport">` set for responsive layout
- [ ] HTTPS configured (required for service workers)
- [ ] Lighthouse PWA audit score = 100
- [ ] Tested on mobile (iOS Safari, Android Chrome) and desktop
