# Workflow Patterns

## Workflow Structure

```
.github/workflows/
  ci.yml              # Build + test on every PR and push to main
  deploy-staging.yml  # Deploy to staging on merge to main
  deploy-prod.yml     # Deploy to production on release tag
```

## CI Workflow

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true

jobs:
  backend:
    name: Backend (.NET)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --warnaserror

      - name: Test
        run: dotnet test --no-build --verbosity normal
             --collect:"XPlat Code Coverage"
             --results-directory ./coverage

      - name: Upload coverage
        if: github.event_name == 'pull_request'
        uses: actions/upload-artifact@v4
        with:
          name: backend-coverage
          path: coverage/

  frontend:
    name: Frontend (React)
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/{frontend-app}
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: src/{frontend-app}/package-lock.json

      - name: Install
        run: npm ci

      - name: Lint
        run: npm run lint

      - name: Type check
        run: npx tsc --noEmit

      - name: Build
        run: npm run build

      - name: Test
        run: npm test -- --run --coverage

      - name: Upload coverage
        if: github.event_name == 'pull_request'
        uses: actions/upload-artifact@v4
        with:
          name: frontend-coverage
          path: src/{frontend-app}/coverage/
```

## Deploy Staging Workflow

```yaml
name: Deploy Staging

on:
  push:
    branches: [main]

concurrency:
  group: deploy-staging
  cancel-in-progress: false

jobs:
  deploy:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    environment: staging
    needs: [ci]  # Reusable workflow or separate trigger
    steps:
      - uses: actions/checkout@v4

      - name: Build and push Docker images
        run: |
          docker build -t myapp-api:${{ github.sha }} -f src/{App}.Api/Dockerfile .
          docker build -t myapp-web:${{ github.sha }} -f src/{frontend-app}/Dockerfile .

      # Push to registry and deploy (Azure, AWS, etc.)
```

## Rules

- **Both backend and frontend must pass** before merging a PR.
- Pin action versions to major tags (`@v4`) — never `@latest` or `@main`.
- Cache dependencies for faster builds.
- Run tests with coverage — fail if coverage drops below threshold.
- Use `concurrency` to cancel in-progress runs on same branch.
- Never store secrets in workflow files — use `${{ secrets.NAME }}`.
- Use environment protection rules for staging/production.
- Build once → deploy to multiple environments via artifacts.
- Add `--warnaserror` to .NET build to catch warnings early.
- Add `npx tsc --noEmit` to frontend CI for type checking.
