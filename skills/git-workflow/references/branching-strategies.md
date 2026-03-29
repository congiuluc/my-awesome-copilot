# Branching Strategies

## GitHub Flow (Recommended for Most Projects)

Simple, lightweight strategy built around a single long-lived branch.

```
main ─────●──────●──────●──────●──────
           \    /        \    /
  feature/x ●──●   fix/y  ●──●
```

**Rules:**
- `main` is always deployable
- Create a feature branch from `main` for every change
- Open a PR when work is ready for review
- Merge to `main` via squash merge after approval
- Deploy from `main`

**Best for:** Teams with CI/CD, continuous deployment, smaller projects.

---

## GitFlow

Structured strategy with dedicated branches for releases and hotfixes.

```
main     ─────●──────────────────●──────
              |                  |
release  ─────┼────●────────────●
              |   /            /
develop  ────●──●──●──●──●──●──────────
              \   /    \  /
  feature/x    ●─●      ●
```

**Branches:**
| Branch | Purpose | Merges Into |
|--------|---------|-------------|
| `main` | Production-ready code | — |
| `develop` | Integration branch for features | `release/*`, `main` |
| `feature/*` | New features | `develop` |
| `release/*` | Release preparation | `main` + `develop` |
| `hotfix/*` | Emergency production fixes | `main` + `develop` |

**Best for:** Projects with scheduled releases, multiple environments, larger teams.

---

## Trunk-Based Development

All developers commit to a single shared branch with short-lived feature branches.

```
main ─●─●─●─●─●─●─●─●─●─●─●─
       \/ \/ 
  feat  ●  ●    (< 1 day lifetime)
```

**Rules:**
- All work lands on `main` (trunk) within 1–2 days
- Feature branches are very short-lived (hours, not days)
- Use feature flags for incomplete features
- Requires strong CI and automated tests

**Best for:** Experienced teams, continuous delivery, microservices.

---

## Strategy Comparison

| Factor | GitHub Flow | GitFlow | Trunk-Based |
|--------|:-----------:|:-------:|:-----------:|
| Simplicity | ✓✓ | | |
| Scheduled releases | | ✓✓ | |
| Continuous deployment | ✓✓ | | ✓✓ |
| Hotfix support | ✓ | ✓✓ | ✓ |
| Parallel releases | | ✓✓ | |
| Small team | ✓✓ | ✓ | ✓✓ |
| Large team | ✓ | ✓✓ | ✓ |
