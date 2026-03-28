---
description: "Use when implementing frontend security: XSS prevention, file upload validation, URL encoding, Content Security Policy, and HTML sanitization."
applyTo: "src/web-app/src/components/**,src/web-app/src/services/**,src/web-app/src/hooks/**"
---
# Frontend Security Guidelines

## XSS Prevention

- **Rely on React's auto-escaping** — `{userInput}` in JSX is safe by default.
- **Never** use `dangerouslySetInnerHTML` without DOMPurify sanitization:
  ```tsx
  import DOMPurify from 'dompurify';
  <div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(htmlContent) }} />
  ```
- Never inject user input into `href`, `src`, or event handler attributes without validation.

## URL Construction

- Use `encodeURIComponent()` for user-provided URL segments.
- Use the `URL` API for building dynamic URLs — never string concatenation.
- Validate URL schemes — only allow `https:` and `http:` (block `javascript:`).

```tsx
// ✅ Safe
const url = new URL(`/api/users/${encodeURIComponent(userId)}`, baseUrl);

// ❌ Dangerous
const url = `/api/users/${userId}`;
```

## File Upload Validation

- Validate file type, size, and name on the **client side** (first defense).
- Maximum file size: enforce a reasonable limit (e.g., 10 MB).
- Accept only expected MIME types — reject executables and scripts.
- **Always validate server-side too** — client validation is bypassable.

## Content Security Policy

- Understand CSP headers impact on inline styles and scripts.
- Avoid inline `<script>` and `style` attributes — use external files.
- Use nonce-based CSP for any unavoidable inline scripts.

## Dependency Security

- Run `npm audit --audit-level=high` in CI pipelines.
- Keep npm dependencies updated — patch security advisories promptly.
- Review new dependencies before adding — check maintenance, download count, and known issues.

## Sensitive Data

- Never store tokens or secrets in `localStorage` — use `httpOnly` cookies.
- Clear sensitive data from component state on unmount.
- Never log sensitive user data to the browser console in production.
