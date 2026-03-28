# SQL Server Connection Setup

## Connection Strings

```csharp
// Local SQL Server
"Server=localhost;Database=MyApp;Trusted_Connection=True;TrustServerCertificate=True"

// SQL Server with username/password
"Server=myserver.database.windows.net;Database=MyApp;User Id=myuser;Password=***;Encrypt=True"

// Azure SQL with Managed Identity (recommended for Azure)
"Server=myserver.database.windows.net;Database=MyApp;Authentication=Active Directory Default"

// With connection pooling tuning
"Server=localhost;Database=MyApp;Trusted_Connection=True;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30"
```

## EF Core Configuration with Resilience

```csharp
/// <summary>
/// Registers SQL Server with EF Core and connection resilience.
/// </summary>
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
                // Retry on transient failures (Azure SQL recommended)
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                // Command timeout for long-running queries
                sqlOptions.CommandTimeout(30);

                // Split queries for multiple includes
                sqlOptions.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery);

                // Compatibility level
                sqlOptions.UseCompatibilityLevel(160); // SQL Server 2022
            });
    });

    return services;
}
```

## Dapper Connection Factory with Resilience

```csharp
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
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
        // Note: Don't call Open() — Dapper opens/closes automatically
        // Connection pooling handles the actual TCP connections
    }
}
```

## Azure SQL with Managed Identity

```csharp
// Using Azure.Identity for passwordless authentication
services.AddDbContext<AppDbContext>(options =>
{
    var connection = new SqlConnection(
        config.GetConnectionString("DefaultConnection"));

    // For Azure SQL with Entra ID (formerly Azure AD)
    connection.AccessToken = new DefaultAzureCredential()
        .GetToken(new TokenRequestContext(
            new[] { "https://database.windows.net/.default" }))
        .Token;

    options.UseSqlServer(connection);
});
```

## Health Check

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(
        config.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        timeout: TimeSpan.FromSeconds(5),
        tags: new[] { "db", "ready" });
```

## Official References

- [SqlClient Connection Strings](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-string-syntax)
- [EF Core Connection Resilience](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)
- [Azure SQL Passwordless Auth](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-overview)
