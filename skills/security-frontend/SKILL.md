---
name: security-frontend
description: >-
  Apply frontend security best practices for React apps. Use when: preventing XSS,
  validating file uploads, encoding URLs, configuring Content Security Policy,
  sanitizing HTML, or reviewing frontend code for security vulnerabilities.
argument-hint: 'Describe the frontend security concern or component to secure.'
---

# Frontend Security (React)

## When to Use

- Preventing XSS in user-generated content
- Validating and restricting file uploads
- Building safe URL construction
- Understanding Content Security Policy impact
- Sanitizing HTML when `dangerouslySetInnerHTML` is unavoidable
- Reviewing frontend code for security vulnerabilities

## Official Documentation

- [OWASP XSS Prevention](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [Content Security Policy (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [DOMPurify](https://github.com/cure53/DOMPurify)

## Procedure

1. Rely on React's auto-escaping — see [frontend security reference](./references/frontend-security.md)
2. Review [security frontend sample](./samples/security-frontend-sample.tsx)
3. **Never** use `dangerouslySetInnerHTML` without DOMPurify
4. Use `encodeURIComponent()` or `URL` API for all user-provided URL segments
5. Validate file uploads on the client (type, size) **and** server
6. Understand CSP headers (set by backend) and their impact on inline styles/scripts
7. Run `npm audit --audit-level=high` in CI
8. Review for XSS and open redirect vulnerabilities
