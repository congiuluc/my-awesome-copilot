---
name: database-cosmosdb
description: "Configure and optimize Azure Cosmos DB for .NET applications. Covers partition key design, RU optimization, consistency levels, indexing policies, change feed, and the Cosmos DB .NET SDK. Use when: setting up Cosmos DB, designing containers, optimizing query cost, configuring consistency levels, or implementing change feed processors."
argument-hint: 'Describe the Cosmos DB configuration, optimization, or issue to address.'
---

# Azure Cosmos DB Database Skill

## When to Use

- Setting up Azure Cosmos DB (NoSQL API) for production
- Designing partition key strategies for scalable containers
- Optimizing RU (Request Unit) consumption
- Choosing consistency levels for your workload
- Configuring indexing policies for query patterns
- Using Change Feed for event-driven scenarios
- Running the Cosmos DB .NET SDK or EF Core Cosmos provider

## Official Documentation

- [Azure Cosmos DB Documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/)
- [Cosmos DB .NET SDK v3](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [Partition Key Design](https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview)
- [Request Units (RU)](https://learn.microsoft.com/en-us/azure/cosmos-db/request-units)
- [Indexing Policies](https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy)
- [Consistency Levels](https://learn.microsoft.com/en-us/azure/cosmos-db/consistency-levels)
- [Change Feed](https://learn.microsoft.com/en-us/azure/cosmos-db/change-feed)
- [Cosmos DB Best Practices (.NET)](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/best-practice-dotnet)

## Procedure

1. Design partition key and data model — see [data modeling](./references/data-modeling.md)
2. Configure SDK client properly — see [sdk setup](./references/sdk-setup.md)
3. Optimize queries and indexing — see [performance tuning](./references/performance-tuning.md)
4. Implement change feed if needed — see [change feed](./references/change-feed.md)
5. Review [complete sample](./samples/cosmosdb-sample.cs)

## Key Principles

- **Partition key is the most critical design decision** — choose based on query patterns, not entity structure
- Always specify partition key in point reads — `ReadItemAsync` costs 1 RU
- Cross-partition queries are expensive — design to avoid them
- Use `CosmosClient` as a singleton — it manages connections internally
- Use Direct mode (not Gateway) for lowest latency
- Monitor RU consumption via response headers
- Prefer point reads (`ReadItemAsync`) over queries when possible
- Use hierarchical partition keys for multi-tenant scenarios
- Store related data together (denormalize) — joins don't exist
