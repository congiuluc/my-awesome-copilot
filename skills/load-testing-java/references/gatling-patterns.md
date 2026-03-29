# Gatling Patterns for Java Load Testing

## HTTP Protocol Configuration

```java
import io.gatling.javaapi.http.HttpProtocolBuilder;
import static io.gatling.javaapi.http.HttpDsl.*;

HttpProtocolBuilder httpProtocol = http
    .baseUrl("http://localhost:8080")
    .acceptHeader("application/json")
    .contentTypeHeader("application/json")
    .userAgentHeader("Gatling/LoadTest")
    .shareConnections();
```

## Basic GET Scenario

```java
import io.gatling.javaapi.core.*;
import static io.gatling.javaapi.core.CoreDsl.*;
import static io.gatling.javaapi.http.HttpDsl.*;

ScenarioBuilder getProducts = scenario("Get Products")
    .exec(
        http("list products")
            .get("/api/products")
            .check(status().is(200))
            .check(jsonPath("$[*]").count().gte(1))
    );
```

## CRUD Flow Scenario

```java
ScenarioBuilder crudFlow = scenario("Product CRUD")
    .exec(
        http("create product")
            .post("/api/products")
            .body(StringBody("""
                {"name": "Gatling Product", "price": 19.99}
                """))
            .check(status().is(201))
            .check(jsonPath("$.id").saveAs("productId"))
    )
    .pause(1)
    .exec(
        http("get product")
            .get("/api/products/#{productId}")
            .check(status().is(200))
            .check(jsonPath("$.name").is("Gatling Product"))
    )
    .pause(1)
    .exec(
        http("update product")
            .put("/api/products/#{productId}")
            .body(StringBody("""
                {"name": "Updated Product", "price": 29.99}
                """))
            .check(status().is(200))
    )
    .pause(1)
    .exec(
        http("delete product")
            .delete("/api/products/#{productId}")
            .check(status().is(204))
    );
```

## Authentication Flow

```java
ScenarioBuilder authenticatedFlow = scenario("Authenticated Flow")
    .exec(
        http("login")
            .post("/api/auth/login")
            .body(StringBody("""
                {"email": "test@example.com", "password": "Test123!"}
                """))
            .check(status().is(200))
            .check(jsonPath("$.token").saveAs("authToken"))
    )
    .exec(
        http("get protected resource")
            .get("/api/products")
            .header("Authorization", "Bearer #{authToken}")
            .check(status().is(200))
    );
```

## Feeders (Data Parameterization)

### CSV Feeder

```java
// File: src/gatling/resources/feeders/products.csv
// name,price
// Widget A,9.99
// Widget B,19.99
// Widget C,29.99

FeederBuilder<String> csvFeeder = csv("feeders/products.csv").circular();

ScenarioBuilder dataScenario = scenario("Data Driven")
    .feed(csvFeeder)
    .exec(
        http("create product #{name}")
            .post("/api/products")
            .body(StringBody("""
                {"name": "#{name}", "price": #{price}}
                """))
            .check(status().is(201))
    );
```

### JSON Feeder

```java
FeederBuilder<Object> jsonFeeder = jsonFile("feeders/products.json").random();
```

### Custom Feeder (Iterator-Based)

```java
import java.util.*;
import java.util.function.Supplier;
import java.util.stream.Stream;

Iterator<Map<String, Object>> customFeeder =
    Stream.generate((Supplier<Map<String, Object>>) () -> {
        var random = new Random();
        return Map.of(
            "name", "Product-" + random.nextInt(10000),
            "price", 5.0 + random.nextDouble() * 95.0
        );
    }).iterator();

ScenarioBuilder customFeedScenario = scenario("Custom Feed")
    .feed(customFeeder)
    .exec(
        http("create random product")
            .post("/api/products")
            .body(StringBody("""
                {"name": "#{name}", "price": #{price}}
                """))
            .check(status().is(201))
    );
```

## Load Simulation Patterns

### Smoke Test (Verify Script Works)

```java
setUp(
    getProducts.injectOpen(atOnceUsers(1))
).protocols(httpProtocol);
```

### Load Test (Sustained Normal Traffic)

```java
setUp(
    getProducts.injectOpen(
        rampUsers(50).during(Duration.ofSeconds(30)),
        constantUsersPerSec(10).during(Duration.ofMinutes(2))
    )
).protocols(httpProtocol);
```

### Stress Test (Find Breaking Point)

```java
setUp(
    getProducts.injectOpen(
        rampUsers(100).during(Duration.ofSeconds(30)),
        rampUsers(200).during(Duration.ofSeconds(30)),
        rampUsers(500).during(Duration.ofSeconds(30))
    )
).protocols(httpProtocol);
```

### Spike Test (Sudden Burst)

```java
setUp(
    getProducts.injectOpen(
        nothingFor(Duration.ofSeconds(5)),
        atOnceUsers(200),
        nothingFor(Duration.ofSeconds(10)),
        atOnceUsers(200)
    )
).protocols(httpProtocol);
```

### Closed Model (Constant Concurrent Users)

```java
setUp(
    getProducts.injectClosed(
        constantConcurrentUsers(50).during(Duration.ofMinutes(2))
    )
).protocols(httpProtocol);
```

### Ramp Closed Model

```java
setUp(
    getProducts.injectClosed(
        rampConcurrentUsers(0).to(100).during(Duration.ofMinutes(1)),
        constantConcurrentUsers(100).during(Duration.ofMinutes(3))
    )
).protocols(httpProtocol);
```

## Assertions

```java
setUp(
    getProducts.injectOpen(
        constantUsersPerSec(10).during(Duration.ofMinutes(2))
    )
).protocols(httpProtocol)
 .assertions(
     global().responseTime().percentile(95.0).lt(200),
     global().responseTime().percentile(99.0).lt(500),
     global().failedRequests().percent().lt(1.0),
     global().requestsPerSec().gte(50.0)
 );
```

## Checks Reference

```java
// Status code
check(status().is(200))
check(status().in(200, 201))

// Response body — JSON path
check(jsonPath("$.id").exists())
check(jsonPath("$.id").saveAs("entityId"))
check(jsonPath("$.name").is("expected"))
check(jsonPath("$[*]").count().gte(1))

// Response body — regex
check(regex("\"id\":\"(.+?)\"").saveAs("entityId"))

// Response time
check(responseTimeInMillis().lt(500))

// Header
check(header("Content-Type").is("application/json"))
```

## Throttling

```java
setUp(
    getProducts.injectOpen(
        constantUsersPerSec(100).during(Duration.ofMinutes(5))
    )
).protocols(httpProtocol)
 .throttle(
     reachRps(50).in(Duration.ofSeconds(10)),
     holdFor(Duration.ofMinutes(1)),
     reachRps(100).in(Duration.ofSeconds(10)),
     holdFor(Duration.ofMinutes(2))
 );
```

## gatling.conf Key Settings

```hocon
gatling {
  core {
    runDescription = "Product API Load Test"
    encoding = "utf-8"
    elFileBodiesCacheMaxCapacity = 200
  }
  http {
    fetchedHtmlResourcesUrlFilters = []
    perUserNameResolution = false
    connectionTimeout = 10000   # 10 seconds
    requestTimeout = 30000      # 30 seconds
  }
  data {
    writers = [console, file]
  }
}
```

## CI/CD Integration (GitHub Actions)

```yaml
- name: Run Gatling tests
  run: mvn gatling:test -Dgatling.simulationClass=simulations.ProductSimulation

- name: Upload Gatling report
  uses: actions/upload-artifact@v4
  if: always()
  with:
    name: gatling-report
    path: target/gatling/**/index.html
```
