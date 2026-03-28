---
name: error-handling-backend
description: >-
  Implement consistent backend error handling with global exception handlers,
  custom exceptions, and structured error responses. Use when: building
  IExceptionHandler, creating domain exceptions, mapping errors to HTTP status
  codes, or structured error logging with Serilog.
argument-hint: 'Describe the error scenario or exception type to handle.'
---

# Backend Error Handling (.NET)

## When to Use

- Setting up global exception handling middleware
- Creating custom exception types for domain errors
- Implementing the standard `ApiResponse` error envelope
- Mapping exceptions to HTTP status codes
- Logging errors with structured Serilog context

## Official Documentation

- [Error Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [IExceptionHandler](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling#iexceptionhandler)
- [Polly (Resilience)](https://github.com/App-vNext/Polly)
- [Serilog](https://serilog.net/)

## Procedure

1. Set up global `IExceptionHandler` — see [error handling reference](./references/backend-error-handling.md)
2. Review [exception handler sample](./samples/exception-handler-sample.cs)
3. Define custom exceptions in Core: `NotFoundException`, `ValidationException`, `ConflictException`
4. Map exceptions to HTTP status codes in the handler
5. Return `ApiResponse` envelope with error details — never expose stack traces
6. Log errors with structured context via Serilog
7. Use Polly for retry/circuit-breaker on external calls
8. Never swallow exceptions silently
