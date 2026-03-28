---
name: api-documentation
description: "Configure Swagger/OpenAPI documentation for .NET Minimal API endpoints. Use when: adding endpoint metadata, configuring Swagger UI, documenting API schemas, adding response types, or setting up OpenAPI generation."
argument-hint: 'Describe the endpoint or schema to document with OpenAPI.'
---

# API Documentation (Swagger/OpenAPI)

## When to Use

- Adding or modifying API endpoint metadata
- Configuring Swagger UI or OpenAPI document generation
- Documenting request/response schemas
- Setting up environment-specific API docs behavior

## Official Documentation

- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview)
- [Swagger UI](https://swagger.io/tools/swagger-ui/)
- [OpenAPI Specification](https://spec.openapis.org/oas/v3.1.0)
- [Scalar API Reference](https://github.com/scalar/scalar)

## Procedure

1. Register OpenAPI in `Program.cs` — see [setup guide](./references/openapi-setup.md)
2. Review [sample endpoint documentation](./samples/documented-endpoint-sample.cs)
3. Annotate every endpoint with `WithName`, `WithSummary`, `Produces<T>`
4. Group endpoints with `.WithTags("Feature")`
5. Ensure all DTOs have XML doc comments for schema descriptions
6. Configure environment-specific behavior (dev: enabled, prod: disabled)
7. Verify API docs render correctly at `/swagger`
