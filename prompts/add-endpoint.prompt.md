---
description: "Scaffold a new backend API endpoint: domain model, DTO, repository interface, repository implementation, service, endpoint with OpenAPI metadata, and DI registration."
agent: "backend-creator"
argument-hint: "Endpoint name and description (e.g., 'GET /api/products - list all products with pagination')"
---
Scaffold a complete backend API endpoint following Clean Architecture:

$ARGUMENTS

Implementation order:
1. Domain model in Core layer
2. DTO(s) in Core layer
3. Repository interface in Core layer
4. Repository implementation in Infrastructure layer
5. Service interface in Core and implementation in Infrastructure
6. API endpoint with full OpenAPI metadata (WithName, WithTags, WithSummary, Produces)
7. DI registration in Program.cs
8. Verify build produces zero errors and zero warnings
9. List all public members that need tests
