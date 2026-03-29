---
name: logging-java
description: >-
  Configure structured logging with SLF4J + Logback for Java Spring Boot applications.
  Covers MDC context, correlation IDs, log levels, sensitive data filtering, and
  JSON formatting. Use when: setting up logging, configuring Logback, adding MDC
  context, filtering sensitive data, or troubleshooting logging issues in Java.
argument-hint: 'Describe the logging requirement: MDC setup, JSON output, filtering, or structured context.'
---

# Structured Logging with SLF4J + Logback (Java)

## When to Use

- Setting up SLF4J + Logback in a Spring Boot project
- Configuring JSON log output for production
- Adding MDC context (correlation IDs, user IDs, operation names)
- Filtering sensitive data from log output
- Adjusting log levels per package
- Troubleshooting log output issues

## Official Documentation

- [SLF4J Manual](https://www.slf4j.org/manual.html)
- [Logback Documentation](https://logback.qos.ch/documentation.html)
- [Spring Boot Logging](https://docs.spring.io/spring-boot/reference/features/logging.html)
- [Logstash Logback Encoder](https://github.com/logfellow/logstash-logback-encoder)

## Key Principles

- **Structured, not string-concatenated** — use parameterized messages with `{}` placeholders.
- **MDC for context** — push correlation IDs, user IDs, and operation names to MDC.
- **Sensitive data never logged** — filter passwords, tokens, PII at configuration level.
- **Correlation is mandatory** — every request gets a correlation ID for tracing.
- **Levels are meaningful** — use the right level for the right situation.

## Procedure

1. Configure Logback via `logback-spring.xml`
2. Review [Logback config sample](./samples/logback-spring.xml)
3. Add MDC filter for correlation ID
4. Use SLF4J parameterized logging — never string concatenation
5. Configure JSON output for production profile
6. Filter sensitive namespaces (Spring Security, Hibernate SQL)

## Log Levels Guide

| Level | Use When | Examples |
|-------|----------|---------|
| `TRACE` | Internal framework detail | Hibernate SQL, Spring MVC handler mapping |
| `DEBUG` | Developer diagnostics | Cache hit/miss, config loaded, request parsed |
| `INFO` | Normal operations | Request started, user logged in, order created |
| `WARN` | Expected but notable | Not found, validation failed, slow query |
| `ERROR` | Unexpected failures | Unhandled exceptions, external service down |

## Structured Logging Patterns

```java
// ✅ Good — parameterized with SLF4J
log.info("Order {} created for user {}", orderId, userId);

// ✅ Good — with MDC context
MDC.put("orderId", orderId);
log.info("Processing order");
MDC.remove("orderId");

// ❌ Bad — string concatenation (evaluated even if level disabled)
log.info("Order " + orderId + " created for user " + userId);
```

## Correlation ID Filter

```java
@Component
@Order(Ordered.HIGHEST_PRECEDENCE)
public class CorrelationIdFilter extends OncePerRequestFilter {

    private static final String HEADER = "X-Correlation-ID";

    @Override
    protected void doFilterInternal(HttpServletRequest request,
            HttpServletResponse response, FilterChain chain)
            throws ServletException, IOException {
        String correlationId = request.getHeader(HEADER);
        if (correlationId == null || correlationId.isBlank()) {
            correlationId = UUID.randomUUID().toString();
        }
        MDC.put("correlationId", correlationId);
        response.setHeader(HEADER, correlationId);
        try {
            chain.doFilter(request, response);
        } finally {
            MDC.remove("correlationId");
        }
    }
}
```

## Logback Configuration (application.yml)

```yaml
logging:
  level:
    root: INFO
    com.myapp: DEBUG
    org.springframework.security: WARN
    org.hibernate.SQL: WARN
    org.hibernate.type.descriptor.sql.BasicBinder: WARN
```

## Constraints

- NEVER use string concatenation in log statements
- ALWAYS use SLF4J parameterized logging (`{}` placeholders)
- ALWAYS clean up MDC context in a `finally` block
- ALWAYS filter sensitive namespaces in production
- NEVER log passwords, tokens, or PII
