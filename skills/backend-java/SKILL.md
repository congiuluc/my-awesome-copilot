---
name: backend-java
description: "Build Java Spring Boot 3.x backend code following Hexagonal Architecture. Use when: writing Java controllers, services, repository adapters, middleware/filters, DI configuration, structured logging, health checks, or API response envelope pattern."
argument-hint: 'Describe the controller, service, or component to implement.'
---

# Backend Java Spring Boot

## When to Use

- Creating or modifying Java backend code following Hexagonal Architecture
- Scaffolding new controllers, services, or filters
- Configuring dependency injection, logging, or health checks
- Reviewing Java backend code for best practices

## Official Documentation

- [Spring Boot Reference](https://docs.spring.io/spring-boot/reference/)
- [Spring Web MVC](https://docs.spring.io/spring-framework/reference/web/webmvc.html)
- [Spring Data JPA](https://docs.spring.io/spring-data/jpa/reference/)
- [Spring Security](https://docs.spring.io/spring-security/reference/)
- [SpringDoc OpenAPI](https://springdoc.org/)
- [Jakarta Validation](https://jakarta.ee/specifications/bean-validation/)
- [Micrometer Observability](https://micrometer.io/docs)

## Procedure

1. Identify the Hexagonal Architecture layer (Domain / Application / Infrastructure / API)
2. Follow the patterns in [Java guidelines](./references/java-guidelines.md)
3. Apply the [code style rules](./references/code-style.md)
4. Review [sample controller](./samples/controller-sample.java) for complete pattern
5. Wire up beans via `@Configuration` classes or component scanning
6. Ensure all I/O follows proper transactional and async patterns
7. Wrap all responses in `ApiResponse<T>` envelope
8. Add Javadoc comments to all public classes and methods
9. Create corresponding tests (see `testing-backend-java` skill)
