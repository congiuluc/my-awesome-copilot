# Git Hooks

Automate code quality checks before commits and pushes.

## Overview

| Hook | Triggers | Use For |
|------|----------|---------|
| `pre-commit` | Before commit is created | Lint, format, type-check |
| `commit-msg` | After commit message is written | Validate Conventional Commits format |
| `pre-push` | Before push to remote | Run tests, build check |

---

## Setup with Husky (Node.js Projects)

### Install

```bash
npm install --save-dev husky
npx husky init
```

This creates a `.husky/` directory with a sample `pre-commit` hook.

### Pre-Commit Hook

`.husky/pre-commit`:

```bash
#!/bin/sh

# Frontend: lint staged files
cd src/web-app && npx lint-staged

# Backend: format check
cd ../../
dotnet format --verify-no-changes --verbosity minimal
```

### Commit-Msg Hook (Validate Conventional Commits)

Install commitlint:

```bash
npm install --save-dev @commitlint/cli @commitlint/config-conventional
```

Create `commitlint.config.js`:

```js
export default { extends: ['@commitlint/config-conventional'] };
```

`.husky/commit-msg`:

```bash
#!/bin/sh
npx --no -- commitlint --edit "$1"
```

### Lint-Staged (Only Lint Changed Files)

Install:

```bash
npm install --save-dev lint-staged
```

Add to `package.json`:

```json
{
  "lint-staged": {
    "src/web-app/**/*.{ts,tsx}": ["eslint --fix", "prettier --write"],
    "src/web-app/**/*.css": ["prettier --write"]
  }
}
```

---

## Setup Without Node.js (Pure .NET Projects)

Use native Git hooks in `.githooks/`:

### Configure Git to use custom hooks directory

```bash
git config core.hooksPath .githooks
```

### Pre-Commit Hook

`.githooks/pre-commit`:

```bash
#!/bin/sh
echo "Running dotnet format check..."
dotnet format --verify-no-changes --verbosity minimal
if [ $? -ne 0 ]; then
  echo "❌ Format check failed. Run 'dotnet format' to fix."
  exit 1
fi
```

### Commit-Msg Hook (Simple Regex)

`.githooks/commit-msg`:

```bash
#!/bin/sh
commit_msg=$(cat "$1")
pattern="^(feat|fix|chore|docs|refactor|test|perf|ci|build|style)(\(.+\))?!?: .{1,}"

if ! echo "$commit_msg" | grep -qE "$pattern"; then
  echo "❌ Commit message must follow Conventional Commits format:"
  echo "  feat: add user profile"
  echo "  fix(auth): resolve token expiry"
  exit 1
fi
```

> Make hooks executable: `chmod +x .githooks/*`

---

## Rules

- Always include a `pre-commit` hook for lint/format — catch issues before they enter history.
- Validate commit messages to enforce Conventional Commits — enables automated changelogs.
- Keep hooks fast (< 5 seconds) — slow hooks frustrate developers and get bypassed.
- Never bypass hooks in CI — use `--no-verify` only for exceptional cases, never as default.
- Document hook setup in `CONTRIBUTING.md` so new developers know to run `npx husky init` or `git config core.hooksPath .githooks`.
