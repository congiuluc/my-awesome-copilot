using System.Text;
using System.Text.Json;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace App.LoadTests.Scenarios;

/// <summary>
/// Load test scenarios for Product API endpoints.
/// Run with: dotnet test --filter Category=LoadTest
/// </summary>
[Trait("Category", "LoadTest")]
public class ProductEndpointLoadTest
{
    #region Configuration

    private const string BaseUrl = "https://localhost:5001";
    private const int WarmUpSeconds = 5;

    #endregion

    #region Scenarios

    [Fact]
    public void SmokeTest_GetProducts_ShouldHandleBaselineLoad()
    {
        var scenario = Scenario.Create("smoke_get_products", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/products")
                .WithHeader("Accept", "application/json");

            return await Http.Send(context, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(WarmUpSeconds))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: 5,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("reports/smoke")
            .WithReportFormats(ReportFormat.Html)
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        Assert.True(scenarioStats.Ok.Latency.Percent99 < 500,
            $"p99 latency {scenarioStats.Ok.Latency.Percent99}ms exceeds 500ms");
        Assert.True(scenarioStats.Fail.Request.Percent < 1,
            $"Error rate {scenarioStats.Fail.Request.Percent}% exceeds 1%");
    }

    [Fact]
    public void LoadTest_CrudProducts_ShouldHandleSustainedLoad()
    {
        var scenario = Scenario.Create("crud_products", async context =>
        {
            // CREATE
            var payload = new StringContent(
                JsonSerializer.Serialize(new
                {
                    name = $"Product-{context.ScenarioInfo.ThreadNumber}",
                    price = 29.99m
                }),
                Encoding.UTF8,
                "application/json");

            var createRequest = Http.CreateRequest("POST",
                    $"{BaseUrl}/api/products")
                .WithBody(payload);

            var createResponse = await Http.Send(context, createRequest);
            if (createResponse.IsError) return createResponse;

            var body = await createResponse.Payload.Value.Content
                .ReadAsStringAsync();
            var id = JsonDocument.Parse(body)
                .RootElement
                .GetProperty("data")
                .GetProperty("id")
                .GetString();

            // READ
            var readRequest = Http.CreateRequest("GET",
                $"{BaseUrl}/api/products/{id}");
            var readResponse = await Http.Send(context, readRequest);
            if (readResponse.IsError) return readResponse;

            // DELETE
            var deleteRequest = Http.CreateRequest("DELETE",
                $"{BaseUrl}/api/products/{id}");
            return await Http.Send(context, deleteRequest);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(WarmUpSeconds))
        .WithLoadSimulations(
            Simulation.RampConstant(copies: 10,
                during: TimeSpan.FromSeconds(15)),
            Simulation.KeepConstant(copies: 10,
                during: TimeSpan.FromMinutes(1)),
            Simulation.RampConstant(copies: 0,
                during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("reports/load")
            .WithReportFormats(ReportFormat.Html)
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        Assert.True(scenarioStats.Ok.Latency.Percent99 < 1000,
            $"p99 latency {scenarioStats.Ok.Latency.Percent99}ms exceeds 1000ms");
        Assert.True(scenarioStats.Fail.Request.Percent < 5,
            $"Error rate {scenarioStats.Fail.Request.Percent}% exceeds 5%");
    }

    #endregion
}
