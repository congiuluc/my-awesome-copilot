---
name: docker-containerization
description: "Create Dockerfiles and docker-compose configurations. Use when: building container images, writing Dockerfiles for .NET or React apps, configuring docker-compose for local development, multi-stage builds, or container optimization."
argument-hint: 'Describe what to containerize (.NET API, React app, full stack, etc.).'
---

# Docker & Containerization

## When to Use

- Creating Dockerfiles for backend (.NET) or frontend (React/Vite)
- Setting up docker-compose for local development
- Optimizing container images for production
- Configuring `.dockerignore`

## Official Documentation

- [Dockerfile Reference](https://docs.docker.com/reference/dockerfile/)
- [Docker Compose](https://docs.docker.com/compose/)
- [.NET Container Images](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Multi-stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [Docker Security Best Practices](https://docs.docker.com/build/building/best-practices/)

## Procedure

1. Choose image strategy: development (hot reload) or production (multi-stage)
2. Follow templates in [Dockerfile patterns](./references/dockerfile-patterns.md)
3. Configure [docker-compose](./references/docker-compose.md) for local dev
4. Review [sample Dockerfile](./samples/Dockerfile.sample) and [sample compose](./samples/docker-compose.sample.yml)
5. Add `.dockerignore` to exclude build artifacts
6. Use specific image tags — never `latest`
7. Run as non-root user in production
8. Test containers build and run correctly
