# Frontend Security

## XSS Prevention

- React auto-escapes JSX output — this is your primary defense.
- **Never use `dangerouslySetInnerHTML`** unless content is sanitized with DOMPurify.
- Validate and sanitize all user input before submission.
- Use `encodeURIComponent()` for URL parameters.

```tsx
// Safe — React escapes automatically
<p>{userInput}</p>

// XSS risk — only if absolutely necessary, sanitize first
<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(htmlContent) }} />
```

## File Upload Validation

- Validate file type, size, and content on **both** client and server.
- Never rely solely on `Content-Type` header — verify magic bytes on the server.
- Set maximum file size limits.
- Store uploaded files outside the webroot.

```tsx
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

const validateFile = (file: File): string | null => {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Only JPEG, PNG, and WebP images are allowed';
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'File size must be under 5MB';
  }
  return null;
};
```

## Safe URL Construction

- Always use `encodeURIComponent` for user-provided path/query values.
- Use `URL` constructor for building URLs.

```tsx
// Safe
const url = new URL(
  `/api/products/${encodeURIComponent(id)}`,
  window.location.origin
);
url.searchParams.set('q', searchTerm);
```

## Content Security Policy (CSP)

CSP headers are set by the backend, but frontend code must comply:

- Avoid inline `<script>` tags — use bundled modules instead.
- Avoid inline `style` attributes when possible — prefer Tailwind classes.
- If `style-src 'unsafe-inline'` is not allowed, extract styles to CSS files.
- Use `nonce`-based CSP if inline scripts are unavoidable.

## Dependency Scanning

Add to CI pipeline:

```yaml
- name: Frontend audit
  working-directory: src/web-app
  run: npm audit --audit-level=high
```

## Official References

- [OWASP XSS Prevention](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [Content Security Policy (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [DOMPurify](https://github.com/cure53/DOMPurify)
