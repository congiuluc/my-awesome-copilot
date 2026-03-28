---
description: "Use when creating, modifying, or reviewing Dockerfiles, docker-compose files, or container configuration. Covers multi-stage builds, development vs production images, and container best practices."
applyTo: "**/Dockerfile*,**/docker-compose*,.dockerignore"
---
# Docker & Containerization Guidelines

## Image Strategy

- **Development**: Single-stage with hot reload, mounted source volumes.
- **Production**: Multi-stage build — restore → build → publish → runtime.

## .NET Dockerfile (Production)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MyApp.Api/MyApp.Api.csproj", "MyApp.Api/"]
COPY ["src/MyApp.Core/MyApp.Core.csproj", "MyApp.Core/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "MyApp.Infrastructure/"]
RUN dotnet restore "MyApp.Api/MyApp.Api.csproj"
COPY src/ .
RUN dotnet publish "MyApp.Api/MyApp.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

## .NET Dockerfile (Development)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src
COPY . .
RUN dotnet restore
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/MyApp.Api/MyApp.Api.csproj"]
```

## Frontend Dockerfile (Production)

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY src/web-app/package*.json ./
RUN npm ci
COPY src/web-app/ .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

## docker-compose.yml

Use Compose for local development with all services:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/MyApp.Api/Dockerfile.dev
    ports:
      - "5000:8080"
    volumes:
      - ./src:/src
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DatabaseProvider=Sqlite

  web:
    build:
      context: .
      dockerfile: src/web-app/Dockerfile.dev
    ports:
      - "5173:5173"
    volumes:
      - ./src/web-app:/app
      - /app/node_modules
```

## Rules

- Always use specific image tags (e.g., `10.0`, `22-alpine`), never `latest`.
- Run containers as non-root user in production.
- Use `.dockerignore` to exclude `bin/`, `obj/`, `node_modules/`, `.git/`.
- Copy only dependency manifests first, then source — maximize layer caching.
- Never embed secrets in images — use environment variables or mounted secrets.
- Keep production images minimal: use `alpine` variants, multi-stage builds.
