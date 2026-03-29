# Architecture Patterns

Select the right Azure hosting model and map application components to Azure services.

## Hosting Models

| Stack | Best For | Azure Services |
|-------|----------|----------------|
| **Containers** | Docker experience, microservices, complex dependencies | Container Apps, AKS, ACR |
| **Serverless** | Event-driven, variable traffic, cost optimization | Functions, Logic Apps, Event Grid |
| **App Service** | Traditional web apps, PaaS preference | App Service, Static Web Apps |

## Decision Factors

| Factor | Containers | Serverless | App Service |
|--------|:----------:|:----------:|:-----------:|
| Docker experience | ✓✓ | | |
| Event-driven | ✓ | ✓✓ | |
| Variable traffic | | ✓✓ | ✓ |
| Complex dependencies | ✓✓ | | ✓ |
| Long-running processes | ✓✓ | ✓ (Durable) | ✓ |
| Minimal ops overhead | | ✓✓ | ✓ |

## Service Mapping

### Hosting

| Component Type | Primary Service | Alternatives |
|----------------|-----------------|--------------|
| SPA Frontend | Static Web Apps | Blob + CDN |
| SSR Web App | Container Apps | App Service, AKS |
| REST/GraphQL API | Container Apps | App Service, Functions, AKS |
| Background Worker | Container Apps | Functions, AKS |
| Scheduled Task | Functions (Timer) | Container Apps Jobs |
| Event Processor | Functions | Container Apps |

### Data

| Need | Primary | Alternatives |
|------|---------|--------------|
| Relational | Azure SQL | PostgreSQL, MySQL |
| Document | Cosmos DB | MongoDB |
| Cache | Redis Cache | |
| Files | Blob Storage | Files Storage |
| Search | AI Search | |

### Integration

| Need | Service |
|------|---------|
| Message Queue | Service Bus |
| Pub/Sub | Event Grid |
| Streaming | Event Hubs |

### Supporting (Always Include)

| Service | Purpose |
|---------|---------|
| Log Analytics | Centralized logging |
| Application Insights | Monitoring & APM |
| Key Vault | Secrets management |
| Managed Identity | Service-to-service auth |
