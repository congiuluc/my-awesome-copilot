---
name: error-handling-backend-java
description: >-
  Implement consistent Java Spring Boot error handling with global exception handlers,
  custom exceptions, and structured error responses. Use when: building
  @RestControllerAdvice, creating domain exceptions, mapping errors to HTTP status
  codes, or structured error logging with SLF4J.
argument-hint: 'Describe the error scenario or exception type to handle.'
---

# Backend Error Handling (Java)

## When to Use

- Setting up global exception handling with `@RestControllerAdvice`
- Creating custom exception types for domain errors
- Implementing the standard `ApiResponse` error envelope
- Mapping exceptions to HTTP status codes
- Logging errors with SLF4J structured context

## Official Documentation

- [Spring Boot Error Handling](https://docs.spring.io/spring-boot/reference/web/servlet.html#web.servlet.spring-mvc.error-handling)
- [@RestControllerAdvice](https://docs.spring.io/spring-framework/reference/web/webmvc/mvc-ann-rest-exceptions.html)
- [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807)
- [Resilience4j](https://resilience4j.readme.io/)
- [SLF4J](https://www.slf4j.org/)

## Procedure

1. Create a global `@RestControllerAdvice` exception handler
2. Review [Java error handling patterns](./references/java-error-handling.md) for advanced scenarios
3. Review [exception handler sample](./samples/GlobalExceptionHandler.java)
3. Define custom domain exceptions: `NotFoundException`, `ValidationException`, `ConflictException`
4. Map exceptions to HTTP status codes in the handler
5. Return `ApiResponse` envelope with error details — never expose stack traces
6. Log errors with SLF4J MDC context (correlation ID, user ID)
7. Use Resilience4j for retry/circuit-breaker on external calls
8. Never swallow exceptions silently

## Exception Hierarchy

```
RuntimeException
├── NotFoundException          → 404
├── ValidationException        → 400
├── ConflictException          → 409
├── UnauthorizedException      → 401
├── ForbiddenException         → 403
└── BusinessRuleException      → 422
```

## Global Exception Handler Pattern

```java
@RestControllerAdvice
@Slf4j
public class GlobalExceptionHandler {

    @ExceptionHandler(NotFoundException.class)
    public ResponseEntity<ApiResponse<Void>> handleNotFound(NotFoundException ex) {
        log.warn("Resource not found: {}", ex.getMessage());
        return ResponseEntity.status(HttpStatus.NOT_FOUND)
            .body(ApiResponse.error(ex.getMessage()));
    }

    @ExceptionHandler(ValidationException.class)
    public ResponseEntity<ApiResponse<Void>> handleValidation(ValidationException ex) {
        log.warn("Validation failed: {}", ex.getMessage());
        return ResponseEntity.status(HttpStatus.BAD_REQUEST)
            .body(ApiResponse.error(ex.getMessage()));
    }

    @ExceptionHandler(Exception.class)
    public ResponseEntity<ApiResponse<Void>> handleUnexpected(Exception ex) {
        log.error("Unexpected error", ex);
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
            .body(ApiResponse.error("An unexpected error occurred."));
    }
}
```

## Error Response Envelope

```java
public record ApiResponse<T>(boolean success, T data, String error) {

    public static <T> ApiResponse<T> ok(T data) {
        return new ApiResponse<>(true, data, null);
    }

    public static <T> ApiResponse<T> error(String message) {
        return new ApiResponse<>(false, null, message);
    }
}
```

## Resilience Patterns

```java
@Retry(name = "externalService", fallbackMethod = "fallback")
@CircuitBreaker(name = "externalService")
public String callExternalService() {
    return restClient.get().uri("/api/data").retrieve().body(String.class);
}

public String fallback(Exception ex) {
    log.warn("Fallback triggered for external service: {}", ex.getMessage());
    return "default";
}
```

## Constraints

- NEVER expose stack traces in API responses
- ALWAYS log the full exception at `error` level for unexpected errors
- ALWAYS use typed exceptions — avoid throwing raw `RuntimeException`
- ALWAYS include correlation ID in error logs via MDC
- ALWAYS return `ApiResponse` envelope — never raw error bodies
