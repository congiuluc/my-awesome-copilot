---
description: "Use when creating, reviewing, or troubleshooting EF Core database migrations, data seeding, or schema change strategies."
applyTo: "src/MyApp.Infrastructure/Migrations/**,src/MyApp.Infrastructure/Data/**"
---
# Database Migration Guidelines

## Creating Migrations

- Run from solution root with `--project` and `--startup-project` flags.
- Naming: `{Action}{Entity}{Detail}` (e.g., `AddOrderStatus`, `CreateProductsTable`).
- Never reorder or rename existing migration files.

## Review Checklist

Before applying:
- [ ] Generates correct SQL (check with `migrations script`)
- [ ] Preserves existing data (no unintended drops)
- [ ] Down method is correct (rollback works)
- [ ] Indexes on new foreign keys
- [ ] Default values for non-nullable new columns

## Safe Changes

| Change | Strategy |
|--------|----------|
| Add nullable column | Direct add |
| Add non-nullable column | Add with `HasDefaultValue()` |
| Rename column | Add new → migrate data → drop old (multi-step) |
| Drop column | Remove code refs first → then drop |

## Data Seeding

- Use `HasData()` for reference data with deterministic IDs.
- Seeds must be idempotent — safe to run multiple times.
- For dynamic data, use `IDataSeeder` at startup.

## Production Rules

- Never run `dotnet ef database update` directly in production.
- Use idempotent migration scripts or migration bundles.
- Generate SQL scripts in CI for review: `dotnet ef migrations script --idempotent`.
- Never use `EnsureCreated()` — it bypasses migration history.
