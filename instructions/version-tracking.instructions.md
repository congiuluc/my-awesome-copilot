---
description: "Use when updating CHANGELOG.md. Covers Keep a Changelog format, Semantic Versioning, section ordering, and Conventional Commits mapping."
applyTo: "CHANGELOG.md"
---
# Version Tracking Guidelines

## Format

Follow the **Keep a Changelog** format strictly:

```markdown
# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- New feature description

### Fixed
- Bug fix description

## [1.2.0] - 2026-03-15

### Added
- Feature X with description
```

## Section Order

Always use this order within a version block:

1. **Added** — new features
2. **Changed** — changes to existing functionality
3. **Deprecated** — features that will be removed
4. **Removed** — removed features
5. **Fixed** — bug fixes
6. **Security** — vulnerability fixes

- Only include sections that have entries — omit empty sections.

## Semantic Versioning

| Change Type | Version Bump | Example |
|-------------|-------------|---------|
| Breaking change | **MAJOR** (X.0.0) | Removed API endpoint |
| New feature | **MINOR** (0.X.0) | Added new endpoint |
| Bug fix | **PATCH** (0.0.X) | Fixed validation error |
| Security fix | **PATCH** (0.0.X) | Patched XSS vulnerability |

## Conventional Commits Mapping

| Commit Prefix | Changelog Section | Bump |
|---------------|-------------------|------|
| `feat:` | Added | MINOR |
| `fix:` | Fixed | PATCH |
| `feat!:` | Changed (breaking) | MAJOR |
| `perf:` | Changed | PATCH |
| `security:` | Security | PATCH |

## Rules

- Every feature, fix, or breaking change **must** have a changelog entry.
- Write entries from the **user's perspective** — not implementation details.
- `[Unreleased]` section collects upcoming changes until the next release.
- Date format: `YYYY-MM-DD` (ISO 8601).
