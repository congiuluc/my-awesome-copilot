// Sample: SQL Server setup with EF Core, Dapper, resilience, and health checks

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyApp.Infrastructure.Extensions;

/// <summary>
/// SQL Server EF Core registration with resilience and performance tuning.
/// </summary>
public static class SqlServerEfCoreExtensions
{
    public static IServiceCollection AddSqlServerEfCore(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.UseQuerySplittingBehavior(
                        QuerySplittingBehavior.SplitQuery);
                });
        });

        return services;
    }
}

/// <summary>
/// SQL Server Dapper connection factory.
/// </summary>
public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}

/// <summary>
/// Dapper repository demonstrating SQL Server-specific patterns.
/// </summary>
public class DapperProductRepository : IRepository<Product>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Gets a page of products using keyset pagination.
    /// </summary>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int pageSize,
        DateTimeOffset? lastCreatedAtUtc = null,
        string? lastId = null,
        CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM Products WHERE Status = 'Active'";

        const string dataSql = """
            SELECT TOP (@PageSize) Id, Name, Description, Price, Status, CreatedAtUtc, UpdatedAtUtc
            FROM Products
            WHERE Status = 'Active'
                AND (@LastCreatedAtUtc IS NULL
                     OR CreatedAtUtc < @LastCreatedAtUtc
                     OR (CreatedAtUtc = @LastCreatedAtUtc AND Id < @LastId))
            ORDER BY CreatedAtUtc DESC, Id DESC
            """;

        using var connection = _connectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<Product>(
            new CommandDefinition(dataSql,
                new { PageSize = pageSize, LastCreatedAtUtc = lastCreatedAtUtc, LastId = lastId },
                cancellationToken: cancellationToken));

        return (items.AsList().AsReadOnly(), count);
    }
}

/// <summary>
/// Health check and DI composition.
/// </summary>
public static class SqlServerHealthExtensions
{
    public static IServiceCollection AddSqlServerWithHealth(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")!;

        services.AddSqlServerEfCore(config);
        services.AddSingleton<IDbConnectionFactory>(new SqlServerConnectionFactory(connectionString));
        services.AddScoped<IRepository<Product>, DapperProductRepository>();

        services.AddHealthChecks()
            .AddSqlServer(
                connectionString,
                name: "sqlserver",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "db", "ready" });

        return services;
    }
}
