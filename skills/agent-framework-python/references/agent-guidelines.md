# Microsoft Agent Framework — Python Guidelines

## Framework & Versions

- Install via `pip install agent-framework --pre` (installs all sub-packages).
- Requires **Python 3.10+**.
- Agent Framework is the successor to **Semantic Kernel** and **AutoGen** — do not mix those SDKs.

## Core Concepts

| Concept | Description |
|---------|-------------|
| Agent | The central abstraction — created via provider client helpers like `.as_agent()`. |
| Session | Conversation state container used across multi-turn runs. |
| Function Tools | Python functions exposed as callable tools for agents. |
| Middleware | Intercepts agent runs and tool calls for cross-cutting concerns. |
| Workflows | Graph-based orchestration connecting executors (agents + functions) via edges. |
| Executors | Processing units in a workflow — agents or deterministic functions. |
| Edges | Define typed message flow and conditional routing between executors. |

## Agent Creation Patterns

### Basic Agent (Azure OpenAI)

```python
import os
import asyncio
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential


async def main():
    agent = AzureOpenAIResponsesClient(
        endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
        deployment_name=os.environ.get("AZURE_OPENAI_RESPONSES_DEPLOYMENT_NAME", "gpt-4o-mini"),
        credential=AzureCliCredential(),
    ).as_agent(
        name="MyAgent",
        instructions="You are a friendly assistant.",
    )

    print(await agent.run("What is the largest city in France?"))


if __name__ == "__main__":
    asyncio.run(main())
```

### Agent with Function Tools

```python
from typing import Annotated


def get_weather(
    location: Annotated[str, "The city or location to get weather for."],
) -> str:
    """Get the current weather for a given location."""
    return f"The weather in {location} is partly cloudy, 18°C with 60% humidity."


agent = client.as_agent(
    name="WeatherAgent",
    instructions="You are a helpful weather assistant.",
    tools=[get_weather],
)
```

### Multi-Turn Conversations

```python
session = await agent.create_session()

first = await agent.run("My name is Alice.", session=session)
second = await agent.run("What is my name?", session=session)
```

### Streaming

```python
async for update in agent.run_streaming("Tell me a fun fact."):
    print(update, end="", flush=True)
print()
```

### Agent-as-Tool (Composition)

```python
weather_agent = client.as_agent(
    name="WeatherAgent",
    instructions="You answer questions about weather.",
    description="An agent that answers weather-related questions.",
    tools=[get_weather],
)

main_agent = client.as_agent(
    name="CoordinatorAgent",
    instructions="You are a helpful assistant.",
    tools=[weather_agent.as_tool()],
)
```

## Authentication

| Scenario | Credential |
|----------|------------|
| Local development | `AzureCliCredential` (run `az login` first) |
| Production | `ManagedIdentityCredential` or specific credential |
| API key (non-Azure) | Pass `api_key` directly to the client |

> **Warning**: `DefaultAzureCredential` is convenient for development but may cause latency and security issues in production. Prefer specific credential types in production.

## Middleware

- **Agent Run Middleware** — intercept all agent runs (input/output).
- **Function Calling Middleware** — intercept tool invocations and results.

Middleware is registered via the agent builder pattern.

## Tool Types

| Type | Description |
|------|-------------|
| Function Tools | Python functions decorated or annotated as tools |
| Tool Approval | Human-in-the-loop approval before tool execution |
| Code Interpreter | Execute code in sandboxed environment |
| File Search | Search through uploaded files |
| Web Search | Search the web |
| Hosted MCP Tools | MCP tools hosted by Microsoft Foundry |
| Local MCP Tools | MCP tools on local/custom servers |

## Workflows

Workflows connect executors and edges into a directed graph:

- **Executors**: Individual processing units (agents or functions).
- **Edges**: Define typed message flow with optional conditions.
- **Events**: Provide observability (lifecycle, executor, custom).
- Supports checkpointing, human-in-the-loop, streaming, and time-travel.

## Environment Variables

| Variable | Description |
|----------|-------------|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI resource endpoint URL |
| `AZURE_OPENAI_RESPONSES_DEPLOYMENT_NAME` | Model deployment name (e.g., `gpt-4o-mini`) |
| `AZURE_OPENAI_API_VERSION` | API version string |
| `AZURE_OPENAI_API_KEY` | API key (alternative to credential-based auth) |
| `OPENAI_API_KEY` | OpenAI API key (non-Azure) |

## Observability

- Built-in **OpenTelemetry** integration for distributed tracing.
- See [Python observability samples](https://github.com/microsoft/agent-framework/tree/main/python/samples/02-agents/observability).

## Common Mistakes

- **Mixing Semantic Kernel / AutoGen APIs** — Agent Framework is the successor; don't combine SDKs.
- **Managing raw chat history** — use agent sessions instead.
- **Blocking I/O in async context** — always use `async def` for agent runs.
- **Using DefaultAzureCredential in production** — use a specific credential.
- **Missing tool docstrings/annotations** — agents rely on descriptions to choose tools.
- **Forgetting `--pre` flag** — the framework is in prerelease; `pip install agent-framework` alone may not find it.
