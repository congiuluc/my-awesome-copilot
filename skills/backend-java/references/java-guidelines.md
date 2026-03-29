# Java Spring Boot Guidelines

## Framework & Language

- Target **Java 21** (latest LTS). Use modern features: records, sealed classes, pattern matching, virtual threads.
- Use **Spring Boot 3.x** with Spring Web MVC for synchronous APIs, or WebFlux for reactive.
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
- Infrastructure contains: repository adapters (JPA, MongoDB), external service clients, configuration.
- API contains: REST controllers, request/response DTOs, exception handlers, filters, OpenAPI config.

## API Response Envelope

All endpoints return a standard envelope:

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

Return `ApiResponse` from every endpoint — never raw objects or primitives.

## Controller Organization

- Group related endpoints in a single `@RestController` with `@RequestMapping` prefix.
- Use `@Tag` from SpringDoc for OpenAPI grouping.
- Use `@Operation(summary = "...")` for endpoint documentation.
- Apply `@Valid` on request body parameters for validation.

```java
@RestController
@RequestMapping("/api/products")
@Tag(name = "Product", description = "Product management")
public class ProductController {

    private final ProductService productService;

    public ProductController(ProductService productService) {
        this.productService = productService;
    }

    @GetMapping
    @Operation(summary = "Get all products")
    public ResponseEntity<ApiResponse<List<ProductResponse>>> getAll() {
        var products = productService.findAll();
        return ResponseEntity.ok(ApiResponse.ok(products));
    }

    @GetMapping("/{id}")
    @Operation(summary = "Get a product by ID")
    public ResponseEntity<ApiResponse<ProductResponse>> getById(@PathVariable String id) {
        var product = productService.findById(id);
        return ResponseEntity.ok(ApiResponse.ok(product));
    }

    @PostMapping
    @Operation(summary = "Create a new product")
    public ResponseEntity<ApiResponse<ProductResponse>> create(
            @Valid @RequestBody CreateProductRequest request) {
        var product = productService.create(request);
        var uri = URI.create("/api/products/" + product.id());
        return ResponseEntity.created(uri).body(ApiResponse.ok(product));
    }
}
```

## Dependency Injection

- Use **constructor injection** exclusively — no `@Autowired` on fields.
- Use `@Configuration` classes for custom bean registration.
- Use `@Service` for application services, `@Repository` for data access, `@Component` for general beans.
- Use `@Qualifier` or custom annotations when multiple implementations exist.

```java
@Configuration
public class ProductConfig {

    @Bean
    public ProductService productService(ProductRepository productRepository) {
        return new ProductServiceImpl(productRepository);
    }
}
```

## Logging (SLF4J + Logback)

- Use `SLF4J` with `LoggerFactory` for structured logging.
- Use parameterized messages: `log.info("Processing product {}", productId);`
- Never log sensitive data (passwords, tokens, PII, connection strings).
- Use `Lombok @Slf4j` if Lombok is present, otherwise manual `LoggerFactory`.
- Add MDC (Mapped Diagnostic Context) for correlation IDs.

```java
private static final Logger log = LoggerFactory.getLogger(ProductServiceImpl.class);

public ProductResponse findById(String id) {
    log.debug("Fetching product with id={}", id);
    // ...
}
```

## Transaction Management

- Use `@Transactional` on service methods that modify data.
- Use `@Transactional(readOnly = true)` on read-only service methods.
- Never place `@Transactional` on controllers — keep it at the service layer.
- Use `propagation = REQUIRES_NEW` only when isolation is explicitly needed.

## Configuration

- Use `@ConfigurationProperties` for typed settings.
- Environment-specific files: `application.yml`, `application-dev.yml`, `application-prod.yml`.
- Never hardcode connection strings or secrets — use environment variables or vault.
- Validate configuration at startup with `@Validated` + Jakarta annotations.

```java
@Validated
@ConfigurationProperties(prefix = "app.database")
public record DatabaseProperties(
        @NotBlank String url,
        @NotBlank String username,
        @Min(1) @Max(100) int maxPoolSize) {
}
```

## Health Checks (Actuator)

- Enable Spring Boot Actuator with health, info, and metrics endpoints.
- Add custom health indicators for external dependencies.
- Secure actuator endpoints — expose only `/actuator/health` publicly.
- Configure separate readiness and liveness probes for Kubernetes.

```java
@Component
public class DatabaseHealthIndicator implements HealthIndicator {

    private final DataSource dataSource;

    public DatabaseHealthIndicator(DataSource dataSource) {
        this.dataSource = dataSource;
    }

    @Override
    public Health health() {
        try (var conn = dataSource.getConnection()) {
            return Health.up().build();
        } catch (Exception e) {
            return Health.down(e).build();
        }
    }
}
```

## Input Validation

- Validate all incoming requests with Jakarta Bean Validation annotations.
- Use `@Valid` on `@RequestBody` parameters.
- Handle `MethodArgumentNotValidException` in `@RestControllerAdvice`.
- Return `400 Bad Request` with structured error messages.

```java
public record CreateProductRequest(
        @NotBlank(message = "Name is required")
        @Size(max = 100, message = "Name must be at most 100 characters")
        String name,

        @Size(max = 500, message = "Description must be at most 500 characters")
        String description,

        @NotNull(message = "Price is required")
        @Positive(message = "Price must be positive")
        BigDecimal price) {
}
```
