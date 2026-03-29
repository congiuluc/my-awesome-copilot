---
name: agent-framework-python
description: "Build AI agents and multi-agent workflows in Python using Microsoft Agent Framework. Use when: creating agents, adding function tools, multi-turn conversations, agent middleware, graph-based workflows, multi-agent orchestration, streaming responses, MCP tool integration, or deploying agents with Azure OpenAI."
argument-hint: 'Describe the agent, tool, workflow, or pattern to implement.'
---

# Microsoft Agent Framework — Python

## When to Use

- Creating or modifying AI agents using Microsoft Agent Framework in Python
- Adding function tools, MCP tools, or code-interpreter capabilities to agents
- Building multi-turn conversational agents with session state
- Implementing graph-based workflows with executors and edges
- Orchestrating multi-agent patterns (sequential, concurrent, hand-off)
- Adding middleware for logging, security, error handling on agent runs
- Integrating with Azure OpenAI, OpenAI, Anthropic, Ollama, or other providers
- Streaming agent responses
- Migrating from Semantic Kernel or AutoGen to Agent Framework

## Official Documentation

- [Agent Framework Overview](https://learn.microsoft.com/en-us/agent-framework/overview/)
- [Your First Agent](https://learn.microsoft.com/en-us/agent-framework/get-started/your-first-agent)
- [Add Tools](https://learn.microsoft.com/en-us/agent-framework/get-started/add-tools)
- [Multi-Turn Conversations](https://learn.microsoft.com/en-us/agent-framework/get-started/multi-turn)
- [Agents Overview](https://learn.microsoft.com/en-us/agent-framework/agents/)
- [Function Tools](https://learn.microsoft.com/en-us/agent-framework/agents/tools/function-tools)
- [Tool Approval (Human-in-the-loop)](https://learn.microsoft.com/en-us/agent-framework/agents/tools/tool-approval)
- [Agent Middleware](https://learn.microsoft.com/en-us/agent-framework/agents/middleware/)
- [Sessions & State](https://learn.microsoft.com/en-us/agent-framework/agents/conversations/session)
- [Context Providers](https://learn.microsoft.com/en-us/agent-framework/agents/conversations/context-providers)
- [Workflows Overview](https://learn.microsoft.com/en-us/agent-framework/workflows/)
- [Providers](https://learn.microsoft.com/en-us/agent-framework/agents/providers/)
- [Integrations (A2A, AG-UI, Azure Functions, M365)](https://learn.microsoft.com/en-us/agent-framework/integrations/)
- [Migration from Semantic Kernel](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel)
- [Migration from AutoGen](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-autogen)
- [GitHub Repository — Python](https://github.com/microsoft/agent-framework/tree/main/python)
- [PyPI Package](https://pypi.org/project/agent-framework/)

## Installation

```bash
pip install agent-framework --pre
```

This installs all sub-packages. For individual packages, see `python/packages` in the GitHub repo.

## Procedure

1. Identify the capability needed (single agent, tools, multi-turn, workflow, middleware)
2. Follow the patterns in [Python agent guidelines](./references/agent-guidelines.md)
3. Apply the [code style rules](./references/code-style.md)
4. Review the matching sample in `./samples/`
5. Use `@kernel_function` decorator or plain functions to define tools
6. Use agent sessions for multi-turn conversations — never manage raw chat history
7. Use middleware for cross-cutting concerns (logging, security, error handling)
8. For multi-agent patterns, use graph-based Workflows with typed executors and edges
9. Use `async def` for all agent invocations with proper error handling
10. Use `AzureCliCredential` during development; prefer `ManagedIdentityCredential` in production
11. Add OpenTelemetry instrumentation for observability
12. Add Google-style docstrings to all public classes and functions
13. Create corresponding tests (see `testing-backend-python` skill)
