---
name: repository-efcore
description: "Implement data access using Entity Framework Core with the IRepository<T> pattern. Covers DbContext setup, LINQ queries, migrations, change tracking, SQLite/SQL Server/PostgreSQL/Cosmos DB EF providers, and DI registration. Use when: creating EF Core repositories, configuring DbContext, writing migrations, or switching database providers."
argument-hint: 'Describe the entity or data access operation to implement with EF Core.'
---

# Repository Pattern — Entity Framework Core

## When to Use

- Creating or modifying `IRepository<T>` interface or EF Core implementations
- Adding new entity types that need persistence via EF Core
- Configuring EF Core database providers (SQLite, SQL Server, PostgreSQL, Cosmos DB EF provider)
- Writing database migrations
- Registering EF Core repositories in DI
- Need LINQ support, change tracking, or complex relational queries

## When to Use Dapper Instead

- Performance-critical read queries (reporting, dashboards)
- Scenarios requiring raw SQL or stored procedures
- Lightweight read models that don't need change tracking
- Use the `repository-dapper` skill for those cases

## Official Documentation

- [EF Core — Getting Started](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [EF Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [EF Core SQL Server Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
- [EF Core PostgreSQL Provider (Npgsql)](https://www.npgsql.org/efcore/)
- [EF Core Cosmos DB Provider](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/)
- [Azure Cosmos DB .NET SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)

## Procedure

1. Define entity in Core with `string Id` property — see [domain models](./references/domain-models.md)
2. If new interface methods needed, extend `IRepository<T>` in Core
3. Create EF Core implementation: [EF Core implementation](./references/efcore-implementation.md)
4. For Cosmos DB direct SDK access: [Cosmos DB implementation](./references/cosmosdb-implementation.md)
5. Register via [DI configuration switching](./references/di-registration.md)
6. Review [sample repository](./samples/repository-efcore-sample.cs) for complete pattern
7. Write tests for the implementation
8. Run EF Core migrations for schema changes

## Key Principles

- Use `AsNoTracking()` for read-only queries — avoids change tracking overhead
- Use `ConfigureAwait(false)` in all async repository methods
- Always accept `CancellationToken` in repository methods
- Use `IReadOnlyList<T>` for collection returns — prevent mutation by callers
- Keep `DbContext` scoped per request; `CosmosClient` as singleton
- Generate IDs in the repository layer (GUID) if not provided by the caller
- Never expose `DbContext` outside Infrastructure layer
