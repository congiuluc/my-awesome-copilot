// Sample: SQLite setup with EF Core and Dapper, including pragmas and performance configuration

using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure.Data;

/// <summary>
/// Configures SQLite pragmas for optimal performance.
/// </summary>
public static class SqlitePragmaExtensions
{
    /// <summary>
    /// Applies recommended pragmas for WAL mode, caching, and safety.
    /// </summary>
    public static void ConfigurePragmas(this SqliteConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        var pragmas = new[]
        {
            "PRAGMA journal_mode=WAL;",
            "PRAGMA synchronous=NORMAL;",
            "PRAGMA mmap_size=268435456;",
            "PRAGMA cache_size=-64000;",
            "PRAGMA temp_store=MEMORY;",
            "PRAGMA foreign_keys=ON;",
            "PRAGMA busy_timeout=5000;"
        };

        foreach (var pragma in pragmas)
        {
            using var command = connection.CreateCommand();
            command.CommandText = pragma;
            command.ExecuteNonQuery();
        }
    }
}

// --- EF Core Registration ---

namespace MyApp.Infrastructure.Extensions;

/// <summary>
/// Registers SQLite with EF Core and applies performance pragmas.
/// </summary>
public static class SqliteEfCoreExtensions
{
    public static IServiceCollection AddSqliteEfCore(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? "Data Source=myapp.db";

            var connection = new SqliteConnection(connectionString);
            connection.Open();
            connection.ConfigurePragmas();

            options.UseSqlite(connection);
        });

        return services;
    }
}

// --- Dapper Connection Factory ---

/// <summary>
/// SQLite connection factory with automatic pragma configuration.
/// </summary>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.ConfigurePragmas();
        return connection;
    }
}

// --- Testing with In-Memory SQLite ---

/// <summary>
/// Creates an in-memory SQLite test database with schema.
/// </summary>
public static class SqliteTestHelper
{
    public static (AppDbContext Context, SqliteConnection Connection) CreateTestDatabase()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        connection.ConfigurePragmas();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
