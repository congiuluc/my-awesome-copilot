# Backend Testing (.NET)

## Frameworks

- **xUnit** as the test framework.
- **Moq** for mocking dependencies.
- **FluentAssertions** for readable assertions.
- **Microsoft.AspNetCore.Mvc.Testing** for API integration tests.
- **Bogus** for generating realistic test data.

## Test Structure

```
tests/
  {App}.Core.Tests/           # Unit tests for domain logic and services
  {App}.Infrastructure.Tests/ # Repository tests with test databases
  {App}.Api.Tests/            # API integration tests
```

## Naming Conventions

- Test class: `{ClassUnderTest}Tests` (e.g., `ProductServiceTests`).
- Test method: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `GetById_WhenNotFound_ReturnsNull`).
- Test project namespace mirrors source namespace: `{App}.Core.Tests.Services`.

## Unit Test Pattern (Arrange-Act-Assert)

```csharp
public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _mockRepo;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IRepository<Product>>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _sut = new ProductService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var expected = new Product { Id = "123", Name = "Test Item" };
        _mockRepo.Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("123");
        result.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetByIdAsync("999", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.GetByIdAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }
}
```

## Integration Test Pattern

```csharp
public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with in-memory for testing
                services.AddScoped<IRepository<Product>, InMemoryRepository<Product>>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessWithData()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/products/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
        body.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_Returns400()
    {
        // Arrange
        var request = new { Name = "" }; // Missing required fields

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Theory Tests for Multiple Inputs

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public async Task GetByIdAsync_WhenIdIsInvalid_ThrowsArgumentException(string? id)
{
    // Act
    var act = () => _sut.GetByIdAsync(id!, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>();
}
```

## What to Test

- All service / business logic methods (happy path + edge cases).
- Repository implementations with in-memory or test databases.
- API endpoints: status codes, response envelope, validation errors.
- Edge cases: null inputs, empty collections, concurrent access, cancellation.
- Error handling: exceptions translated to correct ApiResponse errors.
- Validation rules: all boundary conditions.

## Rules

- Tests must be **deterministic** — no reliance on external services, time, or randomness.
- Mock external dependencies at boundaries, not internal logic.
- Use `CancellationToken.None` explicitly in test method calls.
- Keep tests fast: unit < 100ms, integration < 2s.
- One assertion concept per test (FluentAssertions chains count as one concept).
- Use `_sut` (system under test) for the class being tested.
