# Workspace Conventions

These rules apply to all agents, skills, and instructions in this workspace.

## Language & Framework Defaults

- **Backend**: .NET 10 Minimal API (C#), Spring Boot 3.x (Java), FastAPI (Python)
- **Frontend**: React 19 + TypeScript + Vite + TailwindCSS v4, Angular 19 + TypeScript
- **Testing**: xUnit/Moq/FluentAssertions (C#), JUnit 5/Mockito/AssertJ (Java), pytest (Python), Vitest/RTL (React), Jasmine/Karma (Angular), Playwright (E2E)
- **Infrastructure**: Docker, GitHub Actions, Azure (Bicep + azd), .NET Aspire + OpenTelemetry

## Naming Conventions

### Files
- C# source: `PascalCase.cs`
- Java source: `PascalCase.java`
- Python source: `snake_case.py`
- TypeScript/React: `PascalCase.tsx` (components), `camelCase.ts` (utils/hooks/services)
- Angular: `kebab-case.component.ts`, `kebab-case.service.ts`
- Tests: `{SourceName}.test.tsx` (React), `{source-name}.component.spec.ts` (Angular), `{SourceName}Tests.cs` (C#), `{SourceName}Test.java` (Java), `test_{source_name}.py` (Python)

### Code
- C#: `PascalCase` for public members, `_camelCase` for private fields, `camelCase` for parameters
- Java: `camelCase` for methods/fields, `PascalCase` for classes, `UPPER_SNAKE_CASE` for constants
- Python: `snake_case` for functions/variables, `PascalCase` for classes, `UPPER_SNAKE_CASE` for constants
- TypeScript: `camelCase` for variables/functions, `PascalCase` for types/interfaces/components

## API Response Envelope

All backend APIs must use a consistent response wrapper:

```
{
  "success": true|false,
  "data": <T>,
  "error": "string | null"
}
```

Frontend code must parse this envelope and handle `success: false` cases explicitly.

## Git Commit Convention

Follow Conventional Commits:
- `feat:` — new feature (→ MINOR version bump)
- `fix:` — bug fix (→ PATCH version bump)
- `feat!:` or `fix!:` or `BREAKING CHANGE:` — breaking change (→ MAJOR version bump)
- `docs:` — documentation only
- `test:` — adding/updating tests
- `ci:` — CI/CD changes
- `refactor:` — code restructuring with no behavior change
- `perf:` — performance improvement
- `chore:` — maintenance tasks

## Code Quality

- Maximum line length: 120 characters
- XML documentation comments on all public members (C#)
- Javadoc on all public members (Java)
- Docstrings on all public functions/classes (Python)
- No `any` type in TypeScript — use proper types
- All I/O operations must be async with cancellation support
- No secrets in source code — use environment variables or Key Vault

## Architecture

- Clear separation of concerns: Core/Domain → Infrastructure → API/Presentation
- Dependency injection for all services
- Repository pattern for data access (IRepository<T>)
- Feature-based folder structure for frontend code

## Agent Workflow

When working on features:
1. Plans are saved to `.copilot/plans/{feature-slug}.plan.md`
2. Plans must be `approved` status before implementation begins
3. Build must produce zero errors AND zero warnings before proceeding
4. Every new public member needs at least one test
5. Reviews must clear all Critical and Important issues before merging
