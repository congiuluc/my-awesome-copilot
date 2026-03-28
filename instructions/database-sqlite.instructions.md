---
description: "Use when writing or modifying SQLite data access code. Covers WAL mode, pragmas, connection management, indexing, migrations, and performance tuning."
applyTo: "src/MyApp.Infrastructure/**/Sqlite*,src/MyApp.Infrastructure/**/SQLite*"
---
# SQLite Guidelines

## Connection & Pragmas

- Enable **WAL mode** for concurrent read/write performance: `PRAGMA journal_mode=WAL`.
- Set `PRAGMA synchronous=NORMAL` (safe with WAL, better write performance).
- Enable foreign keys: `PRAGMA foreign_keys=ON` (off by default in SQLite).
- Configure pragmas in the connection string or on connection open.

## Schema Design

- Use `STRICT` tables (SQLite 3.37+) for type enforcement.
- SQLite has no native datetime — use `TEXT` with ISO 8601 strings or `DateTimeOffset`.
- Use `INTEGER PRIMARY KEY` for auto-increment rowid alias.
- Keep schemas simple — SQLite is a single-writer database.

## Query Patterns

- **Always use parameterized queries** — SQLite is equally vulnerable to SQL injection.
- Close connections promptly — the provider handles pooling.
- Use `EXPLAIN QUERY PLAN` to verify index usage during development.
- Prefer `EXISTS` over `COUNT(*)` for existence checks.

## Performance

- SQLite supports **one writer at a time** — design for low-concurrency writes.
- Use `BEGIN IMMEDIATE` for write transactions to fail fast on lock contention.
- Batch inserts in a single transaction — auto-commit per statement is very slow.
- Use `CREATE INDEX IF NOT EXISTS` in migrations.

## Migrations

- Track migrations in a version table or use EF Core migrations.
- Test migrations against a copy of production data.
- Test with the same SQLite version as production.
