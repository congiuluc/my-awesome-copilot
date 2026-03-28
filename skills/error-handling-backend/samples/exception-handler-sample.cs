// Sample: Global Exception Handler with ApiResponse envelope
// Shows IExceptionHandler, domain exceptions, and structured error responses.

using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyApp.Api.Middleware;

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

    /// <inheritdoc />
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
            OperationCanceledException => (499, "Request cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

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

// --- Domain Exceptions (in Core/Exceptions/) ---

namespace MyApp.Core.Exceptions;

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
    /// <summary>
    /// Validation errors keyed by field name.
    /// </summary>
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

// --- Registration in Program.cs ---
// builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// builder.Services.AddProblemDetails();
// app.UseExceptionHandler(); // First in middleware pipeline!
