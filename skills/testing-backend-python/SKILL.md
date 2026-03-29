---
name: testing-backend-python
description: >-
  Write Python backend tests using pytest, pytest-mock, and httpx AsyncClient. Use
  when: creating unit tests for services, integration tests for FastAPI endpoints,
  mocking repositories, testing validation, or testing middleware.
argument-hint: 'Describe the Python class, service, or router you want to test.'
---

# Backend Testing (Python)

## When to Use

- Creating unit tests for services, validators, or domain logic
- Writing integration tests for FastAPI endpoints
- Mocking repository or external service dependencies
- Testing middleware or dependency overrides

## Official Documentation

- [pytest](https://docs.pytest.org/)
- [pytest-asyncio](https://pytest-asyncio.readthedocs.io/)
- [pytest-mock](https://pytest-mock.readthedocs.io/)
- [httpx AsyncClient](https://www.python-httpx.org/async/)
- [FastAPI Testing](https://fastapi.tiangolo.com/tutorial/testing/)
- [factory_boy](https://factoryboy.readthedocs.io/)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.**

## Procedure

1. Identify test type: unit test (service logic) or integration test (API endpoint)
2. Follow naming conventions in [Python testing reference](./references/python-testing.md)
3. Review [service test sample](./samples/test_product_service.py)
4. Use Arrange-Act-Assert pattern for all tests
5. Mock external dependencies at boundaries with `pytest-mock` or `unittest.mock`
6. Use plain `assert` with descriptive messages
7. Cover: happy path, edge cases, error scenarios, validation
8. Verify tests are deterministic — no external service or time dependencies
9. Run `pytest` locally before committing

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

- Test file: `test_{module_name}.py` (e.g., `test_product_service.py`)
- Test class: `Test{ClassUnderTest}` (e.g., `TestProductService`)
- Test function: `test_should_do_x_when_condition` (e.g., `test_should_return_product_when_id_exists`)
