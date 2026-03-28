---
name: database-sqlite
description: "Configure and optimize SQLite for .NET applications. Covers connection management, WAL mode, pragmas, indexing, migrations, and performance tuning. Use when: setting up SQLite for local development, configuring connection strings, optimizing query performance, managing schema migrations, or troubleshooting SQLite-specific issues."
argument-hint: 'Describe the SQLite configuration, optimization, or issue to address.'
---

# SQLite Database Skill

## When to Use

- Setting up SQLite for local development or embedded scenarios
- Configuring connection strings and pragmas for optimal performance
- Designing schemas with proper indexing
- Running EF Core or Dapper against SQLite
- Managing migrations and schema changes
- Troubleshooting WAL mode, concurrency, or locking issues

## Official Documentation

- [SQLite Official Docs](https://www.sqlite.org/docs.html)
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
- [EF Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [SQLite WAL Mode](https://www.sqlite.org/wal.html)
- [SQLite Query Planner](https://www.sqlite.org/queryplanner.html)
- [SQLite PRAGMA Statements](https://www.sqlite.org/pragma.html)

## Procedure

1. Configure connection string and pragmas — see [connection setup](./references/connection-setup.md)
2. Design schema with proper types and constraints — see [schema design](./references/schema-design.md)
3. Add indexes for query performance — see [performance tuning](./references/performance-tuning.md)
4. Run migrations — see [migrations](./references/migrations.md)
5. Review [complete sample](./samples/sqlite-sample.cs)

## Key Principles

- Enable WAL mode for concurrent read/write performance
- Use parameterized queries exclusively — SQLite is equally vulnerable to injection
- SQLite is single-writer — design for low-concurrency write scenarios
- Use `STRICT` tables (SQLite 3.37+) for type enforcement
- Always close connections promptly — connection pooling is managed by the provider
- Use `DateTimeOffset` or ISO 8601 strings for dates — SQLite has no native datetime type
- Test with the same SQLite version used in production
