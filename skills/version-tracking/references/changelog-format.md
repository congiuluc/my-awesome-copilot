# Changelog Format Reference

> Official specification: [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/)

## File Location

The changelog should be at `docs/CHANGELOG.md` in the project root.

## Structure

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- New entries go here as work is completed

## [1.2.0] - 2026-03-15

### Added
- User profile page with avatar upload
- Product search with full-text filtering

### Fixed
- Image upload timeout on slow connections (#42)

## [1.1.0] - 2026-02-28

### Added
- Product listing with pagination
- Health check endpoint

### Changed
- Updated API response envelope to include request ID

## [1.0.0] - 2026-02-01

### Added
- Initial release with core API endpoints
- SQLite repository implementation
- React frontend with product catalog
```

## Rules

1. **`[Unreleased]` section always exists** at the top for ongoing work.
2. **Most recent version first** — newest entries at the top.
3. **Date format**: `YYYY-MM-DD` (ISO 8601).
4. **Group changes by type**: Added, Changed, Deprecated, Removed, Fixed, Security.
5. **One bullet per change** — keep descriptions concise but descriptive.
6. **Reference issues/PRs** when available: `(#42)`, `(PR #55)`.
7. **Never delete old versions** — the changelog is an append-only history.

## Version Bumping Rules (SemVer)

| Change Type | Bump | Example |
|------------|------|---------|
| Bug fix, patch | PATCH | 1.0.0 → 1.0.1 |
| New feature, backward-compatible | MINOR | 1.0.1 → 1.1.0 |
| Breaking API/behavior change | MAJOR | 1.1.0 → 2.0.0 |

When bumping MINOR, reset PATCH to 0. When bumping MAJOR, reset MINOR and PATCH.

## Release Workflow

1. Review all entries under `[Unreleased]`
2. Determine the correct version bump
3. Replace `[Unreleased]` header with `[x.y.z] - YYYY-MM-DD`
4. Add a fresh empty `[Unreleased]` section above
5. Commit: `chore: release vx.y.z`
6. Tag: `git tag vx.y.z`

## Official References

- [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/)
- [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html)
- [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/)
