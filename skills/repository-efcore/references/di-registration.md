# DI Registration — EF Core Repositories

## Configuration-Based Provider Switching

```csharp
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
            "CosmosDb" => services.AddCosmosDbRepositories(config),
            "SqlServer" => services.AddSqlServerRepositories(config),
            "PostgreSql" => services.AddPostgreSqlRepositories(config),
            _ => services.AddSqliteRepositories(config),
        };
    }

    #region SQLite

    private static IServiceCollection AddSqliteRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();
        // Register additional entity repositories here

        return services;
    }

    #endregion

    #region SQL Server

    private static IServiceCollection AddSqlServerRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();

        return services;
    }

    #endregion

    #region PostgreSQL

    private static IServiceCollection AddPostgreSqlRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Product>, EfCoreProductRepository>();

        return services;
    }

    #endregion

    #region Cosmos DB (Direct SDK)

    private static IServiceCollection AddCosmosDbRepositories(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddOptions<CosmosDbSettings>()
            .BindConfiguration("CosmosDb")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            return new CosmosClient(settings.ConnectionString, new CosmosClientOptions
            {
                ApplicationName = "MyApp",
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        services.AddScoped<IRepository<Product>, CosmosDbProductRepository>();
        // Register additional entity repositories here

        return services;
    }

    #endregion
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

### appsettings.Staging.json (SQL Server / PostgreSQL)

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

### appsettings.Production.json (Cosmos DB)

```json
{
  "DatabaseProvider": "CosmosDb",
  "CosmosDb": {
    "ConnectionString": "",
    "DatabaseName": "myapp",
    "ProductContainerName": "products"
  }
}
```

Note: Connection strings should come from Azure Key Vault or environment variables, never committed to source.

## Usage in Program.cs

```csharp
builder.Services.AddEfCoreRepositories(builder.Configuration);
```
