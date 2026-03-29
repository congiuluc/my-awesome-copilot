---
description: "Use when writing, modifying, or reviewing Python backend tests. Covers pytest, pytest-asyncio, pytest-mock, httpx AsyncClient, FastAPI TestClient, and factory_boy conventions."
applyTo: "tests/**/*.py"
---
# Backend Testing Guidelines (Python)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.** No exceptions.

## Frameworks

- **pytest** as the test framework.
- **pytest-asyncio** for async test support.
- **pytest-mock** for mocking dependencies.
- **httpx** (`AsyncClient`) for FastAPI integration tests.
- **factory_boy** for generating realistic test data.

## Test Structure

```
tests/
  unit/
    domain/                # Domain model unit tests
    application/           # Service unit tests
    infrastructure/        # Repository unit tests
  integration/             # API endpoint integration tests
  conftest.py              # Shared fixtures
```

## Naming Convention

- Test file: `test_{module_name}.py` (e.g., `test_product_service.py`).
- Test class: `Test{ClassUnderTest}` (e.g., `TestProductService`).
- Test function: `test_should_do_x_when_condition`.

## Key Patterns

- Use `@pytest.mark.asyncio` for async tests.
- Use `AsyncMock` for async dependency mocking.
- Use `@pytest.fixture` for test setup.
- Use `@pytest.mark.parametrize` for data-driven tests.
- Use Arrange-Act-Assert pattern.
- One assertion concept per test.
- Tests must be deterministic — no external dependencies.
