---
description: "Use when configuring MCP servers for VS Code Copilot agent mode. Covers server discovery, evaluation criteria, installation, and security."
applyTo: ".vscode/mcp.json,mcp.json,.vscode/settings.json"
---
# MCP Server Configuration Guidelines

## Discovery

- Search for MCP servers at [mcp.so](https://mcp.so), GitHub, and npm registry.
- Evaluate servers against: maintenance activity, star count, documentation quality, security.
- Prefer official or well-maintained servers over abandoned projects.

## Evaluation Criteria

| Criterion | Minimum Threshold |
|-----------|-------------------|
| Last commit | Within 3 months |
| Stars / downloads | > 50 stars or > 500 weekly downloads |
| Documentation | README with usage examples |
| Security | No known CVEs, scoped permissions |

## Installation

- Configure MCP servers in `.vscode/mcp.json` (workspace) or user settings.
- Use `npx` or `uvx` for stdio-based servers — avoid global installs.
- Pin versions for reproducibility.

```json
{
  "servers": {
    "server-name": {
      "command": "npx",
      "args": ["-y", "@scope/mcp-server@1.2.3"],
      "env": {}
    }
  }
}
```

## Security

- Never store secrets in MCP configuration files — use environment variable references.
- Review server source code before granting filesystem or network access.
- Use the minimum required permissions for each server.
- Prefer servers with input validation on tool parameters.
