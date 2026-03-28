---
description: "Use when implementing backend error handling: global exception handlers, custom exceptions, structured error responses, and Serilog error logging."
applyTo: "src/MyApp.Api/Middleware/**,src/MyApp.Core/Exceptions/**"
---
# Backend Error Handling Guidelines

## Global Exception Handler

- Implement `IExceptionHandler` for centralized error handling.
- Register as the **first middleware** in the pipeline.
- Map custom exceptions to appropriate HTTP status codes.
- **Never expose stack traces** or internal details to clients.

## Custom Exceptions

| Exception | HTTP Status | Use When |
|-----------|-------------|----------|
| `NotFoundException` | 404 | Entity not found by ID |
| `ValidationException` | 400 | Business rule violation |
| `ConflictException` | 409 | Duplicate or state conflict |
| `UnauthorizedException` | 401 | Missing/invalid auth |
| `ForbiddenException` | 403 | Insufficient permissions |

- All custom exceptions inherit from a base `DomainException`.
- Include a machine-readable `ErrorCode` property for client-side handling.

## Error Response Envelope

All errors return the standard `ApiResponse` envelope:

```csharp
return new ApiResponse<object>(false, null, "Resource not found");
```

- Never return raw exception messages or framework error pages.
- Include correlation ID from `Activity.Current?.Id` in error responses.

## Structured Logging

- Log exceptions with Serilog structured context:
  ```csharp
  Log.Error(ex, "Failed to process order {OrderId} for user {UserId}", orderId, userId);
  ```
- Log at `Warning` for expected errors (not found, validation).
- Log at `Error` for unexpected exceptions.
- Never log sensitive data (passwords, tokens, PII).

## Resilience

- Use Polly for retry and circuit-breaker on external service calls.
- Never swallow exceptions silently — always log or rethrow.
- Propagate `CancellationToken` and handle `OperationCanceledException` gracefully.
