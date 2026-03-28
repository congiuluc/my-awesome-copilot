---
description: "Use when writing or modifying MongoDB data access code. Covers MongoClient singleton, BSON mapping, indexing, aggregation pipeline, and typed collection patterns."
applyTo: "src/MyApp.Infrastructure/**/Mongo*,src/MyApp.Infrastructure/**/MongoDb*"
---
# MongoDB Guidelines

## Client Management

- `MongoClient` is thread-safe — register as **singleton**; never create per-request instances.
- Configure `CamelCaseElementNameConvention` globally for consistent BSON field naming.
- Register conventions once at application startup before any database access.

## Data Modeling

- Design for **access patterns** — embed data read together; reference data accessed independently.
- Avoid deeply nested documents (max 3-4 levels).
- Keep documents under 16 MB (MongoDB document size limit).

## Indexing

- **Index every query** — unindexed queries perform full collection scans.
- Use compound indexes for queries filtering on multiple fields.
- Create indexes in ascending/descending order matching your sort patterns.
- Use `CreateIndexModel<T>` with the Builders API.

## Query Patterns

- Use the **typed Builders API** (`Builders<T>.Filter`, `Builders<T>.Update`) — not raw BSON.
- Prefer server-side **aggregation pipeline** over client-side processing.
- Always pass `CancellationToken` to async driver methods.
- Use `Find` with `.Limit()` for paginated reads — never load unbounded cursors.

## BSON Mapping

```csharp
BsonClassMap.RegisterClassMap<OrderDocument>(cm =>
{
    cm.AutoMap();
    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
    cm.MapMember(c => c.Status).SetElementName("status");
});
```

- Map document classes explicitly when auto-mapping is insufficient.
- Use `[BsonElement]`, `[BsonId]`, and `[BsonIgnore]` attributes for simple mappings.

## Write Operations

- Use `UpdateOneAsync` with `$set` for partial updates — avoid replacing entire documents.
- Use `BulkWriteAsync` for batch operations (inserts, updates, deletes).
- Implement optimistic concurrency with a version field when needed.
