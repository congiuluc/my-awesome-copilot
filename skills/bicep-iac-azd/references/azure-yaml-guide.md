# azure.yaml Guide

Create `azure.yaml` in the project root for Azure Developer CLI.

## Basic Structure

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

## Host Types

| Host | Azure Service | Use For |
|------|---------------|---------|
| `containerapp` | Container Apps | APIs, microservices, workers |
| `appservice` | App Service | Traditional web apps |
| `function` | Azure Functions | Serverless functions |
| `staticwebapp` | Static Web Apps | SPAs, static sites |
| `aks` | AKS | Kubernetes workloads |

## Examples

### .NET API on Container Apps

```yaml
name: myapp

services:
  api:
    project: ./src/api
    language: dotnet
    host: containerapp
    docker:
      path: ./src/api/Dockerfile
```

### React Frontend on Static Web Apps

```yaml
services:
  web:
    project: ./src/web
    language: js
    host: staticwebapp
    dist: dist
```

### Azure Functions

```yaml
services:
  functions:
    project: ./src/functions
    language: dotnet
    host: function
```

### Full-Stack (API + Web)

```yaml
name: myapp

services:
  api:
    project: ./src/api
    language: dotnet
    host: containerapp
    docker:
      path: ./src/api/Dockerfile

  web:
    project: ./src/web
    language: js
    host: staticwebapp
    dist: dist
```

### Pure HTML Static Site (in subfolder)

```yaml
services:
  web:
    project: ./src/web
    host: staticwebapp
    dist: .
```

## Static Web App Notes

- `dist` is **relative to `project`** path.
- When `project: .`, you cannot use `dist: .` — use a distinct output folder.
- Use `language: js` to trigger `npm install && npm run build`.
- `language: html` and `language: static` are **NOT valid** — will fail.

## Custom Docker Context

```yaml
services:
  api:
    project: .
    host: containerapp
    image: api
    docker:
      path: src/api/Dockerfile
      context: src/api
```

> When using `docker` section, omit `language` unless azd needs framework-specific behavior.

## Rules

- One `azure.yaml` per project root.
- Service names must match `azd-service-name` tags in Bicep.
- Omit `infra` section to use Bicep (default). Add `infra.provider: terraform` for Terraform.
- Use `azd env set` for secrets — never put them in `main.parameters.json`.
