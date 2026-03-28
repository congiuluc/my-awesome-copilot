# MCP Installation Guide

## VS Code Workspace Configuration

### Step 1: Create `.vscode/mcp.json`

```jsonc
{
  "servers": {
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "${input:githubToken}"
      }
    }
  }
}
```

### Step 2: Add Input Variables (for secrets)

VS Code prompts for `${input:...}` values on first use. For persistent secrets,
store them in environment variables and reference with `${env:VARIABLE_NAME}`.

### Step 3: Verify Connection

1. Open VS Code Command Palette → `MCP: List Servers`
2. Start the server — check it shows "Running"
3. In Copilot Chat, type `@` and look for the server's tools

## Transport Types

### stdio (Recommended for local)

```jsonc
{
  "servers": {
    "my-server": {
      "command": "node",
      "args": ["path/to/server.js"],
      "env": {}
    }
  }
}
```

### SSE (For remote servers)

```jsonc
{
  "servers": {
    "remote-server": {
      "url": "https://my-mcp-server.example.com/sse",
      "headers": {
        "Authorization": "Bearer ${input:token}"
      }
    }
  }
}
```

## npx-Based Servers (Zero Install)

Most community servers support `npx`:

```jsonc
{
  "command": "npx",
  "args": ["-y", "@package/mcp-server-name"],
  "env": {}
}
```

## Docker-Based Servers

```jsonc
{
  "command": "docker",
  "args": [
    "run", "-i", "--rm",
    "-e", "API_KEY=${env:API_KEY}",
    "ghcr.io/org/mcp-server:latest"
  ]
}
```

## Python-Based Servers

```jsonc
{
  "command": "uvx",
  "args": ["mcp-server-name"],
  "env": {}
}
```

Or with pip:

```jsonc
{
  "command": "python",
  "args": ["-m", "mcp_server_name"],
  "env": {}
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Server won't start | Check `command` path — run manually in terminal first |
| Tools not showing | Restart VS Code, check MCP: List Servers panel |
| Auth errors | Verify env vars / input prompts have correct values |
| Timeout | Increase timeout in server config if supported |
| Permission denied | Check file permissions on server binary |

## Security Best Practices

- **Never** hardcode API keys in `mcp.json` — use `${input:...}` or `${env:...}`
- Review MCP server source code before installing
- Pin versions when possible (avoid `@latest` in production)
- Limit file system access scope for filesystem-type servers
- Monitor MCP server resource usage (CPU, memory, network)
