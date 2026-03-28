# Docker Compose Configuration

## Local Development

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/{App}.Api/Dockerfile.dev
    ports:
      - "5000:8080"
    volumes:
      - ./src:/src
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - DatabaseProvider=Sqlite
      - ConnectionStrings__DefaultConnection=Data Source=/data/myapp.db
    volumes:
      - sqlite-data:/data
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 5s
      retries: 3

  web:
    build:
      context: .
      dockerfile: src/{frontend-app}/Dockerfile.dev
    ports:
      - "5173:5173"
    volumes:
      - ./src/{frontend-app}/src:/app/src
      - /app/node_modules
    environment:
      - VITE_API_URL=http://localhost:5000/api
    depends_on:
      api:
        condition: service_healthy

volumes:
  sqlite-data:
```

## Production Compose (for staging / demo)

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/{App}.Api/Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    env_file:
      - .env.production
    restart: unless-stopped

  web:
    build:
      context: .
      dockerfile: src/{frontend-app}/Dockerfile
    ports:
      - "80:80"
    depends_on:
      - api
    restart: unless-stopped
```

## Useful Commands

```bash
# Start development stack
docker compose up -d

# Rebuild after code changes
docker compose up -d --build

# View logs
docker compose logs -f api

# Stop and remove volumes
docker compose down -v

# Run backend tests in container
docker compose run --rm api dotnet test
```
