---
name: load-testing-dotnet
description: >-
  Load test .NET APIs using NBomber (C#-native) and k6. Covers scenario design,
  assertions, data feeds, real-time reporting, and CI/CD integration. Use when:
  writing load tests in C# with NBomber, testing .NET Minimal API endpoints under
  load, integrating load tests into .NET test suites, or comparing NBomber vs k6
  for a .NET project.
argument-hint: 'Describe the .NET API endpoint or scenario to load test.'
---

# Load Testing for .NET (NBomber + k6)

## When to Use

- Writing load tests in C# alongside unit/integration tests
- Testing .NET Minimal API endpoints under realistic load
- Integrating load tests into existing xUnit test suites
- Need to debug load test scenarios with Visual Studio
- Already using k6 but need supplementary .NET-native tests

## Official Documentation

- [NBomber Documentation](https://nbomber.com/docs/getting-started/overview)
- [NBomber GitHub](https://github.com/PolarisTeam/NBomber)
- [k6 Documentation](https://grafana.com/docs/k6/latest/) (see `load-testing` skill)

## When to Use NBomber vs k6

| Criteria | NBomber | k6 |
|----------|---------|-----|
| Language | C# | JavaScript |
| Best for | .NET teams, debug in VS | Cross-team, CI/CD gates |
| Protocol | HTTP, WebSocket, gRPC, custom | HTTP, WebSocket, gRPC, browser |
| IDE support | Full VS/Rider debugging | Script-only |
| CI/CD | `dotnet test` | `k6 run` / GitHub Action |
| Metrics | Console, HTML, InfluxDB | Console, JSON, Prometheus |

**Recommendation**: Use k6 for CI/CD pipeline gates (smoke/load). Use NBomber when
you need C#-native scenarios, want to share code with your test suite, or need
to debug scenarios interactively.

## Procedure

1. Add NBomber NuGet packages to the test project
2. Follow [NBomber patterns](./references/nbomber-patterns.md)
3. Review [endpoint load test sample](./samples/EndpointLoadTest.cs)
4. Define scenario with steps, data feeds, and assertions
5. Run locally: `dotnet test --filter Category=LoadTest`
6. For CI/CD pipeline integration, use k6 (see `load-testing` skill)
7. Analyze HTML report and console output

## NuGet Packages

```
dotnet add package NBomber
dotnet add package NBomber.Http
```

## Project Structure

```
tests/
  {App}.LoadTests/
    Scenarios/
      ProductScenarios.cs     # Product API load scenarios
      AuthScenarios.cs        # Authentication flow scenarios
    Helpers/
      HttpClientFactory.cs    # Shared HTTP client setup
      TestDataGenerator.cs    # Test data factories
    {App}.LoadTests.csproj
```
