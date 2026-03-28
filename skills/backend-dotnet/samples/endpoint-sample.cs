// Sample: Minimal API Endpoint Group with Clean Architecture
// This shows the complete pattern for a feature endpoint group.

using Microsoft.AspNetCore.Http;

namespace MyApp.Api.Endpoints;

/// <summary>
/// Maps product-related API endpoints.
/// </summary>
public static class ProductEndpoints
{
    #region Public Methods

    /// <summary>
    /// Registers all product endpoints under /api/product.
    /// </summary>
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product")
            .WithTags("Product")
            .WithOpenApi();

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllProducts")
            .WithSummary("Get all products")
            .Produces<ApiResponse<IReadOnlyList<ProductDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .Accepts<CreateProductRequest>("application/json")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id}", UpdateAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", DeleteAsync)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return group;
    }

    #endregion

    #region Private Methods

    private static async Task<IResult> GetAllAsync(
        IProductService service,
        CancellationToken cancellationToken)
    {
        var products = await service.GetAllAsync(cancellationToken);
        return Results.Ok(new ApiResponse<IReadOnlyList<ProductDto>>(true, products, null));
    }

    private static async Task<IResult> GetByIdAsync(
        string id,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var product = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(new ApiResponse<ProductDto>(true, product, null));
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var product = await service.CreateAsync(request, cancellationToken);
        return Results.Created(
            $"/api/product/{product.Id}",
            new ApiResponse<ProductDto>(true, product, null));
    }

    private static async Task<IResult> UpdateAsync(
        string id,
        UpdateProductRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var product = await service.UpdateAsync(id, request, cancellationToken);
        return Results.Ok(new ApiResponse<ProductDto>(true, product, null));
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        IProductService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    #endregion
}

// --- DTOs (typically in Core/DTOs/) ---

/// <summary>
/// Product data returned to API consumers.
/// </summary>
public record ProductDto(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    DateTime CreatedAtUtc);

/// <summary>
/// Request to create a new product.
/// </summary>
public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price);

/// <summary>
/// Request to update an existing product.
/// </summary>
public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price);

// --- Service interface (typically in Core/Interfaces/) ---

/// <summary>
/// Service for product business logic.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(string id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

// --- Service implementation (typically in Core/Services/) ---

/// <summary>
/// Product service implementation with repository pattern.
/// </summary>
public class ProductService : IProductService
{
    #region Fields

    private readonly IRepository<Product> _repository;
    private readonly ILogger<ProductService> _logger;

    #endregion

    public ProductService(IRepository<Product> repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region Public Methods

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(p => p.ToDto()).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<ProductDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return product.ToDto();
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        var created = await _repository.AddAsync(product, cancellationToken);
        _logger.LogInformation("Created product {ProductId}: {ProductName}", created.Id, created.Name);
        return created.ToDto();
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateAsync(
        string id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        var updated = await _repository.UpdateAsync(product, cancellationToken);
        return updated.ToDto();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException(nameof(Product), id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Deleted product {ProductId}", id);
    }

    #endregion
}
