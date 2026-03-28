# Backend Input Validation

## Never Trust Client Input

- Validate **all** incoming data at the API boundary.
- Use `FluentValidation` for complex rules.
- Validate request body, query parameters, route parameters, and headers.
- Return `400 Bad Request` with structured errors — never expose internals.

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .Matches(@"^[\w\s\-\.]+$").WithMessage("Name contains invalid characters");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future");
    }
}
```

## SQL Injection Prevention

- **Always use parameterized queries** — never concatenate user input into SQL.
- EF Core handles parameterization automatically.
- For raw SQL, use `FromSqlInterpolated` or `SqlParameter`.

```csharp
// Safe — EF Core parameterizes automatically
var results = await dbContext.Products
    .Where(p => p.Name == searchTerm)
    .ToListAsync(cancellationToken);

// Safe — parameterized raw SQL
var results = await dbContext.Products
    .FromSqlInterpolated($"SELECT * FROM Products WHERE Name = {searchTerm}")
    .ToListAsync(cancellationToken);

// NEVER — SQL injection vulnerability
var results = await dbContext.Products
    .FromSqlRaw($"SELECT * FROM Products WHERE Name = '{searchTerm}'")
    .ToListAsync(cancellationToken);
```

## Cosmos DB Injection Prevention

- Use parameterized `QueryDefinition`:

```csharp
// Safe
var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @name")
    .WithParameter("@name", searchTerm);

// NEVER
var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = '{searchTerm}'");
```

## Official References

- [OWASP Input Validation](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [EF Core Raw SQL](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
