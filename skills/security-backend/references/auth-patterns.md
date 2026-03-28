# Authentication & Authorization Patterns

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

Never use `AllowAnyOrigin()` with `AllowCredentials()`.

## Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

## Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'");
    await next();
});

// HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

## Secrets Management

### Rules

- **Never** commit secrets to source control.
- Use `appsettings.*.json` for non-sensitive config only.
- Use environment variables for local development secrets.
- Use Azure Key Vault for production secrets.
- Rotate secrets regularly.

### Configuration Priority (highest to lowest)

1. Environment variables
2. Azure Key Vault (via `AddAzureKeyVault`)
3. `appsettings.{Environment}.json`
4. `appsettings.json`

```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}
```

## Dependency Scanning

Add to CI:

```yaml
- name: Security scan
  run: dotnet list package --vulnerable --include-transitive

- name: Frontend audit
  working-directory: src/{frontend-app}
  run: npm audit --audit-level=high
```
