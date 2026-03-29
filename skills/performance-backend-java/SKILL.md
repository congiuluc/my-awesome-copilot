---
name: performance-backend-java
description: >-
  Optimize Java Spring Boot backend performance with caching, response compression,
  pagination, database query optimization, and JVM tuning. Use when: adding Spring
  Cache, Redis caching, response compression, optimizing JPA/Hibernate queries,
  implementing cursor/offset pagination, or profiling Java backend bottlenecks.
argument-hint: 'Describe the backend performance bottleneck or optimization needed.'
---

# Backend Performance Optimization (Java)

## When to Use

- Adding caching to frequently accessed data (Spring Cache, Redis, Caffeine)
- Optimizing JPA/Hibernate queries or repository methods
- Implementing pagination for large datasets
- Adding response compression (Gzip, Brotli)
- Profiling and fixing Java backend performance bottlenecks
- Tuning JVM settings for throughput or latency

## Official Documentation

- [Spring Cache Abstraction](https://docs.spring.io/spring-boot/reference/io/caching.html)
- [Spring Data JPA — Pagination](https://docs.spring.io/spring-data/jpa/reference/repositories/query-methods-details.html#repositories.limit-query-result)
- [Hibernate Performance Tuning](https://docs.jboss.org/hibernate/orm/current/userguide/html_single/Hibernate_User_Guide.html#performance)
- [Spring Boot Actuator](https://docs.spring.io/spring-boot/reference/actuator/)
- [Micrometer](https://micrometer.io/docs)

## Procedure

1. Identify bottleneck: database query, I/O wait, serialization, missing index, JVM GC
2. Apply [caching and optimization patterns](./references/backend-performance-java.md)
3. Review [caching sample](./samples/CachingConfig.java)
4. Add pagination for list endpoints (Spring Data `Pageable`, cursor-based)
5. Use `@EntityGraph` or `JOIN FETCH` to avoid N+1 queries
6. Use projections / DTOs instead of loading full entities for read-only use cases
7. Measure before and after with Micrometer metrics and Actuator
8. Add performance-sensitive tests for critical paths

## Performance Review Checklist

| # | Check | Tool / Method |
|---|-------|---------------|
| 1 | All list endpoints accept `Pageable` | Code review |
| 2 | Read-only queries use `@Transactional(readOnly = true)` | Code review |
| 3 | Only needed columns projected (DTO projections) | Code review |
| 4 | No N+1 queries (use `@EntityGraph` or batch fetching) | Hibernate SQL logging |
| 5 | Response compression enabled (Gzip) | `server.compression.enabled=true` |
| 6 | Hot data cached with `@Cacheable` and appropriate TTL | Code review |
| 7 | Cache invalidation with `@CacheEvict` on write ops | Code review |
| 8 | No blocking I/O in reactive/virtual-thread context | Code review |
| 9 | Connection pool sized correctly (HikariCP) | Config review |
| 10 | Database indexes on frequently queried columns | Schema review |
| 11 | No `EAGER` fetch types on `@ManyToOne`/`@OneToMany` | Code review |
| 12 | Batch inserts/updates use JDBC batching | Hibernate config review |

## Profiling Workflow

1. Enable Hibernate SQL logging: `spring.jpa.show-sql=true` + `logging.level.org.hibernate.SQL=DEBUG`
2. Use Spring Boot Actuator metrics: `/actuator/metrics/http.server.requests`
3. Use `jconsole` or `VisualVM` for JVM profiling
4. Use `async-profiler` for CPU flame graphs
5. Use Micrometer timers for custom operation timing
6. Run JMH benchmarks for micro-benchmarks on critical paths
