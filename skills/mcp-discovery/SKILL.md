---
name: mcp-discovery
description: >-
  Discover, evaluate, and install Model Context Protocol (MCP) servers for your
  project. Use when: searching for useful MCP servers, comparing options,
  configuring MCP in VS Code, or adding tool integrations to Copilot agents.
argument-hint: 'Describe the MCP capability you need (e.g., database, GitHub, search).'
---

# MCP Server Discovery & Installation

## When to Use

- Searching for MCP servers that add capabilities to your AI coding workflow
- Evaluating MCP servers for quality, maintenance, and security
- Installing and configuring MCP servers locally in VS Code
- Adding MCP tools to custom Copilot agents
- Troubleshooting MCP server connections

## Official Documentation

- [Model Context Protocol Spec](https://modelcontextprotocol.io/)
- [MCP Servers Directory](https://github.com/modelcontextprotocol/servers)
- [VS Code MCP Configuration](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [MCP Inspector](https://modelcontextprotocol.io/docs/tools/inspector)

## Procedure

1. Identify the capability needed — see [MCP categories](./references/mcp-categories.md)
2. Search registries for matching servers — see [MCP categories](./references/mcp-categories.md) for sources
3. Evaluate server quality (maintenance, stars, security, license)
4. Install and configure — see [installation guide](./references/installation-guide.md)
5. Test the connection with MCP Inspector or VS Code
6. Review [sample configuration](./samples/mcp-config-sample.json)
7. Add to `.vscode/mcp.json` or user settings for the workspace

## Evaluation Checklist

Before installing any MCP server, verify:

- [ ] **Maintained**: Updated within last 6 months
- [ ] **Stars/Usage**: Reasonable community adoption
- [ ] **License**: Compatible with your project (MIT, Apache 2.0, etc.)
- [ ] **Security**: No known vulnerabilities, reviewed source code
- [ ] **Documentation**: Clear setup instructions and tool descriptions
- [ ] **Transport**: Supports stdio or SSE (match your environment)
- [ ] **Dependencies**: Minimal — avoid servers pulling heavy runtimes

## Quick Install (VS Code)

```jsonc
// .vscode/mcp.json
{
  "servers": {
    "server-name": {
      "command": "npx",
      "args": ["-y", "@package/mcp-server"],
      "env": {
        "API_KEY": "${input:apiKey}"
      }
    }
  }
}
```
