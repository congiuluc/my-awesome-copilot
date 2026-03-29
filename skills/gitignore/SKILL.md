---
name: gitignore
description: "Generate and maintain .gitignore files for any tech stack: .NET, Java, Python, React, Angular, VS Code extensions, Docker, Azure, and full-stack combinations. Use when: scaffolding a new project, adding a new technology to an existing project, reviewing tracked files for accidental commits, or regenerating .gitignore after stack changes."
argument-hint: 'Describe the tech stack or combination (e.g., .NET + React, Java + Angular, Python FastAPI, VS Code extension).'
---

# Gitignore

## When to Use

- Scaffolding a new project and need a .gitignore tailored to the stack
- Adding a new technology or framework to an existing project
- Reviewing whether sensitive or generated files are accidentally tracked
- Regenerating .gitignore after changing build tools, IDEs, or cloud providers
- Combining multiple stacks (e.g., .NET API + React frontend + Docker + Azure)

## Official Documentation

- [gitignore Patterns](https://git-scm.com/docs/gitignore)
- [github/gitignore Templates](https://github.com/github/gitignore)

## Procedure

1. Detect the project stack by scanning for marker files (see [Stack Detection](#stack-detection))
2. Select the matching template(s) from `./samples/`
3. Compose a combined `.gitignore` by merging the relevant sections
4. Follow the conventions in [gitignore patterns reference](./references/gitignore-patterns.md)
5. Always include the **Common** section (OS, IDE, environment)
6. Verify no sensitive files (`.env`, secrets, keys) are tracked
7. Add project-specific entries at the bottom under `## Project-specific`

## Stack Detection

| Marker Files | Stack | Sample |
|-------------|-------|--------|
| `.csproj`, `.sln`, `global.json` | .NET | `.gitignore-dotnet.sample` |
| `pom.xml`, `build.gradle`, `build.gradle.kts` | Java | `.gitignore-java.sample` |
| `pyproject.toml`, `requirements.txt`, `Pipfile` | Python | `.gitignore-python.sample` |
| `vite.config.ts` + `react` in `package.json` | React | `.gitignore-react.sample` |
| `angular.json`, `@angular/core` in `package.json` | Angular | `.gitignore-angular.sample` |
| `.vscode/`, `package.json` with `@vscode/vsce` | VS Code Extension | `.gitignore-vscode-extension.sample` |
| `Dockerfile`, `docker-compose.yml` | Docker | Included in all stack samples |
| `azure.yaml`, `.azure/` | Azure (azd) | `.gitignore-azure.sample` |

For full-stack projects, merge the backend + frontend + infrastructure samples.

## Composition Rules

When combining multiple templates:
1. Start with the **Common** section (OS artifacts, IDE files, env files)
2. Add the **backend** section (.NET / Java / Python)
3. Add the **frontend** section (React / Angular)
4. Add the **infrastructure** section (Docker, Azure, Terraform)
5. Add a `## Project-specific` section at the bottom for custom entries
6. Remove duplicate patterns across sections
7. Keep sections clearly labeled with `##` headers for maintainability
