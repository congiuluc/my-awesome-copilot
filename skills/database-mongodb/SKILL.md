---
name: database-mongodb
description: "MongoDB database skill — connection management, BSON mapping, indexing, aggregation pipeline, and performance tuning with MongoDB.Driver for .NET."
argument-hint: 'Describe the MongoDB configuration, optimization, or issue.'
---

# MongoDB Database Skill

## When to Use

Apply this skill when working with MongoDB as the data store — connection setup, schema design with BSON mapping, indexing strategies, aggregation pipelines, and query optimization.

## Key Principles

1. **MongoClient is thread-safe** — register as singleton; never create per-request instances
2. **Design for access patterns** — embed data read together; reference data accessed independently
3. **Index every query** — unindexed queries perform collection scans (catastrophic at scale)
4. **Use the aggregation pipeline** — prefer server-side aggregation over client-side processing
5. **Typed collections** — use `IMongoCollection<T>` with BSON class maps for type safety
6. **Camel case convention** — configure `CamelCaseElementNameConvention` globally for JSON compatibility

## Procedure

1. Read [references/connection-setup.md](references/connection-setup.md) for MongoClient configuration, connection strings, and health checks
2. Read [references/schema-design.md](references/schema-design.md) for document design, BSON mapping, class maps, and conventions
3. Read [references/performance-tuning.md](references/performance-tuning.md) for indexing, query optimization, explain plans, and read/write concerns
4. Read [references/aggregation-pipeline.md](references/aggregation-pipeline.md) for pipeline stages, typed builders, and common patterns
5. Review [samples/mongodb-sample.cs](samples/mongodb-sample.cs) for a complete implementation example

## Quick Reference

### NuGet Package

```xml
<PackageReference Include="MongoDB.Driver" Version="3.*" />
```

### Register Singleton

```csharp
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = configuration.GetConnectionString("MongoDB")
        ?? throw new InvalidOperationException("MongoDB connection string is required.");
    return new MongoClient(connectionString);
});
```

### Collection Access

```csharp
var database = client.GetDatabase("MyDatabase");
var collection = database.GetCollection<OrderDocument>("orders");
```

### Basic CRUD

```csharp
// Create
await collection.InsertOneAsync(document, cancellationToken: ct);

// Read by ID
var filter = Builders<OrderDocument>.Filter.Eq(x => x.Id, id);
var document = await collection.Find(filter).FirstOrDefaultAsync(ct);

// Update
var update = Builders<OrderDocument>.Update
    .Set(x => x.Status, "Shipped")
    .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);
await collection.UpdateOneAsync(filter, update, cancellationToken: ct);

// Delete
await collection.DeleteOneAsync(filter, cancellationToken: ct);
```

## Official References

- [MongoDB .NET/C# Driver](https://www.mongodb.com/docs/drivers/csharp/current/)
- [MongoDB Manual](https://www.mongodb.com/docs/manual/)
- [MongoDB Atlas](https://www.mongodb.com/docs/atlas/)
- [BSON Types](https://www.mongodb.com/docs/manual/reference/bson-types/)
