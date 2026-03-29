package simulations;

import io.gatling.javaapi.core.*;
import io.gatling.javaapi.http.*;

import java.time.Duration;
import java.util.*;
import java.util.function.Supplier;
import java.util.stream.Stream;

import static io.gatling.javaapi.core.CoreDsl.*;
import static io.gatling.javaapi.http.HttpDsl.*;

/**
 * Load test simulation for the Product API.
 *
 * <p>Run with Maven: {@code mvn gatling:test -Dgatling.simulationClass=simulations.ProductSimulation}
 * <p>Run with Gradle: {@code gradle gatlingRun-simulations.ProductSimulation}
 */
public class ProductSimulation extends Simulation {

    // ---------------------------------------------------------------
    // Configuration
    // ---------------------------------------------------------------

    private static final String BASE_URL =
        System.getProperty("baseUrl", "http://localhost:8080");

    private final HttpProtocolBuilder httpProtocol = http
        .baseUrl(BASE_URL)
        .acceptHeader("application/json")
        .contentTypeHeader("application/json")
        .userAgentHeader("Gatling/ProductSimulation");

    // ---------------------------------------------------------------
    // Feeders
    // ---------------------------------------------------------------

    private final Iterator<Map<String, Object>> productFeeder =
        Stream.generate((Supplier<Map<String, Object>>) () -> {
            var random = new Random();
            return Map.of(
                "productName", "Product-" + random.nextInt(100_000),
                "productPrice", Math.round((5.0 + random.nextDouble() * 95.0) * 100.0) / 100.0
            );
        }).iterator();

    // ---------------------------------------------------------------
    // Scenarios
    // ---------------------------------------------------------------

    /**
     * Browse products — simple read-only scenario.
     */
    private final ScenarioBuilder browse = scenario("Browse Products")
        .exec(
            http("GET /api/products")
                .get("/api/products")
                .check(status().is(200))
                .check(jsonPath("$[0].id").optional().saveAs("firstProductId"))
        )
        .doIf("#{firstProductId.exists()}").then(
            exec(
                http("GET /api/products/{id}")
                    .get("/api/products/#{firstProductId}")
                    .check(status().is(200))
            )
        );

    /**
     * CRUD products — create, read, update, delete cycle.
     */
    private final ScenarioBuilder crud = scenario("CRUD Products")
        .feed(productFeeder)
        .exec(
            http("POST /api/products")
                .post("/api/products")
                .body(StringBody("""
                    {"name": "#{productName}", "price": #{productPrice}}
                    """))
                .check(status().is(201))
                .check(jsonPath("$.id").saveAs("createdId"))
        )
        .pause(Duration.ofMillis(500), Duration.ofSeconds(1))
        .exec(
            http("GET /api/products/{id}")
                .get("/api/products/#{createdId}")
                .check(status().is(200))
                .check(jsonPath("$.name").is("#{productName}"))
        )
        .pause(Duration.ofMillis(500), Duration.ofSeconds(1))
        .exec(
            http("PUT /api/products/{id}")
                .put("/api/products/#{createdId}")
                .body(StringBody("""
                    {"name": "#{productName}-updated", "price": #{productPrice}}
                    """))
                .check(status().is(200))
        )
        .pause(Duration.ofMillis(500), Duration.ofSeconds(1))
        .exec(
            http("DELETE /api/products/{id}")
                .delete("/api/products/#{createdId}")
                .check(status().is(204))
        );

    // ---------------------------------------------------------------
    // Load Profile
    // ---------------------------------------------------------------

    {
        String profile = System.getProperty("loadProfile", "smoke");

        switch (profile) {
            case "load" -> setUp(
                browse.injectOpen(
                    rampUsers(50).during(Duration.ofSeconds(30)),
                    constantUsersPerSec(10).during(Duration.ofMinutes(3))
                ),
                crud.injectOpen(
                    rampUsers(20).during(Duration.ofSeconds(30)),
                    constantUsersPerSec(5).during(Duration.ofMinutes(3))
                )
            ).protocols(httpProtocol)
             .assertions(
                 global().responseTime().percentile(95.0).lt(200),
                 global().responseTime().percentile(99.0).lt(500),
                 global().failedRequests().percent().lt(1.0),
                 global().requestsPerSec().gte(50.0)
             );

            case "stress" -> setUp(
                browse.injectOpen(
                    rampUsers(100).during(Duration.ofSeconds(30)),
                    rampUsers(300).during(Duration.ofSeconds(30)),
                    rampUsers(500).during(Duration.ofSeconds(30))
                ),
                crud.injectOpen(
                    rampUsers(50).during(Duration.ofSeconds(30)),
                    rampUsers(100).during(Duration.ofSeconds(30)),
                    rampUsers(200).during(Duration.ofSeconds(30))
                )
            ).protocols(httpProtocol)
             .assertions(
                 global().responseTime().percentile(95.0).lt(500),
                 global().failedRequests().percent().lt(5.0)
             );

            // smoke (default)
            default -> setUp(
                browse.injectOpen(atOnceUsers(1)),
                crud.injectOpen(atOnceUsers(1))
            ).protocols(httpProtocol)
             .assertions(
                 global().responseTime().percentile(95.0).lt(500),
                 global().failedRequests().percent().lt(1.0)
             );
        }
    }
}
