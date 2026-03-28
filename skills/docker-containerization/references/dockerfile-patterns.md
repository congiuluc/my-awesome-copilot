# Dockerfile Patterns

## .NET Production (Multi-Stage)

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/{App}.Api/{App}.Api.csproj", "{App}.Api/"]
COPY ["src/{App}.Core/{App}.Core.csproj", "{App}.Core/"]
COPY ["src/{App}.Infrastructure/{App}.Infrastructure.csproj", "{App}.Infrastructure/"]
RUN dotnet restore "{App}.Api/{App}.Api.csproj"
COPY src/ .
RUN dotnet publish "{App}.Api/{App}.Api.csproj" \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "{App}.Api.dll"]
```

## .NET Development (Hot Reload)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src
COPY . .
RUN dotnet restore
EXPOSE 8080
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/{App}.Api/{App}.Api.csproj"]
```

## Frontend Production (Multi-Stage)

```dockerfile
# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app
COPY src/{frontend-app}/package*.json ./
RUN npm ci --ignore-scripts
COPY src/{frontend-app}/ .
RUN npm run build

# Stage 2: Serve
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD wget -qO- http://localhost/health || exit 1
```

## Frontend Development (Hot Reload)

```dockerfile
FROM node:22-alpine
WORKDIR /app
COPY src/{frontend-app}/package*.json ./
RUN npm ci
COPY src/{frontend-app}/ .
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
```

## .dockerignore

```
**/bin/
**/obj/
**/node_modules/
**/.git/
**/.vs/
**/.vscode/
**/dist/
**/coverage/
**/*.user
**/*.log
.env
.env.*
```

## Rules

- Always use specific image tags (`10.0`, `22-alpine`) — never `latest`.
- Copy dependency manifests first, then source — maximize layer caching.
- Run as non-root user in production (`USER $APP_UID` or `USER node`).
- Add `HEALTHCHECK` to production images.
- Never embed secrets in images.
- Use `--ignore-scripts` with npm for security.
- Keep production images minimal: use `alpine` variants.
