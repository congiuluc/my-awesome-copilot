// Sample: Backend Caching with IMemoryCache and IDistributedCache
// Shows in-memory caching, distributed caching, and cache invalidation patterns.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MyApp.Infrastructure.Caching;

/// <summary>
/// Caching decorator for IRepository that adds in-memory caching.
/// </summary>
public class CachedProductRepository : IRepository<Product>
{
    private readonly IRepository<Product> _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedProductRepository> _logger;

    private static readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
    private const string AllProductsCacheKey = "products:all";

    public CachedProductRepository(
        IRepository<Product> inner,
        IMemoryCache cache,
        ILogger<CachedProductRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{id}";

        if (_cache.TryGetValue(cacheKey, out Product? cached))
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var product = await _inner.GetByIdAsync(id, cancellationToken);

        if (product is not null)
        {
            _cache.Set(cacheKey, product, _defaultExpiration);
            _logger.LogDebug("Cached {CacheKey}", cacheKey);
        }

        return product;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllProductsCacheKey, out IReadOnlyList<Product>? cached))
        {
            return cached!;
        }

        var products = await _inner.GetAllAsync(cancellationToken);
        _cache.Set(AllProductsCacheKey, products, _defaultExpiration);
        return products;
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddAsync(entity, cancellationToken);
        InvalidateCache();
        return result;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var result = await _inner.UpdateAsync(entity, cancellationToken);
        _cache.Remove($"product:{entity.Id}");
        InvalidateCache();
        return result;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        _cache.Remove($"product:{id}");
        InvalidateCache();
    }

    // Delegate remaining methods to inner repository
    public Task<IReadOnlyList<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        _inner.FindAsync(predicate, cancellationToken);

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default) =>
        _inner.ExistsAsync(id, cancellationToken);

    public Task<int> CountAsync(
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        _inner.CountAsync(predicate, cancellationToken);

    private void InvalidateCache()
    {
        _cache.Remove(AllProductsCacheKey);
        _logger.LogDebug("Invalidated {CacheKey}", AllProductsCacheKey);
    }
}

// --- Response Compression (in Program.cs) ---
// builder.Services.AddResponseCompression(options =>
// {
//     options.EnableForHttps = true;
//     options.Providers.Add<BrotliCompressionProvider>();
//     options.Providers.Add<GzipCompressionProvider>();
// });
// builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
//     options.Level = CompressionLevel.Fastest);
//
// app.UseResponseCompression(); // Before other middleware

// --- DI Registration ---
// services.AddMemoryCache();
// services.AddScoped<IRepository<Product>>(sp =>
//     new CachedProductRepository(
//         sp.GetRequiredService<SqliteProductRepository>(),
//         sp.GetRequiredService<IMemoryCache>(),
//         sp.GetRequiredService<ILogger<CachedProductRepository>>()));
