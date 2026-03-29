---
name: bicep-iac-azd
description: "Create Bicep IaC and Azure Developer CLI (azd) configurations for Azure deployments. Use when: writing Bicep templates, configuring azure.yaml, setting up azd projects, creating infrastructure modules, deploying to Container Apps, App Service, Functions, or Static Web Apps with azd."
argument-hint: 'Describe the Azure infrastructure to create (e.g., Container App API, Static Web App, Function App, full-stack app, etc.).'
---

# Bicep IaC with Azure Developer CLI (azd)

## When to Use

- Creating Bicep infrastructure templates for Azure resources
- Setting up `azure.yaml` for Azure Developer CLI projects
- Scaffolding `infra/` folder with modular Bicep files
- Deploying to Container Apps, App Service, Functions, or Static Web Apps
- Adding supporting services: Key Vault, Application Insights, Log Analytics
- Configuring managed identities and RBAC role assignments

## Official Documentation

- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/overview)
- [Bicep Language](https://learn.microsoft.com/azure/azure-resource-manager/bicep/overview)
- [azure.yaml Schema](https://learn.microsoft.com/azure/developer/azure-developer-cli/azd-schema)
- [Azure Verified Modules (AVM)](https://azure.github.io/Azure-Verified-Modules/)
- [Resource Naming Rules](https://learn.microsoft.com/azure/azure-resource-manager/management/resource-name-rules)
- [Resource Abbreviations](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations)

## Procedure

1. Determine hosting model from [architecture patterns](./references/architecture-patterns.md)
2. Create `azure.yaml` following the [azure.yaml guide](./references/azure-yaml-guide.md)
3. Scaffold `infra/` folder using [Bicep patterns](./references/bicep-patterns.md)
4. Apply [security hardening](./references/security-hardening.md) rules
5. Review [sample main.bicep](./samples/main.bicep.sample) and [sample azure.yaml](./samples/azure.yaml.sample)
6. Add resource naming with unique suffixes — never hardcode IDs
7. Tag all resources with `azd-env-name` and `azd-service-name`
8. Validate with `azd provision --preview` before deployment
