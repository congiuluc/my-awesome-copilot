---
name: database-sqlserver
description: "Configure and optimize SQL Server for .NET applications. Covers connection management, indexing strategies, query optimization, security hardening, and monitoring. Use when: setting up SQL Server or Azure SQL, designing schemas, optimizing query performance, configuring Always Encrypted, or troubleshooting SQL Server issues."
argument-hint: 'Describe the SQL Server configuration, optimization, or issue to address.'
---

# SQL Server Database Skill

## When to Use

- Setting up SQL Server or Azure SQL Database for production
- Configuring connection strings, pooling, and resilience
- Designing schemas with proper data types, constraints, and indexes
- Optimizing query performance with execution plans
- Implementing security features (TDE, Always Encrypted, row-level security)
- Running EF Core or Dapper against SQL Server
- Monitoring and diagnosing performance issues

## Official Documentation

- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/database/)
- [EF Core SQL Server Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
- [SQL Server Indexing](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/)
- [Query Performance Tuning](https://learn.microsoft.com/en-us/sql/relational-databases/performance/)
- [SQL Server Security](https://learn.microsoft.com/en-us/sql/relational-databases/security/)

## Procedure

1. Configure connection and resilience — see [connection setup](./references/connection-setup.md)
2. Design schema with proper types — see [schema design](./references/schema-design.md)
3. Add indexes and optimize queries — see [performance tuning](./references/performance-tuning.md)
4. Apply security hardening — see [security](./references/security.md)
5. Review [complete sample](./samples/sqlserver-sample.cs)

## Key Principles

- Use `NVARCHAR` for Unicode text, `VARCHAR` for ASCII-only
- Use `DATETIMEOFFSET` for timestamps — stores timezone offset
- Always use parameterized queries — SQL injection is the #1 vulnerability
- Design indexes around your query patterns, not your table structure
- Use `INCLUDE` columns in indexes to create covering indexes
- Configure connection resilience with `EnableRetryOnFailure()` for Azure SQL
- Monitor with Query Store (enabled by default in Azure SQL)
- Use `READ COMMITTED SNAPSHOT` isolation to reduce blocking
