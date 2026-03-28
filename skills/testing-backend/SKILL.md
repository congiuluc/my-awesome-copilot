---
name: testing-backend
description: >-
  Write backend tests using xUnit, Moq, and FluentAssertions. Use when: creating
  unit tests for services, integration tests for API endpoints, mocking
  repositories, testing validation, or testing middleware.
argument-hint: 'Describe the backend class, service, or endpoint you want to test.'
---

# Backend Testing (.NET)

## When to Use

- Creating unit tests for services, validators, or domain logic
- Writing integration tests for API endpoints
- Mocking repository or external service dependencies
- Testing middleware or filters

## Official Documentation

- [xUnit.net](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Moq Quickstart](https://github.com/devlooped/moq/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/introduction)
- [Integration Tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.**

## Procedure

1. Identify test type: unit test (service logic) or integration test (API endpoint)
2. Follow naming conventions in [.NET testing reference](./references/backend-testing.md)
3. Review [service test sample](./samples/service-test-sample.cs)
4. Use Arrange-Act-Assert pattern for all tests
5. Mock external dependencies at boundaries with Moq
6. Use FluentAssertions for readable assertions
7. Cover: happy path, edge cases, error scenarios, validation
8. Verify tests are deterministic — no external service or time dependencies
9. Run `dotnet test` locally before committing

## Test Structure

```
tests/
  MyApp.Core.Tests/           # Unit tests for domain logic
  MyApp.Infrastructure.Tests/ # Repository and infra tests
  MyApp.Api.Tests/            # API integration tests
```

## Naming Convention

- Test class: `{ClassUnderTest}Tests`
- Test method: `{Method}_{Scenario}_{ExpectedResult}`
- Example: `GetByIdAsync_WhenItemExists_ReturnsItem`
