---
name: database-migration
description: "Manage database schema migrations for .NET applications. Covers EF Core migrations, naming conventions, rollback strategies, data seeding, migration testing, and CI/CD pipeline integration. Use when: creating migrations, handling schema changes, seeding data, testing migrations, or configuring migration pipelines."
argument-hint: 'Describe the migration need: new migration, rollback, seed data, or CI pipeline setup.'
---

# Database Migration Management

## When to Use

- Creating a new EF Core migration for schema changes
- Rolling back or reverting a failed migration
- Seeding reference data or test data
- Testing migrations against a copy of production data
- Integrating migrations into CI/CD pipelines
- Switching database providers (SQLite → SQL Server → Cosmos DB)

## Official Documentation

- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Data Seeding](https://learn.microsoft.com/ef/core/modeling/data-seeding)
- [Migration Bundles](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)

## Key Principles

- **Migrations are sequential** — never reorder or rename existing migrations.
- **Forward-only in production** — rollback by creating a new corrective migration.
- **Test before apply** — always test migrations against realistic data.
- **Idempotent seeds** — seed data must be safe to run multiple times.
- **No data loss** — schema changes must preserve existing data or migrate it.

## Procedure

### 1. Creating Migrations

```bash
# From solution root
dotnet ef migrations add AddOrderStatus \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api \
    --context AppDbContext
```

**Naming Convention**: `{Action}{Entity}{Detail}`
- `AddOrderStatus` — adding a new column
- `CreateProductsTable` — creating a new table
- `RenameUserEmailToEmailAddress` — renaming a column
- `AddIndexOnOrderDate` — adding an index

### 2. Migration Review Checklist

Before applying any migration:
- [ ] Does it compile and generate correct SQL?
- [ ] Does it preserve existing data?
- [ ] Is the rollback (Down method) correct?
- [ ] Are indexes added for new foreign keys?
- [ ] Are default values set for non-nullable columns?
- [ ] Has it been tested against a copy of production data?

### 3. Data Seeding

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = "cat-1", Name = "Electronics" },
        new Category { Id = "cat-2", Name = "Clothing" }
    );
}
```

- Use `HasData()` for reference data that rarely changes.
- Use IDs that are deterministic (not auto-generated GUIDs).
- For dynamic seeds, use a `IDataSeeder` service called at startup.

### 4. Safe Schema Changes

| Change | Safe? | Strategy |
|--------|-------|----------|
| Add column (nullable) | ✅ | Direct add |
| Add column (non-nullable) | ⚠️ | Add with default value |
| Rename column | ⚠️ | Add new → migrate data → drop old |
| Drop column | ⚠️ | Remove code refs first → migrate → drop |
| Change column type | ❌ | Add new → migrate data → drop old |
| Drop table | ❌ | Ensure no references → backup → drop |

### 5. CI/CD Integration

```yaml
# In GitHub Actions workflow
- name: Validate EF Migrations
  run: |
    dotnet ef migrations script \
      --project src/MyApp.Infrastructure \
      --startup-project src/MyApp.Api \
      --idempotent \
      --output migrations.sql
```

- Generate idempotent SQL scripts in CI for review.
- Use migration bundles for production deployment.
- Never run `dotnet ef database update` directly in production.

## Anti-Patterns

- ❌ Editing existing migration files after they've been applied
- ❌ Using `EnsureCreated()` in production (bypasses migration history)
- ❌ Running `dotnet ef database update` on production directly
- ❌ Deleting and recreating migrations to "clean up"
- ❌ Ignoring the Down method (makes rollback impossible)
