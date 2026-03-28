# Input Security

## Backend Validation

### Never Trust Client Input

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

        RuleFor(x => x.DrawDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Draw date must be in the future");
    }
}
```

### SQL Injection Prevention

- **Always use parameterized queries** — never concatenate user input into SQL.
- EF Core handles parameterization automatically.
- For raw SQL, use `FromSqlInterpolated` or `SqlParameter`.

```csharp
// ✅ Safe — EF Core parameterizes automatically
var results = await dbContext.Products
    .Where(l => l.Name == searchTerm)
    .ToListAsync(cancellationToken);

// ✅ Safe — parameterized raw SQL
var results = await dbContext.Products
    .FromSqlInterpolated($"SELECT * FROM Products WHERE Name = {searchTerm}")
    .ToListAsync(cancellationToken);

// ❌ NEVER — SQL injection vulnerability
var results = await dbContext.Products
    .FromSqlRaw($"SELECT * FROM Products WHERE Name = '{searchTerm}'")
    .ToListAsync(cancellationToken);
```

### Cosmos DB Injection Prevention

- Use parameterized `QueryDefinition`:

```csharp
// ✅ Safe
var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @name")
    .WithParameter("@name", searchTerm);

// ❌ NEVER
var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = '{searchTerm}'");
```

## Frontend Validation

### XSS Prevention

- React auto-escapes JSX output — this is your primary defense.
- **Never use `dangerouslySetInnerHTML`** unless content is sanitized with DOMPurify.
- Validate and sanitize all user input before submission.
- Use `encodeURIComponent()` for URL parameters.

```tsx
// ✅ Safe — React escapes automatically
<p>{userInput}</p>

// ❌ XSS risk — only if absolutely necessary, sanitize first
<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(htmlContent) }} />
```

### File Upload Validation

- Validate file type, size, and content on **both** client and server.
- Never rely solely on `Content-Type` header — verify magic bytes.
- Set maximum file size limits.
- Store uploaded files outside the webroot.

```tsx
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

const validateFile = (file: File): string | null => {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Only JPEG, PNG, and WebP images are allowed';
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'File size must be under 5MB';
  }
  return null;
};
```

## URL Construction

- Always use `encodeURIComponent` for user-provided path/query values.
- Use `URL` constructor for building URLs.

```tsx
// ✅ Safe
const url = new URL(`/api/products/${encodeURIComponent(id)}`, window.location.origin);
url.searchParams.set('q', searchTerm);
```
