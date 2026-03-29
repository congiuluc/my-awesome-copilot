# Bicep Patterns

Common patterns for Bicep infrastructure templates with azd.

## File Structure

```
infra/
├── main.bicep              # Entry point (subscription scope)
├── main.parameters.json    # Parameter values
└── modules/
    ├── container-apps.bicep
    ├── monitoring.bicep
    ├── keyvault.bicep
    └── ...
```

## main.bicep Template

```bicep
targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (used for resource naming)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

var tags = { 'azd-env-name': environmentName }
var resourceSuffix = take(uniqueString(subscription().id, environmentName, location), 6)

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module monitoring './modules/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    location: location
    tags: tags
    resourceSuffix: resourceSuffix
  }
}

module keyVault './modules/keyvault.bicep' = {
  name: 'keyvault'
  scope: rg
  params: {
    location: location
    tags: tags
    resourceSuffix: resourceSuffix
  }
}

// Outputs — UPPERCASE names become azd env vars
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = monitoring.outputs.logAnalyticsWorkspaceId
```

## main.parameters.json

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environmentName": { "value": "${AZURE_ENV_NAME}" },
    "location": { "value": "${AZURE_LOCATION}" }
  }
}
```

## Naming Conventions

```bicep
// General pattern: {abbreviation}-{name}-{uniqueHash}
var resourceSuffix = take(uniqueString(subscription().id, environmentName, location), 6)

// Resources allowing dashes
var appName = 'ca-${environmentName}-${resourceSuffix}'
var kvName = 'kv-${environmentName}-${resourceSuffix}'

// Resources requiring alphanumeric only (e.g., Storage, ACR)
var storageName = 'st${replace(environmentName, '-', '')}${resourceSuffix}'
var acrName = 'cr${replace(environmentName, '-', '')}${resourceSuffix}'
```

> Always check [Resource naming rules](https://learn.microsoft.com/azure/azure-resource-manager/management/resource-name-rules)
> for valid characters, length limits, and uniqueness scope.

**Forbidden:** Hard-coded tenant IDs, subscription IDs, resource group names.

## Required Tags (azd)

| Tag | Apply To | Value |
|-----|----------|-------|
| `azd-env-name` | Resource group | `environmentName` parameter |
| `azd-service-name` | Hosting resources | Service name from `azure.yaml` |

## Common Modules

### Monitoring (Log Analytics + App Insights)

```bicep
param location string
param tags object
param resourceSuffix string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log-${resourceSuffix}'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${resourceSuffix}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

output logAnalyticsWorkspaceId string = logAnalytics.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
```

### Key Vault

```bicep
param location string
param tags object
param resourceSuffix string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-${resourceSuffix}'
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
  }
}

output name string = keyVault.name
output id string = keyVault.id
```

### Container Apps Environment

```bicep
param location string
param tags object
param resourceSuffix string
param logAnalyticsWorkspaceId string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${resourceSuffix}'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(logAnalyticsWorkspaceId, '2022-10-01').customerId
        sharedKey: listKeys(logAnalyticsWorkspaceId, '2022-10-01').primarySharedKey
      }
    }
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
```

### Container App

```bicep
param name string
param location string
param tags object
param containerAppsEnvironmentId string
param containerRegistryName string
param imageName string
param targetPort int = 8080
param env array = []

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: union(tags, { 'azd-service-name': name })
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'http'
      }
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: name
          image: '${containerRegistryName}.azurecr.io/${imageName}:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: env
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output url string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
```

> **Container resources:** CPU must use `json()` wrapper: `cpu: json('0.5')`, memory as string: `memory: '1Gi'`

## AVM Module Selection Order (Preferred)

When Azure Verified Modules (AVM) are available, prefer them in this order:

1. AVM Pattern Modules (AVM+AZD first)
2. AVM Resource Modules
3. AVM Utility Modules

## Recommended Outputs

| Output | When |
|--------|------|
| `AZURE_RESOURCE_GROUP` | Always (required) |
| `AZURE_CONTAINER_REGISTRY_ENDPOINT` | If using containers |
| `AZURE_KEY_VAULT_NAME` | If using secrets |
| `AZURE_LOG_ANALYTICS_WORKSPACE_ID` | If using monitoring |
| `API_URL`, `WEB_URL` | One per service endpoint |
