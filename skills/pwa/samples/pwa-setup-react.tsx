/**
 * Sample PWA setup for React + Vite using vite-plugin-pwa.
 *
 * This file demonstrates:
 * - vite.config.ts with VitePWA plugin
 * - Service worker registration hook
 * - Install prompt hook
 * - Online status hook
 * - Offline indicator component
 * - Update notification component
 * - App integration bringing it all together
 */

// ============================================================
// vite.config.ts
// ============================================================
/*
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
      manifest: {
        name: 'My Awesome App',
        short_name: 'AwesomeApp',
        description: 'An awesome progressive web app',
        theme_color: '#3b82f6',
        background_color: '#ffffff',
        display: 'standalone',
        scope: '/',
        start_url: '/',
        icons: [
          {
            src: 'pwa-192x192.png',
            sizes: '192x192',
            type: 'image/png',
          },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png',
          },
          {
            src: 'pwa-512x512-maskable.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'maskable',
          },
        ],
      },
      workbox: {
        globPatterns: ['**\/*.{js,css,html,ico,png,svg,woff2}'],
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/api\..*/i,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'api-cache',
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 60 * 60,
              },
              networkTimeoutSeconds: 10,
            },
          },
          {
            urlPattern: /\.(?:png|jpg|jpeg|svg|gif|webp)$/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'image-cache',
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 30 * 24 * 60 * 60,
              },
            },
          },
          {
            urlPattern: /\.(?:woff|woff2|ttf|otf)$/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'font-cache',
              expiration: {
                maxEntries: 20,
                maxAgeSeconds: 365 * 24 * 60 * 60,
              },
            },
          },
        ],
      },
    }),
  ],
});
*/

// ============================================================
// hooks/useOnlineStatus.ts
// ============================================================

import { useState, useEffect, useCallback } from 'react';

export function useOnlineStatus(): boolean {
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

// ============================================================
// hooks/useInstallPrompt.ts
// ============================================================

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

interface InstallPromptState {
  canInstall: boolean;
  isInstalled: boolean;
  promptInstall: () => Promise<boolean>;
}

export function useInstallPrompt(): InstallPromptState {
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

    if (window.matchMedia('(display-mode: standalone)').matches) {
      setIsInstalled(true);
    }

    return () => {
      window.removeEventListener('beforeinstallprompt', handler);
      window.removeEventListener('appinstalled', installedHandler);
    };
  }, []);

  const promptInstall = useCallback(async (): Promise<boolean> => {
    if (!installPrompt) return false;
    await installPrompt.prompt();
    const { outcome } = await installPrompt.userChoice;
    setInstallPrompt(null);
    return outcome === 'accepted';
  }, [installPrompt]);

  return { canInstall: !!installPrompt, isInstalled, promptInstall };
}

// ============================================================
// hooks/useServiceWorkerUpdate.ts
// ============================================================

interface ServiceWorkerUpdateState {
  isUpdateAvailable: boolean;
  applyUpdate: () => void;
}

export function useServiceWorkerUpdate(): ServiceWorkerUpdateState {
  const [isUpdateAvailable, setIsUpdateAvailable] = useState(false);

  useEffect(() => {
    const handler = () => setIsUpdateAvailable(true);
    window.addEventListener('sw-update-available', handler);
    return () => window.removeEventListener('sw-update-available', handler);
  }, []);

  const applyUpdate = useCallback(() => {
    window.location.reload();
  }, []);

  return { isUpdateAvailable, applyUpdate };
}

// ============================================================
// components/OfflineIndicator.tsx
// ============================================================

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
      <span className="inline-flex items-center gap-2">
        <svg
          className="h-4 w-4"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M18.364 5.636a9 9 0 11-12.728 0M12 9v4m0 4h.01"
          />
        </svg>
        You are offline. Some features may be unavailable.
      </span>
    </div>
  );
}

// ============================================================
// components/UpdateNotification.tsx
// ============================================================

export function UpdateNotification() {
  const { isUpdateAvailable, applyUpdate } = useServiceWorkerUpdate();

  if (!isUpdateAvailable) return null;

  return (
    <div
      role="alert"
      className="fixed bottom-4 right-4 bg-blue-600 text-white rounded-lg
        p-4 shadow-lg z-50 flex items-center gap-3 max-w-sm"
    >
      <div className="flex-1">
        <p className="font-medium">Update available</p>
        <p className="text-sm text-blue-100">
          A new version is ready. Refresh to update.
        </p>
      </div>
      <button
        onClick={applyUpdate}
        className="bg-white text-blue-600 px-4 py-2 rounded-md font-medium
          hover:bg-blue-50 transition-colors min-h-11 min-w-11 shrink-0"
      >
        Refresh
      </button>
    </div>
  );
}

// ============================================================
// components/InstallBanner.tsx
// ============================================================

export function InstallBanner() {
  const { canInstall, isInstalled, promptInstall } = useInstallPrompt();

  if (!canInstall || isInstalled) return null;

  return (
    <div
      className="fixed bottom-4 left-4 bg-white border border-gray-200
        rounded-lg p-4 shadow-lg z-50 flex items-center gap-3 max-w-sm"
    >
      <div className="flex-1">
        <p className="font-medium text-gray-900">Install App</p>
        <p className="text-sm text-gray-500">
          Add to your home screen for quick access.
        </p>
      </div>
      <button
        onClick={promptInstall}
        className="bg-blue-600 text-white px-4 py-2 rounded-md font-medium
          hover:bg-blue-700 transition-colors min-h-11 min-w-11 shrink-0"
      >
        Install
      </button>
    </div>
  );
}

// ============================================================
// App.tsx — Integration example
// ============================================================
/*
import { OfflineIndicator } from './components/OfflineIndicator';
import { UpdateNotification } from './components/UpdateNotification';
import { InstallBanner } from './components/InstallBanner';

export function App() {
  return (
    <>
      <OfflineIndicator />
      <UpdateNotification />

      <main className="min-h-screen">
        {/* App content here *\/}
      </main>

      <InstallBanner />
    </>
  );
}
*/

// ============================================================
// index.html — Required meta tags
// ============================================================
/*
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta name="theme-color" content="#3b82f6" />
    <meta name="description" content="My awesome progressive web app" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="default" />
    <meta name="apple-mobile-web-app-title" content="AwesomeApp" />
    <link rel="apple-touch-icon" href="/apple-touch-icon.png" />
    <link rel="manifest" href="/manifest.json" />
    <title>My Awesome App</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>
  </body>
</html>
*/
