# Security Hardening

Secure Azure resources following Zero Trust principles in Bicep templates.

## Principles

1. **Zero Trust** — Never trust, always verify
2. **Least Privilege** — Minimum required permissions
3. **Defense in Depth** — Multiple security layers
4. **Encryption Everywhere** — At rest and in transit

## Checklist

- [ ] Use managed identities — no credentials in code
- [ ] Store secrets in Key Vault — never in parameters or app settings
- [ ] Enable RBAC authorization on Key Vault (`enableRbacAuthorization: true`)
- [ ] Enforce HTTPS only (`httpsOnly: true`)
- [ ] Require TLS 1.2+ (`minTlsVersion: '1.2'`)
- [ ] Disable public blob access (`allowBlobPublicAccess: false`)
- [ ] Enable diagnostic logging on all resources
- [ ] Use latest API versions in Bicep resources
- [ ] Never hardcode tenant IDs, subscription IDs, or secrets

## Managed Identity Pattern

```bicep
// System-assigned identity
resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  identity: { type: 'SystemAssigned' }
  // ...
}

// Grant Key Vault access
resource kvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, app.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User
    )
    principalId: app.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
```

## Key Vault References (Instead of Secrets)

```bicep
// In container app env vars — reference Key Vault via managed identity
env: [
  {
    name: 'ConnectionStrings__Database'
    secretRef: 'db-connection-string'
  }
]
```

## Storage Security

```bicep
resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    defaultToOAuthAuthentication: true
  }
}
```

## Common RBAC Role IDs

| Role | ID |
|------|----|
| Key Vault Secrets User | `4633458b-17de-408a-b874-0445c86b69e6` |
| Storage Blob Data Reader | `2a2b9908-6ea1-4ae2-8e65-a410df84e7d1` |
| Storage Blob Data Contributor | `ba92f5b4-2d11-453d-a403-e96b0029c9fe` |
| AcrPull | `7f951dda-4ed3-4680-a7ca-43fe172d538d` |
| Cosmos DB Data Contributor | `00000000-0000-0000-0000-000000000002` |

## Rules

- Always use `UserAssigned` or `SystemAssigned` managed identity — never service principals with secrets.
- Prefer `enableRbacAuthorization: true` on Key Vault over access policies.
- Use `guid()` function for role assignment names to ensure idempotency.
- Add `principalType: 'ServicePrincipal'` to role assignments for managed identities.
