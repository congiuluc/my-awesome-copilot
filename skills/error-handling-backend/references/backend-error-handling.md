# Backend Error Handling

## Global Exception Handler

```csharp
/// <summary>
/// Global exception handler that returns structured ApiResponse errors.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        // Log with appropriate level
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Client error {StatusCode}: {Message}", statusCode, message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiResponse<object>(false, null, message),
            cancellationToken);

        return true;
    }
}
```

## Registration

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In middleware pipeline (first!)
app.UseExceptionHandler();
```

## Custom Domain Exceptions

Define in `{App}.Core/Exceptions/`:

```csharp
/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, string id)
        : base($"{entityName} with ID '{id}' was not found.") { }
}

/// <summary>
/// Thrown when input validation fails at the domain level.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = new ReadOnlyDictionary<string, string[]>(errors);
    }
}

/// <summary>
/// Thrown when an operation conflicts with existing state.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
```

## Service Usage

```csharp
public async Task<ProductDto> GetByIdAsync(string id, CancellationToken cancellationToken)
{
    var product = await _repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(nameof(Product), id);

    return product.ToDto();
}
```

## Rules

- Never return raw exception messages to clients in production.
- Log full exception details server-side, return sanitized message to client.
- Use semantic HTTP status codes: 400 (validation), 404 (not found), 409 (conflict), 500 (unexpected).
- All errors flow through `ApiResponse<T>` envelope.
- Cancel operations gracefully when `CancellationToken` is triggered.
- Use `ILogger` structured logging — include entity type, ID, and correlation ID.
