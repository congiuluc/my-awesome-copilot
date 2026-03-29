---
description: "Use when writing, modifying, or reviewing Java Spring Boot backend code. Covers Spring Boot 3.x patterns, Hexagonal Architecture, DI, filters, SLF4J logging, and API response conventions."
applyTo: "src/main/java/**"
---
# Backend Java Spring Boot Guidelines

## Framework & Language

- Target **Java 21** (latest LTS). Use modern features: records, sealed classes, pattern matching.
- Use **Spring Boot 3.x** with Spring Web MVC (REST controllers).
- Use `record` types for DTOs and request/response models.
- Use `sealed interface` for domain type hierarchies.

## Hexagonal Architecture Layers

| Layer | Package | Depends On |
|-------|---------|------------|
| Domain | `domain/` | Nothing |
| Application | `application/` | Domain |
| Infrastructure | `infrastructure/` | Domain, Application |
| API | `api/` | Domain, Application, Infrastructure |

- Domain contains: entities, value objects, domain events, domain exceptions. NO Spring annotations.
- Application contains: service interfaces (ports), service implementations, use cases.
- Infrastructure contains: repository adapters, external service clients, config classes.
- API contains: REST controllers, DTOs, exception handlers, filters.

## API Response Envelope

All endpoints return a standard envelope:

```java
public record ApiResponse<T>(boolean success, T data, String error) {
    public static <T> ApiResponse<T> ok(T data) { return new ApiResponse<>(true, data, null); }
    public static <T> ApiResponse<T> error(String msg) { return new ApiResponse<>(false, null, msg); }
}
```

## Key Conventions

- **Javadoc** on all public classes and methods.
- **Constructor injection** — no `@Autowired` on fields.
- **camelCase** for fields, parameters, local variables.
- **UPPER_SNAKE_CASE** for constants.
- **120 character** max line length.
- **`@Transactional`** on service write methods, `@Transactional(readOnly = true)` on reads.
- **Jakarta Validation** annotations on request DTOs.
- **SLF4J** for structured logging.
- **SpringDoc OpenAPI** for auto-generated API documentation.
- **Spring Boot Actuator** for health checks and metrics.
