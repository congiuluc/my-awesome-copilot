# Java Backend Performance Patterns

## Caching with Spring Cache

### Caffeine (In-Memory)

```java
@Configuration
@EnableCaching
public class CacheConfig {

    @Bean
    public CacheManager cacheManager() {
        var caffeine = Caffeine.newBuilder()
            .maximumSize(500)
            .expireAfterWrite(Duration.ofMinutes(10))
            .recordStats();

        var manager = new CaffeineCacheManager();
        manager.setCaffeine(caffeine);
        return manager;
    }
}
```

### Redis (Distributed)

```java
@Configuration
@EnableCaching
public class RedisCacheConfig {

    @Bean
    public RedisCacheManager cacheManager(RedisConnectionFactory factory) {
        var config = RedisCacheConfiguration.defaultCacheConfig()
            .entryTtl(Duration.ofMinutes(15))
            .serializeValuesWith(
                SerializationPair.fromSerializer(new GenericJackson2JsonRedisSerializer()));

        return RedisCacheManager.builder(factory)
            .cacheDefaults(config)
            .build();
    }
}
```

### Usage in Service

```java
@Service
public class ProductService {

    @Cacheable(value = "products", key = "#id")
    public ProductDto getById(String id) { /* ... */ }

    @CacheEvict(value = "products", key = "#id")
    public void update(String id, UpdateProductRequest request) { /* ... */ }

    @CacheEvict(value = "products", allEntries = true)
    public void deleteAll() { /* ... */ }
}
```

## Response Compression

In `application.yml`:

```yaml
server:
  compression:
    enabled: true
    mime-types: application/json,application/xml,text/html,text/plain
    min-response-size: 1024
```

## Pagination

### Offset-Based (Spring Data)

```java
@GetMapping
public ApiResponse<Page<ProductDto>> getAll(Pageable pageable) {
    var page = repository.findAll(pageable).map(mapper::toDto);
    return ApiResponse.success(page);
}
```

### Cursor-Based

```java
@GetMapping
public ApiResponse<CursorPage<ProductDto>> getAll(
    @RequestParam(required = false) String cursor,
    @RequestParam(defaultValue = "20") int limit) {

    var items = repository.findAfterCursor(cursor, limit + 1);
    var hasMore = items.size() > limit;
    var page = hasMore ? items.subList(0, limit) : items;
    var nextCursor = hasMore ? page.get(page.size() - 1).getId() : null;

    return ApiResponse.success(new CursorPage<>(page, nextCursor, hasMore));
}
```

## JPA/Hibernate Optimization

### Avoid N+1 with EntityGraph

```java
@EntityGraph(attributePaths = {"category", "tags"})
List<Product> findAll();
```

### Batch Fetching

```yaml
spring:
  jpa:
    properties:
      hibernate:
        default_batch_fetch_size: 25
        jdbc:
          batch_size: 50
        order_inserts: true
        order_updates: true
```

### DTO Projections (Read-Only)

```java
public interface ProductSummary {
    String getId();
    String getName();
    BigDecimal getPrice();
}

List<ProductSummary> findAllProjectedBy();
```

## HikariCP Connection Pool

```yaml
spring:
  datasource:
    hikari:
      maximum-pool-size: 20
      minimum-idle: 5
      connection-timeout: 30000
      idle-timeout: 600000
      max-lifetime: 1800000
```

## Micrometer Custom Metrics

```java
@Service
public class ProductService {

    private final Timer queryTimer;

    public ProductService(MeterRegistry registry) {
        this.queryTimer = Timer.builder("product.query.time")
            .description("Time to query products")
            .register(registry);
    }

    public List<ProductDto> search(String query) {
        return queryTimer.record(() -> repository.search(query));
    }
}
```
