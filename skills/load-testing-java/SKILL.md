---
name: load-testing-java
description: >-
  Load test Java Spring Boot APIs using Gatling (JVM-native) and k6. Covers
  simulation design, feeders, assertions, Gradle/Maven plugin integration, and
  CI/CD pipelines. Use when: writing load tests in Java/Scala with Gatling,
  testing Spring Boot endpoints under load, integrating Gatling into Maven or
  Gradle builds, or comparing Gatling vs k6 for a Java project.
argument-hint: 'Describe the Java API endpoint or scenario to load test.'
---

# Load Testing for Java (Gatling + k6)

## When to Use

- Writing load tests co-located with Java projects (Maven/Gradle)
- Testing Spring Boot REST API endpoints under realistic load
- Need rich HTML reports with detailed percentile breakdowns
- Need feeder-based data parameterization for large datasets
- Already using k6 but need supplementary JVM-native tests

## Official Documentation

- [Gatling Documentation](https://docs.gatling.io/)
- [Gatling Maven Plugin](https://docs.gatling.io/reference/integrations/build-tools/maven-plugin/)
- [Gatling Gradle Plugin](https://docs.gatling.io/reference/integrations/build-tools/gradle-plugin/)
- [k6 Documentation](https://grafana.com/docs/k6/latest/) (see `load-testing` skill)

## When to Use Gatling vs k6

| Criteria | Gatling | k6 |
|----------|---------|-----|
| Language | Java / Scala / Kotlin | JavaScript |
| Best for | JVM teams, data-driven | Cross-team, CI/CD gates |
| Protocol | HTTP, WebSocket, JMS, MQTT | HTTP, WebSocket, gRPC |
| Build integration | Maven/Gradle plugin | CLI / GitHub Action |
| Reports | Rich HTML (built-in) | Console, JSON, Prometheus |
| Data feeds | CSV, JSON, JDBC, custom | SharedArray, JSON |

**Recommendation**: Use Gatling for JVM teams that want IDE-integrated, data-driven
load tests with rich HTML reports. Use k6 for quick CI/CD pipeline smoke tests.

## Procedure

1. Add Gatling dependencies and plugin to build file
2. Follow [Gatling patterns](./references/gatling-patterns.md)
3. Review [product simulation sample](./samples/ProductSimulation.java)
4. Design simulations with feeders, checks, and assertions
5. Run locally: `mvn gatling:test` or `gradle gatlingRun`
6. For CI/CD pipeline integration, use k6 (see `load-testing` skill)
7. Analyze HTML report in `target/gatling/` or `build/reports/gatling/`

## Maven Dependencies

```xml
<dependencies>
    <dependency>
        <groupId>io.gatling.highcharts</groupId>
        <artifactId>gatling-charts-highcharts</artifactId>
        <scope>test</scope>
    </dependency>
    <dependency>
        <groupId>io.gatling</groupId>
        <artifactId>gatling-test-framework</artifactId>
        <scope>test</scope>
    </dependency>
</dependencies>

<build>
    <plugins>
        <plugin>
            <groupId>io.gatling</groupId>
            <artifactId>gatling-maven-plugin</artifactId>
        </plugin>
    </plugins>
</build>
```

## Gradle Dependencies

```groovy
plugins {
    id 'io.gatling.gradle' version '3.12.0'
}

dependencies {
    gatling 'io.gatling.highcharts:gatling-charts-highcharts'
}
```

## Project Structure

```
src/
  gatling/
    java/
      simulations/
        ProductSimulation.java    # Product API simulation
        AuthSimulation.java       # Auth flow simulation
    resources/
      feeders/
        products.csv              # Test data
      gatling.conf                # Gatling configuration
      logback-test.xml            # Logging config
```
