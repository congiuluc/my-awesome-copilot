// Sample: Fully documented Minimal API endpoint with OpenAPI metadata
// Shows WithName, WithSummary, WithDescription, Produces, Accepts, and grouping.

using Microsoft.AspNetCore.Http;

namespace MyApp.Api.Endpoints;

/// <summary>
/// Maps product-related API endpoints with full OpenAPI documentation.
/// </summary>
public static class DocumentedProductEndpoints
{
    /// <summary>
    /// Registers product endpoints with complete OpenAPI metadata.
    /// </summary>
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product")
            .WithTags("Product")
            .WithOpenApi();

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves all products, ordered by creation date descending. "
                + "Supports optional category filtering via query parameter.")
            .Produces<ApiResponse<IReadOnlyList<ProductDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID")
            .WithDescription("Retrieves a single product by its unique identifier. "
                + "Returns 404 if the product does not exist.")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product entry. The ID is auto-generated. "
                + "Returns the created product with a 201 status.")
            .Accepts<CreateProductRequest>("application/json")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();

        group.MapPut("/{id}", UpdateAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .WithDescription("Updates all fields of an existing product. "
                + "Returns 404 if the product does not exist.")
            .Accepts<UpdateProductRequest>("application/json")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id}", DeleteAsync)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .WithDescription("Permanently deletes a product by its ID. "
                + "Returns 204 on success, 404 if not found.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return group;
    }

    #region Private Methods

    private static async Task<IResult> GetAllAsync(
        IProductService service,
        string? category,
        CancellationToken cancellationToken)
    {
        var products = category is not null
            ? await service.GetByCategoryAsync(category, cancellationToken)
            : await service.GetAllAsync(cancellationToken);

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

// --- OpenAPI Registration in Program.cs ---

// builder.Services.AddOpenApi();
//
// if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
// {
//     app.MapOpenApi();
//     app.UseSwaggerUI(options =>
//     {
//         options.SwaggerEndpoint("/openapi/v1.json", "API v1");
//         options.RoutePrefix = "swagger";
//         options.DocumentTitle = "API Documentation";
//     });
// }
//
// app.MapProductEndpoints();
