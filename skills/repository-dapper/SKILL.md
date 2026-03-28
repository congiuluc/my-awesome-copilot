---
name: repository-dapper
description: "Implement data access using Dapper micro-ORM with the IRepository<T> pattern. Covers raw SQL queries, stored procedures, multi-mapping, bulk operations, and DI registration. Use when: creating Dapper repositories, writing raw SQL queries, implementing read-optimized data access, using stored procedures, or building reporting/dashboard queries."
argument-hint: 'Describe the entity or data access operation to implement with Dapper.'
---

# Repository Pattern — Dapper

## When to Use

- Performance-critical read queries (reporting, dashboards, analytics)
- Scenarios requiring raw SQL or stored procedures
- Lightweight read models that don't need change tracking
- Multi-mapping queries joining multiple tables into complex DTOs
- Bulk insert/update operations
- When you need full control over the generated SQL

## When to Use EF Core Instead

- LINQ-based queries with compile-time type safety
- Change tracking and unit-of-work patterns
- Schema migrations with `dotnet ef`
- Complex navigation properties and lazy/eager loading
- Use the `repository-efcore` skill for those cases

## Official Documentation

- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [Dapper — Getting Started](https://www.learndapper.com/)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [SqlClient Documentation](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/)
- [Npgsql Documentation](https://www.npgsql.org/doc/index.html)

## Procedure

1. Define entity in Core with `string Id` property — see [domain models](./references/domain-models.md)
2. If new interface methods needed, extend `IRepository<T>` in Core
3. Create Dapper implementation: [Dapper implementation](./references/dapper-implementation.md)
4. For advanced queries: [advanced patterns](./references/advanced-patterns.md)
5. Register via [DI configuration](./references/di-registration.md)
6. Review [sample repository](./samples/repository-dapper-sample.cs) for complete pattern
7. Write tests for the implementation
8. Manage schema with SQL migration scripts or a migration tool

## Key Principles

- Always use parameterized queries — never concatenate user input into SQL
- Use `using` statements for `IDbConnection` — connections are lightweight and pooled
- Use `CommandType.StoredProcedure` when calling stored procedures
- Prefer `QueryAsync<T>` over `Query<T>` for async operations
- Use `ConfigureAwait(false)` in all async repository methods
- Always accept `CancellationToken` in repository methods (via `CommandDefinition`)
- Use `IReadOnlyList<T>` for collection returns — prevent mutation by callers
- Use `IDbConnectionFactory` for testability — never `new SqlConnection()` directly
