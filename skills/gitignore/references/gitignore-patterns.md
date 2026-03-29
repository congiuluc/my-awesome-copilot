# Gitignore Patterns Reference

## Pattern Syntax

| Pattern | Meaning |
|---------|---------|
| `*.ext` | Ignore all files with that extension |
| `dir/` | Ignore entire directory (trailing slash required) |
| `!pattern` | Negate / un-ignore a previously ignored pattern |
| `**/dir` | Match `dir` at any depth |
| `dir/**` | Match everything inside `dir` |
| `#` | Comment line |

## Section Organization

Always organize `.gitignore` with labeled sections using `##` comments:

```
## Common — OS & IDE artifacts
## Backend — .NET / Java / Python
## Frontend — React / Angular / Node
## Infrastructure — Docker, Azure, Terraform
## Project-specific
```

## Mandatory Entries (All Projects)

These must always be present regardless of stack:

```gitignore
## Common — OS artifacts
.DS_Store
Thumbs.db
ehthumbs.db
Desktop.ini

## Common — IDE / Editor
.idea/
.vscode/*
!.vscode/settings.json
!.vscode/launch.json
!.vscode/tasks.json
!.vscode/extensions.json
*.swp
*.swo
*~

## Common — Environment & Secrets
.env
.env.*
!.env.example
*.pem
*.key
*.pfx
*.p12
```

## .NET Patterns

```gitignore
## .NET build output
bin/
obj/
out/
publish/

## .NET user files
*.user
*.suo
*.userosscache
*.sln.docstates

## .NET tools
.dotnet/
.nuget/
*.nupkg

## Code coverage
TestResults/
coverage/
*.coverage
*.coveragexml
```

## Java Patterns

```gitignore
## Java build output
target/
build/
out/
*.class
*.jar
*.war
*.ear

## Java / Maven / Gradle
.gradle/
gradle/wrapper/gradle-wrapper.jar
.mvn/wrapper/maven-wrapper.jar
!gradle/wrapper/gradle-wrapper.properties
!.mvn/wrapper/maven-wrapper.properties

## IDE
*.iml
.idea/
*.ipr
*.iws
```

## Python Patterns

```gitignore
## Python bytecode
__pycache__/
*.py[cod]
*$py.class
*.pyo

## Python envs
.venv/
venv/
env/
.env/
.Python

## Python packaging
dist/
build/
*.egg-info/
*.egg
.eggs/
wheels/
pip-wheel-metadata/

## Python tools
.mypy_cache/
.ruff_cache/
.pytest_cache/
.coverage
htmlcov/
.tox/
.nox/
```

## React / Node Patterns

```gitignore
## Node
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*
.pnpm-store/

## React / Vite build output
dist/
.next/
.nuxt/
.output/

## Storybook
storybook-static/

## Type checking
*.tsbuildinfo
```

## Angular Patterns

```gitignore
## Angular build output
dist/
.angular/

## Angular cache
.angular/cache/

## Node (same as React/Node section)
node_modules/
```

## Docker Patterns

```gitignore
## Docker
.docker/
docker-compose.override.yml
```

## Azure / azd Patterns

```gitignore
## Azure Developer CLI
.azure/
azd-env/

## Terraform
.terraform/
*.tfstate
*.tfstate.backup
*.tfplan
.terraform.lock.hcl

## Bicep
*.json.bak
```

## VS Code Extension Patterns

```gitignore
## VS Code Extension build
out/
dist/
*.vsix

## Node
node_modules/
```

## Security Rules

**Never track these files — verify with `git ls-files`:**

- `.env`, `.env.*` (except `.env.example`)
- Private keys (`*.pem`, `*.key`, `*.pfx`)
- Connection strings or secrets in config files
- `appsettings.*.json` with real credentials (use user-secrets in dev)
- Token/credential caches (`.azure/`, `.aws/credentials`)

## Validation

After generating `.gitignore`, always:

1. Run `git status` to verify no sensitive files are tracked
2. If sensitive files are already tracked, run `git rm --cached <file>` to untrack
3. Check `git ls-files` for any files that should be ignored
