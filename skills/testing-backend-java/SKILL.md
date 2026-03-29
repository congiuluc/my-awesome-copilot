---
name: testing-backend-java
description: >-
  Write Java backend tests using JUnit 5, Mockito, and AssertJ. Use when: creating
  unit tests for services, integration tests for REST controllers, mocking
  repositories, testing validation, or testing Spring Boot components.
argument-hint: 'Describe the Java class, service, or controller you want to test.'
---

# Backend Testing (Java)

## When to Use

- Creating unit tests for services, validators, or domain logic
- Writing integration tests for REST controllers
- Mocking repository or external service dependencies
- Testing Spring Boot configuration and filters

## Official Documentation

- [JUnit 5 User Guide](https://junit.org/junit5/docs/current/user-guide/)
- [Mockito](https://site.mockito.org/)
- [AssertJ](https://assertj.github.io/doc/)
- [Spring Boot Testing](https://docs.spring.io/spring-boot/reference/testing/)
- [Testcontainers](https://testcontainers.com/guides/getting-started-with-testcontainers-for-java/)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.**

## Procedure

1. Identify test type: unit test (service logic) or integration test (controller endpoint)
2. Follow naming conventions in [Java testing reference](./references/java-testing.md)
3. Review [service test sample](./samples/service-test-sample.java)
4. Use Given-When-Then or Arrange-Act-Assert pattern for all tests
5. Mock external dependencies at boundaries with Mockito
6. Use AssertJ for readable assertions
7. Cover: happy path, edge cases, error scenarios, validation
8. Verify tests are deterministic — no external service or time dependencies
9. Run `mvn test` or `gradle test` locally before committing

## Test Structure

```
src/test/java/
  com/myapp/
    domain/             # Domain model unit tests
    application/
      service/          # Service unit tests
    infrastructure/
      persistence/      # Repository adapter tests
    api/
      controller/       # Controller integration tests
```

## Naming Convention

- Test class: `{ClassUnderTest}Test` (e.g., `ProductServiceTest`)
- Test method: `shouldDoX_whenCondition` (e.g., `shouldReturnProduct_whenIdExists`)
