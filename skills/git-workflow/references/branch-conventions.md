# Branch Conventions

## Branch Naming

Use a prefix that describes the type of work, followed by a short slug:

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New feature | `feature/user-profile` |
| `bugfix/` | Non-urgent bug fix | `bugfix/login-redirect` |
| `hotfix/` | Urgent production fix | `hotfix/payment-crash` |
| `chore/` | Maintenance, deps, config | `chore/update-deps` |
| `docs/` | Documentation only | `docs/api-readme` |
| `refactor/` | Code restructuring | `refactor/auth-service` |
| `test/` | Adding or fixing tests | `test/checkout-e2e` |

### Rules

- Use lowercase with hyphens: `feature/add-user-search` (not `Feature/Add_User_Search`)
- Keep it short but descriptive (3–5 words max)
- Include ticket/issue number when available: `feature/123-user-profile`
- Never use spaces, underscores, or uppercase in branch names
- Protected branches (`main`, `develop`) cannot be pushed to directly

---

## Merge Policies

### Squash Merge (Recommended for Feature Branches)

Combines all commits into a single commit on `main`. Keeps history clean.

```bash
# GitHub PR setting: "Squash and merge"
# Or manually:
git checkout main
git merge --squash feature/user-profile
git commit -m "feat: add user profile page (#42)"
```

**When to use:** Feature branches, bugfix branches — most of the time.

### Merge Commit

Preserves full branch history with a merge commit. Useful for tracking branch lifecycle.

```bash
git checkout main
git merge --no-ff feature/user-profile
```

**When to use:** Release branches into main (GitFlow), when history matters.

### Rebase

Replays commits on top of the target branch. Linear history, no merge commits.

```bash
git checkout feature/user-profile
git rebase main
git checkout main
git merge feature/user-profile  # fast-forward
```

**When to use:** Personal/local branch cleanup before opening a PR.

### Policy Comparison

| Policy | History | PR Workflow | Use For |
|--------|---------|-------------|---------|
| Squash merge | Clean, 1 commit per feature | ✓ Default | Feature/bugfix branches |
| Merge commit | Verbose, preserves branches | ✓ | Release branches |
| Rebase | Linear, no merge markers | Before PR | Local cleanup |

---

## Branch Protection Rules (GitHub)

Configure on `main` (and `develop` if using GitFlow):

| Rule | Recommendation |
|------|---------------|
| Require PR before merge | ✓ Enabled |
| Required approving reviews | 1 minimum (2 for production) |
| Dismiss stale reviews on new push | ✓ Enabled |
| Require status checks to pass | ✓ CI must pass |
| Require branch is up to date | ✓ Enabled |
| Require signed commits | Optional (team preference) |
| Restrict force pushes | ✓ Enabled |
| Restrict deletions | ✓ Enabled |

---

## Conflict Resolution

### Prevention

- Keep branches short-lived (merge within 1–3 days)
- Pull from `main` frequently: `git pull origin main --rebase`
- Avoid multiple people editing the same file simultaneously
- Break large features into smaller PRs

### Resolution Steps

```bash
# 1. Update your branch with latest main
git checkout feature/my-feature
git fetch origin
git rebase origin/main

# 2. If conflicts occur, resolve them file by file
# Edit conflicted files, then:
git add <resolved-file>
git rebase --continue

# 3. If rebase gets messy, abort and use merge instead
git rebase --abort
git merge origin/main
```

### Rules

- Always resolve conflicts on **your branch**, never on `main`
- After resolving, run the full build + test suite to verify
- When in doubt, discuss with the other author before resolving their changes
