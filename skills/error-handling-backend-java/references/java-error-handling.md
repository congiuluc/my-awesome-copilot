# Java Backend Error Handling Patterns

## Exception Hierarchy

Define in `domain/exceptions/`:

```java
/**
 * Base exception for all domain errors.
 */
public abstract class AppException extends RuntimeException {

    private final int statusCode;

    protected AppException(String message, int statusCode) {
        super(message);
        this.statusCode = statusCode;
    }

    public int getStatusCode() {
        return statusCode;
    }
}

public class NotFoundException extends AppException {
    public NotFoundException(String entityName, Object id) {
        super("%s with ID '%s' was not found.".formatted(entityName, id), 404);
    }
}

public class ValidationException extends AppException {
    public ValidationException(String message) {
        super(message, 400);
    }
}

public class ConflictException extends AppException {
    public ConflictException(String message) {
        super(message, 409);
    }
}

public class BusinessRuleException extends AppException {
    public BusinessRuleException(String message) {
        super(message, 422);
    }
}
```

## @RestControllerAdvice Patterns

### Handling Jakarta Validation Errors

```java
@ExceptionHandler(MethodArgumentNotValidException.class)
public ResponseEntity<ApiResponse<Map<String, String>>> handleValidation(
        MethodArgumentNotValidException ex) {
    Map<String, String> errors = new HashMap<>();
    ex.getBindingResult().getFieldErrors().forEach(e ->
        errors.put(e.getField(), e.getDefaultMessage()));
    log.warn("Validation errors: {}", errors);
    return ResponseEntity.badRequest()
        .body(ApiResponse.error("Validation failed", errors));
}
```

### Handling Spring Security Errors

```java
@ExceptionHandler(AccessDeniedException.class)
public ResponseEntity<ApiResponse<Void>> handleAccessDenied(AccessDeniedException ex) {
    log.warn("Access denied: {}", ex.getMessage());
    return ResponseEntity.status(HttpStatus.FORBIDDEN)
        .body(ApiResponse.error("Access denied."));
}
```

### Handling Data Integrity Violations

```java
@ExceptionHandler(DataIntegrityViolationException.class)
public ResponseEntity<ApiResponse<Void>> handleDataIntegrity(
        DataIntegrityViolationException ex) {
    log.warn("Data integrity violation: {}", ex.getMostSpecificCause().getMessage());
    return ResponseEntity.status(HttpStatus.CONFLICT)
        .body(ApiResponse.error("Resource conflict — operation could not be completed."));
}
```

## MDC Correlation for Error Logging

```java
import org.slf4j.MDC;
import jakarta.servlet.*;
import java.util.UUID;

public class CorrelationIdFilter implements Filter {

    @Override
    public void doFilter(ServletRequest request, ServletResponse response,
                         FilterChain chain) throws IOException, ServletException {
        String correlationId = UUID.randomUUID().toString();
        MDC.put("correlationId", correlationId);
        try {
            chain.doFilter(request, response);
        } finally {
            MDC.remove("correlationId");
        }
    }
}
```

## Resilience4j Configuration

### application.yml

```yaml
resilience4j:
  retry:
    instances:
      externalService:
        max-attempts: 3
        wait-duration: 1s
        exponential-backoff-multiplier: 2
  circuitbreaker:
    instances:
      externalService:
        sliding-window-size: 10
        failure-rate-threshold: 50
        wait-duration-in-open-state: 10s
```

### Annotations

```java
@Retry(name = "externalService", fallbackMethod = "fallback")
@CircuitBreaker(name = "externalService")
@TimeLimiter(name = "externalService")
public CompletionStage<String> callExternalService() {
    return CompletableFuture.supplyAsync(() ->
        restClient.get().uri("/api/data").retrieve().body(String.class));
}

public CompletionStage<String> fallback(Exception ex) {
    log.warn("Fallback triggered: {}", ex.getMessage());
    return CompletableFuture.completedFuture("default");
}
```
