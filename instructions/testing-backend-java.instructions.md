---
description: "Use when writing, modifying, or reviewing Java backend tests. Covers JUnit 5, Mockito, AssertJ, Spring Boot Test, MockMvc, and Testcontainers conventions."
applyTo: "src/test/java/**"
---
# Backend Testing Guidelines (Java)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.** No exceptions.

## Frameworks

- **JUnit 5** as the test framework.
- **Mockito** for mocking dependencies.
- **AssertJ** for fluent, readable assertions.
- **Spring Boot Test** (`@WebMvcTest`, `@SpringBootTest`) for integration tests.
- **Testcontainers** for database integration tests.

## Test Structure

```
src/test/java/
  com/myapp/
    domain/                  # Domain model tests
    application/service/     # Service unit tests
    infrastructure/          # Repository adapter tests
    api/controller/          # Controller integration tests
```

## Naming Convention

- Test class: `{ClassUnderTest}Test` (e.g., `ProductServiceTest`).
- Test method: `shouldDoX_whenCondition` (e.g., `shouldReturnProduct_whenIdExists`).

## Key Patterns

- Use `@ExtendWith(MockitoExtension.class)` for unit tests.
- Use `@WebMvcTest` for controller-layer tests.
- Use `@DataJpaTest` for JPA repository tests.
- Use Arrange-Act-Assert pattern.
- One assertion concept per test.
- Tests must be deterministic — no external dependencies.
