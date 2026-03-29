# NBomber Patterns for .NET Load Testing

## Basic HTTP Scenario

```csharp
using NBomber.CSharp;
using NBomber.Http.CSharp;

var scenario = Scenario.Create("get_products", async context =>
{
    var request = Http.CreateRequest("GET", "https://localhost:5001/api/products")
        .WithHeader("Accept", "application/json");

    var response = await Http.Send(context, request);

    return response;
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5))
.WithLoadSimulations(
    Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("reports")
    .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
    .Run();
```

## Authenticated Scenario

```csharp
var scenario = Scenario.Create("authenticated_flow", async context =>
{
    // Step 1: Login
    var loginPayload = new StringContent(
        """{"email":"test@example.com","password":"Test123!"}""",
        Encoding.UTF8, "application/json");

    var loginRequest = Http.CreateRequest("POST", $"{baseUrl}/api/auth/login")
        .WithBody(loginPayload);

    var loginResponse = await Http.Send(context, loginRequest);

    if (!loginResponse.IsError)
    {
        var body = loginResponse.Payload.Value;
        var json = await body.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json)
            .RootElement.GetProperty("token").GetString();

        // Step 2: Access protected resource
        var protectedRequest = Http.CreateRequest("GET",
                $"{baseUrl}/api/products")
            .WithHeader("Authorization", $"Bearer {token}");

        return await Http.Send(context, protectedRequest);
    }

    return loginResponse;
})
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 5, during: TimeSpan.FromMinutes(1))
);
```

## Data Feed Pattern

```csharp
// Feed from an in-memory collection
var productIds = Enumerable.Range(1, 100)
    .Select(i => new { Id = i })
    .ToArray();

var dataFeed = DataFeed.Random(productIds);

var scenario = Scenario.Create("get_product_by_id", async context =>
{
    var item = dataFeed.GetNextItem(context.ScenarioInfo);
    var request = Http.CreateRequest("GET",
        $"{baseUrl}/api/products/{item.Id}");

    return await Http.Send(context, request);
});
```

## Load Simulations Reference

```csharp
// Inject: N requests per interval (open model — rate-based)
Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1),
    during: TimeSpan.FromMinutes(2))

// KeepConstant: N concurrent virtual users (closed model)
Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(2))

// RampConstant: ramp up to N concurrent users
Simulation.RampConstant(copies: 50, during: TimeSpan.FromMinutes(1))

// RampPerSec: ramp injection rate from 0 to N/sec
Simulation.RampPerSec(rate: 100, during: TimeSpan.FromMinutes(1))

// Pause between stages
Simulation.Pause(during: TimeSpan.FromSeconds(10))
```

## Assertions

```csharp
NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("reports")
    .Run();

// Or use in xUnit with assertions on stats
var stats = NBomberRunner
    .RegisterScenarios(scenario)
    .Run();

var scenarioStats = stats.ScenarioStats[0];
// p99 latency under 500ms
Assert.True(scenarioStats.Ok.Latency.Percent99 < 500);
// Error rate under 1%
Assert.True(scenarioStats.Fail.Request.Percent < 1);
```

## CRUD Scenario

```csharp
var scenario = Scenario.Create("crud_products", async context =>
{
    // CREATE
    var payload = new StringContent(
        """{"name":"Test Product","price":19.99}""",
        Encoding.UTF8, "application/json");

    var createRequest = Http.CreateRequest("POST",
            $"{baseUrl}/api/products")
        .WithBody(payload);

    var createResponse = await Http.Send(context, createRequest);
    if (createResponse.IsError) return createResponse;

    var body = await createResponse.Payload.Value.Content
        .ReadAsStringAsync();
    var id = JsonDocument.Parse(body)
        .RootElement.GetProperty("data").GetProperty("id").GetString();

    // READ
    var readRequest = Http.CreateRequest("GET",
        $"{baseUrl}/api/products/{id}");
    var readResponse = await Http.Send(context, readRequest);
    if (readResponse.IsError) return readResponse;

    // DELETE
    var deleteRequest = Http.CreateRequest("DELETE",
        $"{baseUrl}/api/products/{id}");
    return await Http.Send(context, deleteRequest);
})
.WithLoadSimulations(
    Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30))
);
```

## HTML Report & CI/CD

```csharp
NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("reports")
    .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
    .Run();
```

Reports are saved to `reports/` folder. Upload as CI artifacts:

```yaml
- name: Upload load test results
  uses: actions/upload-artifact@v4
  with:
    name: load-test-report
    path: tests/**/reports/
```
