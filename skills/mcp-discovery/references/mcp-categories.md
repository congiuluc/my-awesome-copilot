# MCP Server Categories

## Common Categories for Web Application Projects

### Database & Data Access

| Server | Description | Transport |
|--------|-------------|-----------|
| `@modelcontextprotocol/server-sqlite` | Query SQLite databases | stdio |
| `mcp-server-postgres` | PostgreSQL queries and schema | stdio |
| `mcp-cosmos-db` | Azure Cosmos DB operations | stdio |
| `mcp-server-mongodb` | MongoDB queries and aggregation | stdio |

### Version Control & CI/CD

| Server | Description | Transport |
|--------|-------------|-----------|
| `@modelcontextprotocol/server-github` | GitHub repos, issues, PRs, actions | stdio |
| `mcp-server-gitlab` | GitLab project management | stdio |
| `mcp-server-git` | Local git operations (log, diff, blame) | stdio |

### Search & Knowledge

| Server | Description | Transport |
|--------|-------------|-----------|
| `@modelcontextprotocol/server-brave-search` | Web search via Brave API | stdio |
| `mcp-server-fetch` | Fetch and parse web pages | stdio |
| `@anthropic/mcp-server-memory` | Persistent knowledge base | stdio |

### Cloud & Infrastructure

| Server | Description | Transport |
|--------|-------------|-----------|
| `@azure/mcp` | Azure resource management | stdio |
| `mcp-server-aws` | AWS service management | stdio |
| `mcp-server-docker` | Docker container management | stdio |
| `mcp-server-kubernetes` | K8s cluster operations | stdio |

### File System & Utilities

| Server | Description | Transport |
|--------|-------------|-----------|
| `@modelcontextprotocol/server-filesystem` | File read/write with safety | stdio |
| `@modelcontextprotocol/server-puppeteer` | Browser automation | stdio |
| `mcp-server-playwright` | Playwright browser testing | stdio |

### Monitoring & Observability

| Server | Description | Transport |
|--------|-------------|-----------|
| `mcp-server-prometheus` | Query Prometheus metrics | stdio |
| `mcp-server-grafana` | Grafana dashboard queries | stdio |
| `mcp-server-sentry` | Sentry error tracking | stdio |

## Discovery Sources

| Source | URL | Notes |
|--------|-----|-------|
| Official MCP Servers | https://github.com/modelcontextprotocol/servers | Curated, high quality |
| MCP Hub | https://mcphub.io | Community directory |
| npm Registry | https://www.npmjs.com/search?q=mcp-server | Search npm for `mcp-server` |
| PyPI | https://pypi.org/search/?q=mcp+server | Python-based MCP servers |
| Awesome MCP | https://github.com/punkpeye/awesome-mcp-servers | Community curated list |
| Smithery | https://smithery.ai | MCP server marketplace |
