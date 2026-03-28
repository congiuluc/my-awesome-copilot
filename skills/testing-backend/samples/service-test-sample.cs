// Sample: Backend Unit Tests with xUnit, Moq, and FluentAssertions
// Shows Arrange-Act-Assert pattern, mocking, Theory tests, and naming conventions.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MyApp.Core.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ProductService"/>.
/// </summary>
public class ProductServiceTests
{
    #region Fields

    private readonly Mock<IRepository<Product>> _mockRepo;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _sut;

    #endregion

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IRepository<Product>>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _sut = new ProductService(_mockRepo.Object, _mockLogger.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var expected = new Product
        {
            Id = "prod-123",
            Name = "Widget",
            Price = 29.99m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _mockRepo
            .Setup(r => r.GetByIdAsync("prod-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("prod-123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("prod-123");
        result.Name.Should().Be("Widget");
        result.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.GetByIdAsync("missing", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*missing*");
    }

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

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        var request = new CreateProductRequest("Widget", "A useful widget", 29.99m);
        _mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                p.Id = "new-id";
                p.CreatedAtUtc = DateTime.UtcNow;
                return p;
            });

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-id");
        result.Name.Should().Be("Widget");
        _mockRepo.Verify(
            r => r.AddAsync(It.Is<Product>(p => p.Name == "Widget"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = "1", Name = "A", Price = 10m },
            new() { Id = "2", Name = "B", Price = 20m }
        };
        _mockRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "A");
        result.Should().Contain(p => p.Name == "B");
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>().AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenExists_DeletesSuccessfully()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.ExistsAsync("prod-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.DeleteAsync("prod-123", CancellationToken.None);

        // Assert
        _mockRepo.Verify(
            r => r.DeleteAsync("prod-123", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.ExistsAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.DeleteAsync("missing", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
