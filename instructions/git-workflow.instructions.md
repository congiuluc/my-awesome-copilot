---
description: "Use when working with Git branching, merges, PRs, .gitignore, or commit conventions. Covers branching strategies, naming conventions, merge policies, PR templates, git hooks, and conflict resolution."
applyTo: ".gitignore,.github/PULL_REQUEST_TEMPLATE.md,.husky/**,.githooks/**,CONTRIBUTING.md"
---
# Git Workflow Guidelines

## Branching Strategy (GitHub Flow)

- `main` is always deployable — never push directly.
- Create a branch for every change: `feature/`, `bugfix/`, `hotfix/`, `chore/`, `docs/`.
- Open a PR, get approval, squash-merge into `main`.

## Branch Naming

```
feature/short-description
bugfix/issue-number-description
hotfix/critical-fix
chore/update-deps
```

- Lowercase with hyphens only. No spaces, underscores, or uppercase.
- Include issue number when available: `feature/123-user-profile`.

## Commit Messages (Conventional Commits)

```
feat: add user profile page
fix(auth): resolve token expiry issue
chore: update dependencies
docs: add API authentication guide
feat!: change auth to OAuth 2.0 (BREAKING)
```

| Prefix | Changelog Section | Version Bump |
|--------|-------------------|--------------|
| `feat:` | Added | MINOR |
| `fix:` | Fixed | PATCH |
| `feat!:` | Changed (breaking) | MAJOR |
| `chore:` | — (skip) | — |
| `docs:` | — (skip) | — |

## Merge Policy

- **Squash merge** for feature/bugfix branches (clean history).
- **Merge commit** for release branches (preserve branch context).
- **Rebase** for local cleanup before opening a PR — never rebase shared branches.

## PR Rules

- Keep PRs under 500 lines changed — split larger work.
- Use the PR template (`.github/PULL_REQUEST_TEMPLATE.md`).
- Link the issue: `Closes #123`.
- Title follows Conventional Commits format.
- All CI checks must pass before merge.
- At least 1 approving review required.

## Branch Protection (`main`)

- Require PR before merge.
- Require at least 1 approving review.
- Require status checks to pass (CI).
- Dismiss stale reviews on new pushes.
- Restrict force pushes and deletions.

## .gitignore

- Always include: `bin/`, `obj/`, `node_modules/`, `dist/`, `.env`, `.vs/`.
- Never commit secrets, local settings, or build outputs.
- Use specific image tags in Docker — never `.dockerignore` everything.

## Conflict Resolution

- Resolve conflicts on **your branch**, never on `main`.
- Pull from main frequently: `git pull origin main --rebase`.
- After resolving, run full build + test suite before pushing.
- When in doubt, discuss with the other author.
