# Agent Framework .NET — Code Style

## Naming

- Use `PascalCase` for public types, methods, and properties.
- Use `camelCase` for method parameters and private fields.
- Prefix private fields with underscore: `_myField`.
- Agent names should be descriptive: `"WeatherAgent"`, `"OrderProcessor"`.
- Tool method names should be verb-based: `GetWeather`, `SearchProducts`.

## XML Documentation

- Add XML doc comments (`///`) to all public members.
- Document agent instructions, tool descriptions, and workflow edges.

## Code Organization

- Use `#region` / `#endregion` to organize code into collapsible sections.
- Keep lines under 120 characters.
- Group `using` statements: System → Azure → Microsoft.Agents → project namespaces.

## Async Patterns

- All agent runs (`RunAsync`, `RunStreamingAsync`) must use `async`/`await`.
- Always propagate `CancellationToken`.
- Use `ConfigureAwait(false)` in library code, not in application code.

## Tool Functions

- Decorate tools with `[Description("...")]` for the method and each parameter.
- Keep tool functions pure or clearly side-effect-documented.
- Return `string` or serializable types from tools.

## Dependency Injection

- Register agents and services in DI using extension methods.
- Use `IServiceCollection` extensions to encapsulate agent configuration.
- Prefer scoped lifetime for session-bound agents.

## Error Handling

- Use function calling middleware to add retry or error handling to tool calls.
- Use agent run middleware for centralized error handling.
- Never swallow exceptions silently in middleware — always log.

## Project Structure

```
src/
  {App}.Agents/
    Agents/            # Agent definitions and configurations
    Tools/             # Function tool classes
    Middleware/         # Custom middleware
    Workflows/         # Workflow definitions with executors and edges
    Extensions/        # DI registration extension methods
    Program.cs
```
