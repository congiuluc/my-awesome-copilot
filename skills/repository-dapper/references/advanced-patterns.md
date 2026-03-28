# Advanced Dapper Patterns

## Multi-Mapping (Joins)

Map a single query result to multiple objects:

```csharp
/// <summary>
/// Gets products with their category information.
/// </summary>
public async Task<IReadOnlyList<ProductWithCategory>> GetProductsWithCategoryAsync(
    CancellationToken cancellationToken = default)
{
    const string sql = """
        SELECT p.Id, p.Name, p.Price, p.CreatedAtUtc,
               c.Id, c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        ORDER BY p.CreatedAtUtc DESC
        """;

    using var connection = _connectionFactory.CreateConnection();
    var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

    var result = await connection.QueryAsync<ProductWithCategory, Category, ProductWithCategory>(
        command,
        (product, category) =>
        {
            product.Category = category;
            return product;
        },
        splitOn: "Id")
        .ConfigureAwait(false);

    return result.AsList().AsReadOnly();
}
```

## Stored Procedures

```csharp
/// <summary>
/// Gets products by status using a stored procedure.
/// </summary>
public async Task<IReadOnlyList<Product>> GetByStatusAsync(
    string status,
    CancellationToken cancellationToken = default)
{
    using var connection = _connectionFactory.CreateConnection();
    var command = new CommandDefinition(
        "sp_GetProductsByStatus",
        new { Status = status },
        commandType: CommandType.StoredProcedure,
        cancellationToken: cancellationToken);

    var result = await connection.QueryAsync<Product>(command).ConfigureAwait(false);
    return result.AsList().AsReadOnly();
}
```

## Bulk Operations

```csharp
/// <summary>
/// Inserts multiple products in a single round-trip.
/// </summary>
public async Task BulkInsertAsync(
    IEnumerable<Product> entities,
    CancellationToken cancellationToken = default)
{
    const string sql = """
        INSERT INTO Products (Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc)
        VALUES (@Id, @Name, @Description, @Price, @CreatedAtUtc, @UpdatedAtUtc)
        """;

    using var connection = _connectionFactory.CreateConnection();
    var command = new CommandDefinition(sql, entities, cancellationToken: cancellationToken);
    await connection.ExecuteAsync(command).ConfigureAwait(false);
}
```

## Pagination

```csharp
/// <summary>
/// Gets a page of products with total count.
/// </summary>
public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    const string sql = """
        SELECT COUNT(*) FROM Products;

        SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc
        FROM Products
        ORDER BY CreatedAtUtc DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        """;

    var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };

    using var connection = _connectionFactory.CreateConnection();
    var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

    using var multi = await connection.QueryMultipleAsync(command).ConfigureAwait(false);
    var totalCount = await multi.ReadSingleAsync<int>().ConfigureAwait(false);
    var items = (await multi.ReadAsync<Product>().ConfigureAwait(false)).AsList().AsReadOnly();

    return (items, totalCount);
}
```

## Dynamic SQL with DapperAOT

For compile-time checked queries, consider [Dapper.AOT](https://github.com/DapperLib/DapperAOT):

```csharp
[DapperAot]
public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
{
    const string sql = "SELECT * FROM Products WHERE Id = @id";
    using var connection = _connectionFactory.CreateConnection();
    return await connection.QuerySingleOrDefaultAsync<Product>(
        new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken))
        .ConfigureAwait(false);
}
```

## Transaction Support

```csharp
/// <summary>
/// Transfers a product between categories within a transaction.
/// </summary>
public async Task TransferProductAsync(
    string productId,
    string newCategoryId,
    CancellationToken cancellationToken = default)
{
    using var connection = _connectionFactory.CreateConnection();
    connection.Open();
    using var transaction = connection.BeginTransaction();

    try
    {
        const string updateProduct = "UPDATE Products SET CategoryId = @CategoryId WHERE Id = @Id";
        const string logTransfer = """
            INSERT INTO TransferLog (ProductId, NewCategoryId, TransferredAtUtc)
            VALUES (@ProductId, @NewCategoryId, @TransferredAtUtc)
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(updateProduct,
                new { Id = productId, CategoryId = newCategoryId },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        await connection.ExecuteAsync(
            new CommandDefinition(logTransfer,
                new { ProductId = productId, NewCategoryId = newCategoryId, TransferredAtUtc = DateTime.UtcNow },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

## Official References

- [Dapper Multi-Mapping](https://www.learndapper.com/relationships)
- [Dapper Stored Procedures](https://www.learndapper.com/stored-procedures)
- [Dapper Bulk Operations](https://www.learndapper.com/bulk-operations)
- [Dapper.AOT](https://github.com/DapperLib/DapperAOT)
