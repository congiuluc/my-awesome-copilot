# Agent Framework Python — Code Style

## Naming

- Use `snake_case` for functions, variables, and module names.
- Use `PascalCase` for class names.
- Agent names should be descriptive strings: `"WeatherAgent"`, `"OrderProcessor"`.
- Tool function names should be verb-based: `get_weather`, `search_products`.

## Docstrings

- Add Google-style docstrings to all public classes and functions.
- Document agent instructions, tool descriptions, and workflow edges.

## Code Organization

- Keep lines under 120 characters.
- Group imports: stdlib → third-party → azure → agent_framework → project modules.
- Use `from __future__ import annotations` for forward references.

## Async Patterns

- All agent runs (`run`, `run_streaming`) must use `async`/`await`.
- Use `asyncio.run()` as the entry point.
- Never use blocking I/O inside async functions.

## Tool Functions

- Use type annotations and `Annotated[type, "description"]` for parameters.
- Use descriptive docstrings — these become the tool description for the LLM.
- Keep tool functions pure or clearly side-effect-documented.
- Return `str` or JSON-serializable types from tools.

## Type Hints

- Use type hints for all function signatures.
- Use `Annotated` from `typing` for enriched parameter metadata.
- Use `Optional[T]` or `T | None` for nullable parameters.

## Error Handling

- Use middleware for centralized error handling on agent runs.
- Never silently swallow exceptions — always log.
- Use `try`/`except` with specific exception types.

## Project Structure

```
src/
  {app}_agents/
    agents/            # Agent definitions and configurations
    tools/             # Function tool modules
    middleware/         # Custom middleware
    workflows/         # Workflow definitions with executors and edges
    config.py          # Environment and configuration
    main.py            # Entry point
```

## Testing

- Use `pytest` with `pytest-asyncio` for async tests.
- Mock LLM responses using provider-specific test helpers.
- Test tool functions independently from agent integration.
