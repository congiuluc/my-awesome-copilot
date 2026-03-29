# Web App Manifest

## Overview

The web app manifest (`manifest.json`) tells the browser about your PWA and how it should behave
when installed on the user's device. It controls the app's name, icons, theme, display mode,
and launch behavior.

## Complete Manifest Template

```json
{
  "name": "My Application",
  "short_name": "MyApp",
  "description": "A brief description of the application",
  "start_url": "/",
  "scope": "/",
  "display": "standalone",
  "orientation": "any",
  "theme_color": "#ffffff",
  "background_color": "#ffffff",
  "dir": "ltr",
  "lang": "en",
  "categories": ["productivity"],
  "icons": [
    {
      "src": "/icons/icon-48x48.png",
      "sizes": "48x48",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-96x96.png",
      "sizes": "96x96",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-128x128.png",
      "sizes": "128x128",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-144x144.png",
      "sizes": "144x144",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-384x384.png",
      "sizes": "384x384",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-512x512-maskable.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "maskable"
    }
  ],
  "screenshots": [
    {
      "src": "/screenshots/desktop.png",
      "sizes": "1280x720",
      "type": "image/png",
      "form_factor": "wide",
      "label": "Desktop view"
    },
    {
      "src": "/screenshots/mobile.png",
      "sizes": "750x1334",
      "type": "image/png",
      "form_factor": "narrow",
      "label": "Mobile view"
    }
  ],
  "shortcuts": [
    {
      "name": "New Item",
      "short_name": "New",
      "description": "Create a new item",
      "url": "/new",
      "icons": [
        {
          "src": "/icons/shortcut-new.png",
          "sizes": "192x192",
          "type": "image/png"
        }
      ]
    }
  ],
  "related_applications": [],
  "prefer_related_applications": false
}
```

## Linking in HTML

```html
<!-- In <head> -->
<link rel="manifest" href="/manifest.json" />
<meta name="theme-color" content="#ffffff" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<meta name="apple-mobile-web-app-status-bar-style" content="default" />
<meta name="apple-mobile-web-app-title" content="MyApp" />
<link rel="apple-touch-icon" href="/icons/icon-192x192.png" />
```

## Field Reference

### Required Fields

| Field | Description | Example |
|-------|-------------|---------|
| `name` | Full name shown on install and splash screen | `"My Application"` |
| `short_name` | Shown on home screen (≤12 chars) | `"MyApp"` |
| `start_url` | URL that loads when the app launches | `"/"` |
| `display` | How the app is displayed | `"standalone"` |
| `icons` | Array of icon objects (need 192 and 512 minimum) | See template |
| `theme_color` | Browser toolbar and task switcher color | `"#1a73e8"` |
| `background_color` | Splash screen background | `"#ffffff"` |

### Display Modes

| Mode | Behavior |
|------|----------|
| `fullscreen` | No browser UI — fills entire screen |
| `standalone` | Looks like a native app — no URL bar (recommended) |
| `minimal-ui` | Minimal browser UI — back/forward/reload buttons |
| `browser` | Standard browser tab |

### Optional Fields

| Field | Description |
|-------|-------------|
| `description` | Description of the app (shown in install dialog) |
| `scope` | URL scope — navigation outside this falls back to browser |
| `orientation` | `any`, `portrait`, `landscape`, `portrait-primary`, `landscape-primary` |
| `dir` | Text direction: `ltr`, `rtl`, `auto` |
| `lang` | Language tag (e.g., `en`, `it`, `es`) |
| `categories` | Array of category strings for app stores |
| `screenshots` | Screenshots shown in install dialog (Chrome 90+) |
| `shortcuts` | App launcher shortcuts (right-click on icon) |

## Icons

### Minimum Required

- **192×192** — Standard icon for home screen
- **512×512** — Required for Lighthouse PWA audit and splash screen

### Recommended Set

Generate all sizes from a single 1024×1024 source image:

```
48×48, 72×72, 96×96, 128×128, 144×144, 192×192, 384×384, 512×512
```

### Maskable Icons

Maskable icons adapt to different device shapes (circle, squircle, rounded square).
The safe zone is a centered circle with radius 40% of the icon size.
Keep important content inside this safe area.

```json
{
  "src": "/icons/icon-512x512-maskable.png",
  "sizes": "512x512",
  "type": "image/png",
  "purpose": "maskable"
}
```

Use [Maskable.app](https://maskable.app/) to preview and validate maskable icons.

**Important**: Use separate entries for `any` and `maskable` purposes. Don't combine them
in one icon entry (`"purpose": "any maskable"`) — this causes the maskable icon to be used
everywhere, which looks bad on platforms that don't apply masking.

## Shortcuts

App shortcuts appear on long-press (mobile) or right-click (desktop):

```json
{
  "shortcuts": [
    {
      "name": "Create New Task",
      "short_name": "New Task",
      "description": "Quickly create a new task",
      "url": "/tasks/new?source=shortcut",
      "icons": [{ "src": "/icons/shortcut-add.png", "sizes": "192x192" }]
    },
    {
      "name": "Search",
      "url": "/search?source=shortcut",
      "icons": [{ "src": "/icons/shortcut-search.png", "sizes": "192x192" }]
    }
  ]
}
```

Limit to 3–4 shortcuts. More will be truncated by the OS.

## Screenshots for Richer Install UI

Chrome 90+ shows screenshots in the install dialog. Provide both wide (desktop) and
narrow (mobile) variants:

```json
{
  "screenshots": [
    {
      "src": "/screenshots/desktop-home.png",
      "sizes": "1280x720",
      "type": "image/png",
      "form_factor": "wide",
      "label": "Home page on desktop"
    },
    {
      "src": "/screenshots/mobile-home.png",
      "sizes": "750x1334",
      "type": "image/png",
      "form_factor": "narrow",
      "label": "Home page on mobile"
    }
  ]
}
```

## Validation

Use these tools to validate your manifest:

- **Lighthouse** — PWA audit checks manifest completeness
- **Chrome DevTools** — Application → Manifest panel shows parsed manifest
- **[PWA Builder](https://www.pwabuilder.com/)** — Scores and suggests improvements
- **[Maskable.app](https://maskable.app/)** — Validates maskable icon safe zone
