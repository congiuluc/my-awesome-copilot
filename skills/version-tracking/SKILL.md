---
name: version-tracking
description: >-
  Track implementations, features, bugs, and releases in a versioning document.
  Use when: adding a commit, completing a feature, fixing a bug, or preparing
  a release. Automatically updates CHANGELOG.md following Keep a Changelog format
  and Semantic Versioning.
argument-hint: 'Describe what was implemented, fixed, or changed for the changelog entry.'
---

# Version Tracking

## When to Use

- After implementing a new feature
- After fixing a bug
- After making a breaking change
- When preparing a release
- When reviewing what changed between versions

## Official Documentation

- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Semantic Versioning 2.0.0](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)

## Procedure

1. Read the current [CHANGELOG.md](./references/changelog-format.md) format reference
2. Determine change type: Added, Changed, Deprecated, Removed, Fixed, Security
3. Determine version bump:
   - **PATCH** (0.0.x): bug fixes, minor corrections
   - **MINOR** (0.x.0): new features, backward-compatible additions
   - **MAJOR** (x.0.0): breaking changes
4. Add entry to `[Unreleased]` section in `docs/CHANGELOG.md`
5. When releasing: move `[Unreleased]` entries to `[x.y.z] - YYYY-MM-DD`
6. Review [sample changelog](./samples/changelog-sample.md) for format

## Change Categories

| Category | Use When |
|----------|----------|
| **Added** | New features, endpoints, components, pages |
| **Changed** | Modifications to existing functionality |
| **Deprecated** | Features marked for future removal |
| **Removed** | Features or code that was deleted |
| **Fixed** | Bug fixes |
| **Security** | Vulnerability patches |

## Commit Message Convention

Follow Conventional Commits to enable automated changelog generation:

```
feat: add user profile page
fix: resolve image upload timeout on slow connections
feat!: change authentication to OAuth 2.0 (BREAKING)
chore: update dependencies
docs: add API usage examples
perf: optimize product listing query
```

| Prefix | Maps To | Version Bump |
|--------|---------|-------------|
| `feat:` | Added | MINOR |
| `fix:` | Fixed | PATCH |
| `feat!:` or `BREAKING CHANGE:` | Changed | MAJOR |
| `perf:` | Changed | PATCH |
| `security:` | Security | PATCH |
| `docs:` | — (no changelog) | — |
| `chore:` | — (no changelog) | — |

## Completion Checklist

- [ ] CHANGELOG.md updated with change description
- [ ] Change category is correct (Added/Changed/Fixed/etc.)
- [ ] Version bump type determined (MAJOR/MINOR/PATCH)
- [ ] Commit message follows Conventional Commits format
- [ ] Breaking changes documented explicitly
