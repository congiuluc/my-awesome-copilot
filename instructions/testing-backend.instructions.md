---
description: "Use when writing, modifying, or reviewing backend .NET tests. Covers xUnit, Moq, FluentAssertions, WebApplicationFactory integration tests, and backend test conventions."
applyTo: "tests/MyApp.Api.Tests/**,tests/MyApp.Core.Tests/**,tests/MyApp.Infrastructure.Tests/**"
---
# Backend Testing Guidelines (.NET)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.** No exceptions.

## Frameworks

- **xUnit** as the test framework.
- **Moq** for mocking dependencies.
- **FluentAssertions** for readable assertions.
- **Microsoft.AspNetCore.Mvc.Testing** for API integration tests.

## Test Structure

```
tests/
  MyApp.Core.Tests/           # Unit tests for domain logic
  MyApp.Infrastructure.Tests/ # Repository and infra tests
  MyApp.Api.Tests/            # API integration tests
```

## Naming Convention

- Test class: `{ClassUnderTest}Tests` (e.g., `ProductServiceTests`).
- Test method: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `GetById_WhenNotFound_ReturnsNull`).

## Unit Test Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public async Task GetById_WhenItemExists_ReturnsItem()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Product>>();
    mockRepo.Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Product { Id = "123", Name = "Test" });
    var service = new ProductService(mockRepo.Object);

    // Act
    var result = await service.GetByIdAsync("123", CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be("Test");
}
```

## Integration Test Pattern

```csharp
public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndData()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
        body!.Success.Should().BeTrue();
    }
}
```

## What to Test

- All service/business logic methods.
- Repository implementations with in-memory/test databases.
- API endpoints for correct status codes, response shapes, validation errors.
- Edge cases: null inputs, empty collections, concurrent access, cancellation.

## General Rules

- Tests must be **deterministic** — no reliance on external services or time.
- Mock external dependencies at boundaries, not internal logic.
- Prefer `Fact` over `Theory` unless testing multiple inputs for the same logic.
- Keep tests fast: unit tests < 100ms each, integration tests < 2s each.
- Use `CancellationToken.None` explicitly in test method calls.
