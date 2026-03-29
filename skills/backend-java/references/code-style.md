# Java Code Style

## Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Package | lowercase.dotted | `com.myapp.domain.model` |
| Class / Interface | PascalCase | `ProductService` |
| Record | PascalCase | `CreateProductRequest` |
| Method | camelCase | `findById` |
| Field (private) | camelCase | `productRepository` |
| Parameter | camelCase | `productId` |
| Constant | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |
| Enum member | UPPER_SNAKE_CASE | `ACTIVE`, `INACTIVE` |
| Local variable | camelCase | `productDto` |

## File Organization

```java
// 1. Package declaration
package com.myapp.api.controller;

// 2. Imports (grouped: java.*, jakarta.*, org.*, com.*, static)
import java.util.List;

import jakarta.validation.Valid;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import com.myapp.application.service.ProductService;
import com.myapp.api.dto.ProductResponse;

// 3. Class Javadoc
/**
 * REST controller for product operations.
 */
@RestController
@RequestMapping("/api/products")
@Tag(name = "Product", description = "Product management endpoints")
public class ProductController {

    // 4. Fields (private final for injected dependencies)
    private final ProductService productService;

    // 5. Constructor
    public ProductController(ProductService productService) {
        this.productService = productService;
    }

    // 6. Public methods (endpoints)
    // 7. Private helper methods
}
```

## Rules

- Javadoc (`/** */`) on all public classes, methods, and non-trivial fields.
- Max line length: 120 characters — break long lines at commas or operators.
- Use **constructor injection** exclusively — no `@Autowired` on fields.
- One public class per file. File name matches class name.
- Use `var` for local variables when the type is obvious (Java 10+).
- Use **records** for DTOs, request/response objects, and value objects.
- Use **sealed interfaces** for domain type hierarchies.
- Use `Optional<T>` for nullable returns — never return `null` from service methods.
- Prefer `List.of()`, `Map.of()`, `Set.of()` for immutable collections.
- Use `Objects.requireNonNull()` for null checks on constructor parameters.
- Use text blocks (`"""`) for multi-line strings.
- Use pattern matching for `instanceof` (Java 16+).
- Use switch expressions (Java 14+).

## Method Style

```java
/**
 * Retrieves a product by its unique identifier.
 *
 * @param id the product identifier
 * @return the product if found
 * @throws ProductNotFoundException if no product exists with the given id
 */
public ProductResponse findById(String id) {
    Objects.requireNonNull(id, "id must not be null");
    return productRepository.findById(id)
            .map(ProductResponse::from)
            .orElseThrow(() -> new ProductNotFoundException(id));
}
```

## Record / DTO Style

```java
/**
 * Request to create a new product.
 *
 * @param name display name of the product
 * @param description optional description
 * @param price product price
 */
public record CreateProductRequest(
        @NotBlank @Size(max = 100) String name,
        @Size(max = 500) String description,
        @NotNull @Positive BigDecimal price) {
}

/**
 * Product data returned to API consumers.
 */
public record ProductResponse(
        String id,
        String name,
        String description,
        BigDecimal price,
        Instant createdAt) {

    public static ProductResponse from(Product product) {
        return new ProductResponse(
                product.getId(),
                product.getName(),
                product.getDescription(),
                product.getPrice(),
                product.getCreatedAt());
    }
}
```

## Exception Handling Style

```java
/**
 * Global exception handler for REST controllers.
 */
@RestControllerAdvice
public class GlobalExceptionHandler {

    @ExceptionHandler(ProductNotFoundException.class)
    public ResponseEntity<ApiResponse<Void>> handleNotFound(ProductNotFoundException ex) {
        return ResponseEntity.status(HttpStatus.NOT_FOUND)
                .body(ApiResponse.error(ex.getMessage()));
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ApiResponse<Void>> handleValidation(
            MethodArgumentNotValidException ex) {
        var errors = ex.getBindingResult().getFieldErrors().stream()
                .map(e -> e.getField() + ": " + e.getDefaultMessage())
                .toList();
        return ResponseEntity.badRequest()
                .body(ApiResponse.error(String.join("; ", errors)));
    }
}
```
