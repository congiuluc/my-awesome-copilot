# EF Core Implementation

## Setup

- Use **Entity Framework Core** with a provider of choice:
  - `Microsoft.EntityFrameworkCore.Sqlite` — local development
  - `Microsoft.EntityFrameworkCore.SqlServer` — production SQL Server
  - `Npgsql.EntityFrameworkCore.PostgreSQL` — production PostgreSQL
  - `Microsoft.EntityFrameworkCore.Cosmos` — Cosmos DB via EF Core provider
- Place in `{App}.Infrastructure/Repositories/EfCore/` (or provider-specific subfolder like `Sqlite/`, `SqlServer/`).
- One `DbContext` shared across repositories (scoped per request).

## DbContext

```csharp
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
```

## Repository

```csharp
/// <summary>
/// EF Core repository implementation.
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
```

## Migrations

```bash
# Create migration
dotnet ef migrations add InitialCreate --project src/{App}.Infrastructure --startup-project src/{App}.Api

# Apply migration
dotnet ef database update --project src/{App}.Infrastructure --startup-project src/{App}.Api
```

## Provider-Specific Registration

### SQLite

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(config.GetConnectionString("DefaultConnection")));
```

### SQL Server

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
```

### PostgreSQL (Npgsql)

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
```

### Cosmos DB (EF Core Provider)

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseCosmos(
        config["CosmosDb:ConnectionString"]!,
        config["CosmosDb:DatabaseName"]!));
```

## Testing with In-Memory SQLite

```csharp
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connection)
    .Options;
using var context = new AppDbContext(options);
context.Database.EnsureCreated();
```

## Performance Best Practices

- Use `AsNoTracking()` for all read-only queries.
- Use `AsSplitQuery()` when loading multiple collections to avoid cartesian explosion.
- Prefer `FirstOrDefaultAsync` over `SingleOrDefaultAsync` when uniqueness is guaranteed by a key.
- Use compiled queries for hot-path operations.
- Index frequently filtered/sorted columns in `OnModelCreating`.
- Profile generated SQL with `Microsoft.EntityFrameworkCore.Diagnostics` or logging.

## Official References

- [EF Core — Getting Started](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [EF Core Performance Tips](https://learn.microsoft.com/en-us/ef/core/performance/)
- [EF Core — Complex Query Operators](https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators)
