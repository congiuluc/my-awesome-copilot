// Sample: Dapper Repository Pattern with IRepository<T>
// Shows the complete pattern: interface, Dapper implementation, connection factory, and DI registration.

using System.Data;
using System.Linq.Expressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

// --- Connection Factory (in Infrastructure/Data/) ---

namespace MyApp.Infrastructure.Data;

/// <summary>
/// Factory for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Creates a new database connection.</summary>
    IDbConnection CreateConnection();
}

/// <summary>
/// SQL Server connection factory.
/// </summary>
public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}

// --- Dapper Repository (in Infrastructure/Repositories/Dapper/) ---

namespace MyApp.Infrastructure.Repositories.Dapper;

/// <summary>
/// Dapper repository implementation for Product.
/// </summary>
public class DapperProductRepository : IRepository<Product>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc
            FROM Products WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(command).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc
            FROM Products ORDER BY CreatedAtUtc DESC
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var result = await connection.QueryAsync<Product>(command).ConfigureAwait(false);
        return result.AsList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Dapper does not support LINQ predicates natively.
        // Prefer adding purpose-specific methods (FindByStatusAsync, etc.)
        var allItems = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allItems.Where(predicate.Compile()).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        entity.CreatedAtUtc = DateTime.UtcNow;

        const string sql = """
            INSERT INTO Products (Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc)
            VALUES (@Id, @Name, @Description, @Price, @CreatedAtUtc, @UpdatedAtUtc)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, entity, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;

        const string sql = """
            UPDATE Products
            SET Name = @Name, Description = @Description, Price = @Price, UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, entity, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate is null)
        {
            const string sql = "SELECT COUNT(*) FROM Products";
            using var connection = _connectionFactory.CreateConnection();
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
        }

        var allItems = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allItems.Count(predicate.Compile());
    }
}

// --- DI Registration (in Infrastructure/Extensions/) ---

namespace MyApp.Infrastructure.Extensions;

/// <summary>
/// Registers Dapper repository implementations.
/// </summary>
public static class DapperRepositoryExtensions
{
    public static IServiceCollection AddDapperRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IDbConnectionFactory>(
            new SqlServerConnectionFactory(config.GetConnectionString("DefaultConnection")!));

        services.AddScoped<IRepository<Product>, DapperProductRepository>();

        return services;
    }
}
