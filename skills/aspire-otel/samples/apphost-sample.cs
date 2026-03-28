// Sample: Aspire AppHost Program.cs
// Orchestrates all services and resources for local development.

var builder = DistributedApplication.CreateBuilder(args);

// --- Add infrastructure resources ---

// SQLite (file-based, no container needed)
// For production, use Cosmos DB or PostgreSQL:
// var cosmosDb = builder.AddAzureCosmosDB("cosmos")
//     .AddDatabase("myapp-db");

// --- Add API service ---
var api = builder.AddProject<Projects.MyApp_Api>("api")
    .WithExternalHttpEndpoints();
    // .WithReference(cosmosDb);  // Uncomment for Cosmos DB

// --- Add frontend (Vite dev server) ---
// var web = builder.AddNpmApp("web", "../src/my-app-web")
//     .WithReference(api)
//     .WithHttpEndpoint(port: 5173, targetPort: 5173, env: "PORT")
//     .WithExternalHttpEndpoints();

// --- Build and run ---
builder.Build().Run();

// After running with `dotnet run --project MyApp.AppHost`:
// - Dashboard: https://localhost:18888
// - API: https://localhost:{dynamic-port}
// - All telemetry (traces, metrics, logs) flows to dashboard automatically
