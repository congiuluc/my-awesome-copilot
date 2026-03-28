# Backend Performance

## Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/json", "text/json"]);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
```

## In-Memory Caching

```csharp
builder.Services.AddMemoryCache();

public class CachedProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IMemoryCache _cache;

    public CachedProductService(IRepository<Product> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync("products:all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);
            return await _repository.GetAllAsync(cancellationToken);
        }) ?? [];
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken)
    {
        var result = await _repository.AddAsync(entity, cancellationToken);
        _cache.Remove("products:all"); // Invalidate cache
        return result;
    }
}
```

## Pagination

Add to the repository interface:

```csharp
public interface IRepository<T> where T : class
{
    // ... existing methods

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

Endpoint:

```csharp
group.MapGet("/", async (
    [AsParameters] PaginationRequest request,
    IProductService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.GetPagedAsync(
        request.Page ?? 1,
        request.PageSize ?? 20,
        cancellationToken);
    return TypedResults.Ok(new ApiResponse<PagedResult<ProductDto>>(true, result, null));
});

public record PaginationRequest(int? Page, int? PageSize);
```

## Database Query Optimization

### SQLite / EF Core

- Use `AsNoTracking()` for read-only queries.
- Select only needed columns with `.Select()`.
- Add indexes for frequently queried columns.
- Use `Include()` only when related data is needed.
- Avoid N+1 queries — eager load or batch.

```csharp
// ✅ Efficient: select only needed columns
var dtos = await _dbContext.Products
    .AsNoTracking()
    .Where(l => l.Status == ProductStatus.Active)
    .OrderByDescending(l => l.CreatedAtUtc)
    .Select(l => new ProductDto(l.Id, l.Name, l.Description, l.DrawDateUtc, l.CreatedAtUtc))
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

### Cosmos DB

- Always use partition key in queries (1 RU for point reads).
- Avoid cross-partition queries.
- Use `MaxItemCount` to limit batch sizes.
- Monitor and optimize RU cost per query.

## Output Caching

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(30)));
    options.AddPolicy("Products", builder =>
        builder.Expire(TimeSpan.FromMinutes(5)).Tag("products"));
});

group.MapGet("/", GetAllAsync).CacheOutput("Products");
```
