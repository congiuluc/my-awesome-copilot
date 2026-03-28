// Sample: EF Core Repository Pattern with IRepository<T>
// Shows the complete pattern: interface, EF Core implementation, DbContext, and DI registration.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace MyApp.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Gets an entity by its unique identifier.</summary>
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Finds entities matching a predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity.</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity.</summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by ID.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Checks if an entity exists.</summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Counts entities, optionally filtered.</summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}

// --- Domain model (in Core/Models/) ---

namespace MyApp.Core.Models;

/// <summary>
/// Product domain entity.
/// </summary>
public class Product
{
    /// <summary>Unique identifier for the product.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name of the product.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Price of the product.</summary>
    public decimal Price { get; set; }

    /// <summary>UTC timestamp when the product was created.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>UTC timestamp when the product was last modified.</summary>
    public DateTime? UpdatedAtUtc { get; set; }
}

// --- DbContext (in Infrastructure/Data/) ---

namespace MyApp.Infrastructure.Data;

/// <summary>
/// EF Core database context.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Status);
        });
    }
}

// --- EF Core Repository (in Infrastructure/Repositories/EfCore/) ---

namespace MyApp.Infrastructure.Repositories.EfCore;

/// <summary>
/// EF Core repository implementation for Product.
/// </summary>
public class EfCoreProductRepository : IRepository<Product>
{
    private readonly AppDbContext _dbContext;

    public EfCoreProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        entity.CreatedAtUtc = DateTime.UtcNow;

        _dbContext.Products.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _dbContext.Products.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is not null)
        {
            _dbContext.Products.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AnyAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }
}

// --- DI Registration (in Infrastructure/Extensions/) ---

namespace MyApp.Infrastructure.Extensions;

/// <summary>
/// Registers EF Core repository implementations based on configuration.
/// </summary>
public static class EfCoreRepositoryExtensions
{
    public static IServiceCollection AddEfCoreRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        var provider = config.GetValue<string>("DatabaseProvider") ?? "Sqlite";

        return provider switch
        {
            "SqlServer" => services.AddSqlServerRepositories(config),
            "PostgreSql" => services.AddPostgreSqlRepositories(config),
            _ => services.AddSqliteRepositories(config),
        };
    }

    private static IServiceCollection AddSqliteRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();
        return services;
    }

    private static IServiceCollection AddSqlServerRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();
        return services;
    }

    private static IServiceCollection AddPostgreSqlRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();
        return services;
    }
}
