---
description: "Use when creating, modifying, or reviewing GitHub Actions workflows, CI/CD pipelines, or build automation. Covers .NET build/test, frontend build/test, and deployment workflows."
applyTo: ".github/workflows/**"
---
# CI/CD GitHub Actions Guidelines

## Workflow Structure

```
.github/workflows/
  ci.yml              # Build + test on every PR and push to main
  deploy-staging.yml  # Deploy to staging on merge to main
  deploy-prod.yml     # Deploy to production on release tag
```

## CI Workflow Pattern

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/web-app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: src/web-app/package-lock.json
      - run: npm ci
      - run: npm run lint
      - run: npm run build
      - run: npm test -- --run
```

## Rules

- **Both backend and frontend must pass** before merging a PR.
- Pin action versions to major tags (`@v4`) — never use `@latest` or `@main`.
- Cache dependencies: `actions/setup-dotnet` + NuGet cache, `actions/setup-node` + npm cache.
- Run tests with coverage collection — fail if coverage drops below threshold.
- Use `concurrency` to cancel in-progress runs on same branch.
- Never store secrets in workflow files — use GitHub Secrets and `${{ secrets.NAME }}`.
- Use environment protection rules for staging/production deployments.
- Separate build artifacts from deploy steps — build once, deploy to multiple environments.
