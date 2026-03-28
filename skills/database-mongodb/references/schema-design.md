# MongoDB Schema Design

## Document Design Principles

### Embed vs Reference

| Pattern | When to Use | Example |
|---------|-------------|---------|
| **Embed** (denormalize) | Data read together, 1:few, bounded arrays | Order → OrderItems |
| **Reference** | Data updated independently, 1:many unbounded, large sub-docs | Order → Customer (by ID) |
| **Hybrid** | Embed summary, reference full detail | Order embeds product name/price, references product catalog |

### Embedding Example

```json
{
    "_id": "order-001",
    "customerId": "cust-123",
    "status": "Shipped",
    "items": [
        { "productId": "prod-1", "name": "Widget", "quantity": 2, "unitPrice": 9.99 },
        { "productId": "prod-2", "name": "Gadget", "quantity": 1, "unitPrice": 24.99 }
    ],
    "shippingAddress": {
        "street": "123 Main St",
        "city": "Springfield",
        "zip": "62704"
    },
    "createdAtUtc": { "$date": "2026-03-28T10:00:00Z" }
}
```

### Reference Example

```json
// orders collection
{ "_id": "order-001", "customerId": "cust-123", "total": 44.97 }

// customers collection
{ "_id": "cust-123", "name": "Jane Doe", "email": "jane@example.com" }
```

## BSON Class Maps

### Using Attributes

```csharp
/// <summary>
/// Order document mapped to MongoDB with BSON attributes.
/// </summary>
public class OrderDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = [];

    [BsonElement("totalAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; set; }
}

public class OrderItem
{
    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("unitPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

### Using Fluent Class Maps (Preferred for Clean Architecture)

```csharp
/// <summary>
/// Registers BSON class maps for all document types.
/// Call once at startup before any collection access.
/// </summary>
public static class BsonMappingConfiguration
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderDocument)))
        {
            BsonClassMap.RegisterClassMap<OrderDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.String));
                cm.GetMemberMap(c => c.Status)
                    .SetSerializer(new EnumSerializer<OrderStatus>(BsonType.String));
                cm.GetMemberMap(c => c.TotalAmount)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderItem)))
        {
            BsonClassMap.RegisterClassMap<OrderItem>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.UnitPrice)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });
        }
    }
}
```

Call at startup:

```csharp
// Program.cs — Before builder.Build()
BsonMappingConfiguration.Register();
```

## Type Discriminator Pattern

Store multiple document types in one collection using `_t` discriminator:

```csharp
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(StandardOrder), typeof(SubscriptionOrder))]
public abstract class BaseOrder
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;
}

[BsonDiscriminator("standard")]
public class StandardOrder : BaseOrder
{
    public List<OrderItem> Items { get; set; } = [];
}

[BsonDiscriminator("subscription")]
public class SubscriptionOrder : BaseOrder
{
    public string PlanId { get; set; } = string.Empty;
    public DateTime NextBillingDate { get; set; }
}
```

## Document Size Limits

- Max BSON document size: **16 MB**
- Keep documents under **100 KB** for optimal performance
- If an array grows unbounded, use the **bucket** or **outlier** pattern
- Use `$slice` to limit array size on updates:

```csharp
var update = Builders<T>.Update
    .Push(x => x.RecentEvents, newEvent)
    .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

// Keep only last 100 events
var options = new UpdateOptions();
var pushEach = Builders<T>.Update.PushEach(
    x => x.RecentEvents, new[] { newEvent }, slice: -100);
```

## Schema Validation

Apply JSON Schema validation at the collection level:

```javascript
db.createCollection("orders", {
   validator: {
      $jsonSchema: {
         bsonType: "object",
         required: ["customerId", "status", "createdAtUtc"],
         properties: {
            customerId: { bsonType: "string" },
            status: { enum: ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"] },
            totalAmount: { bsonType: "decimal" },
            createdAtUtc: { bsonType: "date" }
         }
      }
   },
   validationLevel: "moderate",
   validationAction: "error"
})
```

## Official References

- [Data Model Design](https://www.mongodb.com/docs/manual/core/data-model-design/)
- [BSON Types](https://www.mongodb.com/docs/manual/reference/bson-types/)
- [.NET BSON Serialization](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/)
- [Schema Validation](https://www.mongodb.com/docs/manual/core/schema-validation/)
