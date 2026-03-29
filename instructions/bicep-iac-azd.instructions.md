---
description: "Use when creating, modifying, or reviewing Bicep IaC templates, azure.yaml files, or azd project configuration. Covers infrastructure modules, naming conventions, security hardening, and deployment patterns."
applyTo: "**/infra/**/*.bicep,**/azure.yaml,**/main.parameters.json"
---
# Bicep IaC with Azure Developer CLI Guidelines

## Project Structure

```
project-root/
├── azure.yaml               # azd configuration
├── infra/
│   ├── main.bicep            # Entry point (subscription scope)
│   ├── main.parameters.json  # Parameter values
│   └── modules/
│       ├── monitoring.bicep
│       ├── keyvault.bicep
│       ├── container-apps-env.bicep
│       ├── container-app.bicep
│       ├── container-registry.bicep
│       └── ...
└── src/
    └── <service>/
        └── Dockerfile
```

## azure.yaml Pattern

```yaml
name: <project-name>
metadata:
  template: azd-init

services:
  <service-name>:
    project: <path-to-source>
    language: <python|js|ts|java|dotnet|go>
    host: <containerapp|appservice|function|staticwebapp|aks>
```

## Bicep Entry Point (main.bicep)

- Always set `targetScope = 'subscription'`.
- Accept `environmentName` and `location` as required parameters.
- Create a resource group named `rg-${environmentName}`.
- Use `take(uniqueString(subscription().id, environmentName, location), 6)` for resource suffixes.
- Tag the resource group with `{ 'azd-env-name': environmentName }`.
- Export UPPERCASE outputs for azd env vars: `AZURE_RESOURCE_GROUP`, `AZURE_KEY_VAULT_NAME`, etc.

## Naming Conventions

- Use [resource abbreviations](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations): `ca-`, `kv-`, `log-`, `appi-`, `cae-`, `cr`, `st`.
- Append a unique suffix: `take(uniqueString(...), 6)`.
- For alphanumeric-only resources (Storage, ACR): remove dashes with `replace()`.
- Never hardcode tenant IDs, subscription IDs, or resource group names.

## Module Standards

- Every module must accept: `location` (string), `tags` (object).
- Use `targetScope = 'resourceGroup'` for child modules.
- Output resource `name`, `id`, and any connection strings needed by other modules.

## Security Rules

- Use managed identities — no credentials in code or parameters.
- Store secrets in Key Vault with `enableRbacAuthorization: true`.
- Enforce `httpsOnly: true`, `minTlsVersion: '1.2'`.
- Disable public blob access: `allowBlobPublicAccess: false`.
- Use `guid()` for role assignment names (idempotent).
- Add `principalType: 'ServicePrincipal'` on role assignments for managed identities.

## Container Apps Specifics

- CPU must use `json()` wrapper: `cpu: json('0.5')`.
- Memory as string: `memory: '1Gi'`.
- Tag hosting resources with `{ 'azd-service-name': '<name>' }`.
- Use system-assigned identity for ACR pull via `identity: 'system'` in registry config.

## Deployment

```bash
azd auth login       # Authenticate
azd init             # Initialize (or azd init --from-code for Aspire)
azd up               # Provision + build + deploy
azd provision        # Provision infrastructure only
azd deploy           # Deploy application only
azd env set KEY val  # Set environment variables (use for secrets)
```

## Rules

- Always include Log Analytics + Application Insights for observability.
- Always include Key Vault for secrets management.
- Use latest Azure API versions in Bicep resources.
- Prefer Azure Verified Modules (AVM) when available.
- Never put secrets in `main.parameters.json` — use `azd env set`.
- Validate with `azd provision --preview` before deploying.
