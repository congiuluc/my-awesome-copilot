---
description: "Use when writing or modifying Azure Cosmos DB data access code. Covers partition key design, RU optimization, consistency levels, singleton CosmosClient, and query patterns."
applyTo: "src/MyApp.Infrastructure/**/Cosmos*,src/MyApp.Infrastructure/**/CosmosDb*"
---
# Azure Cosmos DB Guidelines

## Client Management

- Register `CosmosClient` as a **singleton** — it is thread-safe and manages connection pooling.
- Use **Direct mode** (default) for lowest latency — do not switch to Gateway unless behind a proxy.
- Never create `CosmosClient` per-request.

## Partition Key Design

- **Partition key is the most critical design decision** — choose based on query patterns.
- High-cardinality values make the best partition keys (e.g., `userId`, `tenantId`).
- Point reads with partition key cost **1 RU** — always specify the partition key.
- Cross-partition queries are expensive — design to avoid them.
- Use hierarchical partition keys for multi-tenant scenarios.

## Query Optimization

- Prefer **point reads** (`ReadItemAsync`) over queries when you have the ID + partition key.
- Always project only needed fields — avoid `SELECT *`.
- Monitor RU consumption via `RequestCharge` in response headers.
- Use `FeedIterator` for paginated reads — never load unbounded results.

## Data Modeling

- **Denormalize** data — Cosmos DB has no joins.
- Embed data that is read together in the same document.
- Reference data that is updated independently.
- Keep documents under 100 KB for optimal performance.

## Consistency & Indexing

- Default consistency: **Session** — sufficient for most workloads.
- Customize indexing policy: exclude unused paths to save RUs on writes.
- Use composite indexes for `ORDER BY` on multiple properties.

## Change Feed

- Use Change Feed for event-driven processing and materialized views.
- Process changes idempotently — the feed guarantees at-least-once delivery.
