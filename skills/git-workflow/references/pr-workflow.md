# PR Workflow

## PR Template

Store in `.github/PULL_REQUEST_TEMPLATE.md` to auto-populate every new PR.

### Template Contents

```markdown
## Description

<!-- What does this PR do? Link the issue if applicable. -->

Closes #

## Type of Change

- [ ] Feature (new functionality)
- [ ] Bug fix (non-breaking fix)
- [ ] Breaking change (existing functionality altered)
- [ ] Refactor (no functional change)
- [ ] Documentation
- [ ] Chore (dependencies, config, CI)

## Changes Made

<!-- List the key changes in bullet points -->

-

## Testing

<!-- How was this tested? -->

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist

- [ ] Code follows project conventions
- [ ] Self-review completed
- [ ] No new warnings introduced
- [ ] Tests pass locally
- [ ] Documentation updated (if applicable)
- [ ] Changelog entry added (if user-facing change)
```

---

## PR Best Practices

### Size

| Size | Lines Changed | Review Time | Recommendation |
|------|:------------:|:-----------:|----------------|
| Small | < 200 | < 30 min | ✓ Ideal |
| Medium | 200–500 | 30–60 min | ✓ Acceptable |
| Large | 500–1000 | 1–2 hours | ⚠️ Consider splitting |
| Huge | > 1000 | > 2 hours | ❌ Must split |

### Title Convention

Follow Conventional Commits format for PR titles:

```
feat: add user profile page
fix: resolve login redirect loop
chore: update React to v19.1
docs: add API authentication guide
refactor: extract auth middleware
```

### Description

- Link the issue/story: `Closes #123`
- Explain **why**, not just **what**
- Include screenshots for UI changes
- Note any deployment steps or config changes

---

## Review Process

### Reviewer Responsibilities

1. **Correctness** — Does the code work as intended?
2. **Design** — Does it follow project architecture and patterns?
3. **Security** — Are there any vulnerabilities? (inputs, auth, secrets)
4. **Tests** — Are new code paths covered?
5. **Readability** — Is the code clear and maintainable?

### Review Etiquette

- Review within 24 hours of being assigned
- Use suggestion blocks for concrete fixes
- Distinguish blocking vs. non-blocking feedback
- Approve when all blocking issues are resolved — don't block on style nits

### Labels

| Label | Meaning |
|-------|---------|
| `needs-review` | Ready for review |
| `changes-requested` | Reviewer requested changes |
| `approved` | Ready to merge |
| `do-not-merge` | PR is open but not ready |
| `breaking-change` | Contains breaking changes |

---

## Draft PRs

Open a **draft PR** when:
- Work is in progress but you want early feedback
- You want CI to run before the code is complete
- You want to show progress without requesting review

Convert to "Ready for Review" when all checks pass.
