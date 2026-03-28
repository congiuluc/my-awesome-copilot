# DI Registration — Dapper Repositories

## Configuration-Based Provider Switching

```csharp
/// <summary>
/// Registers Dapper repository implementations based on configuration.
/// </summary>
public static class DapperRepositoryExtensions
{
    public static IServiceCollection AddDapperRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        var provider = config.GetValue<string>("DatabaseProvider") ?? "SqlServer";

        // Register the connection factory based on provider
        services.AddSingleton<IDbConnectionFactory>(provider switch
        {
            "PostgreSql" => new PostgreSqlConnectionFactory(
                config.GetConnectionString("DefaultConnection")!),
            "Sqlite" => new SqliteConnectionFactory(
                config.GetConnectionString("DefaultConnection")!),
            _ => new SqlServerConnectionFactory(
                config.GetConnectionString("DefaultConnection")!),
        });

        // Register repositories
        services.AddScoped<IRepository<Product>, DapperProductRepository>();
        // Register additional entity repositories here

        return services;
    }
}
```

## Configuration Files

### appsettings.Development.json (SQLite)

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=myapp.db"
  }
}
```

### appsettings.Production.json (SQL Server)

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

### appsettings.Production.json (PostgreSQL)

```json
{
  "DatabaseProvider": "PostgreSql",
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

Note: Connection strings should come from Azure Key Vault or environment variables, never committed to source.

## Combining EF Core and Dapper

You can use both EF Core (for write operations with change tracking) and Dapper (for read-optimized queries) side by side:

```csharp
/// <summary>
/// Registers both EF Core and Dapper repositories.
/// </summary>
public static class HybridRepositoryExtensions
{
    public static IServiceCollection AddHybridRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        // EF Core for write operations
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Dapper connection factory for read operations
        services.AddSingleton<IDbConnectionFactory>(
            new SqlServerConnectionFactory(config.GetConnectionString("DefaultConnection")!));

        // Write repository (EF Core)
        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();

        // Read-optimized queries (Dapper)
        services.AddScoped<IProductReadRepository, DapperProductReadRepository>();

        return services;
    }
}
```

## Usage in Program.cs

```csharp
// Dapper-only
builder.Services.AddDapperRepositories(builder.Configuration);

// Or hybrid (EF Core writes + Dapper reads)
builder.Services.AddHybridRepositories(builder.Configuration);
```
