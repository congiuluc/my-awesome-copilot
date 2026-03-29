# Microsoft Agent Framework — .NET Guidelines

## Framework & Versions

- Use the **latest prerelease** NuGet packages from `MicrosoftAgentFramework` profile.
- Target **.NET 10** (latest stable). Use C# 14 features where appropriate.
- Always install `Microsoft.Agents.AI.OpenAI`, `Azure.AI.OpenAI`, and `Azure.Identity`.
- Agent Framework is the successor to **Semantic Kernel** and **AutoGen** — do not mix those SDKs.

## Core Concepts

| Concept | Description |
|---------|-------------|
| `AIAgent` | The central abstraction for an AI agent. Created via extension methods on provider clients. |
| `AgentSession` | Conversation state container used across multi-turn runs. |
| `AIFunction` / `AIFunctionFactory` | Exposes C# methods as callable function tools for agents. |
| Middleware | Intercepts agent runs, function calls, or chat client requests for cross-cutting concerns. |
| Workflows | Graph-based orchestration of multiple executors (agents + functions) connected by edges. |
| Executors | Processing units in a workflow — can be agents or deterministic functions. |
| Edges | Define message flow and conditional routing between executors. |

## Agent Creation Patterns

### Basic Agent (Azure OpenAI)

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;

AIAgent agent = new AzureOpenAIClient(
        new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
        new AzureCliCredential())
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "You are a friendly assistant.",
        name: "MyAgent");
```

### Agent with Function Tools

```csharp
using System.ComponentModel;
using Microsoft.Extensions.AI;

[Description("Get the weather for a given location.")]
static string GetWeather(
    [Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

AIAgent agent = client
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "You are a helpful assistant.",
        tools: [AIFunctionFactory.Create(GetWeather)]);
```

### Agent with Middleware

```csharp
var middlewareAgent = originalAgent
    .AsBuilder()
        .Use(runFunc: LoggingMiddleware, runStreamingFunc: StreamingLoggingMiddleware)
        .Use(FunctionCallMiddleware)
    .Build();
```

### Multi-Turn Conversations

```csharp
AgentSession session = await agent.CreateSessionAsync();

var first = await agent.RunAsync("My name is Alice.", session);
var second = await agent.RunAsync("What is my name?", session);
```

### Streaming

```csharp
await foreach (var update in agent.RunStreamingAsync("Tell me a fun fact."))
{
    Console.Write(update);
}
```

### Agent-as-Tool (Composition)

```csharp
AIAgent weatherAgent = client
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "You answer questions about the weather.",
        name: "WeatherAgent",
        description: "An agent that answers weather questions.",
        tools: [AIFunctionFactory.Create(GetWeather)]);

AIAgent mainAgent = client
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "You are a helpful assistant.",
        tools: [weatherAgent.AsAIFunction()]);
```

## Authentication

| Scenario | Credential |
|----------|------------|
| Local development | `AzureCliCredential` (run `az login` first) |
| Production | `ManagedIdentityCredential` or specific credential |
| API key (non-Azure) | Pass key directly to `OpenAIClient` |

> **Warning**: `DefaultAzureCredential` is convenient for development but may cause latency and security issues in production due to fallback probing. Prefer specific credential types in production.

## Middleware Types

1. **Agent Run Middleware** — intercept all agent runs (input/output).
2. **Agent Run Streaming Middleware** — intercept streaming runs.
3. **Function Calling Middleware** — intercept tool invocations and results.
4. **IChatClient Middleware** — intercept underlying chat client requests.

All middleware types are registered via `AsBuilder().Use(...)`.

## Tool Types

| Type | Description |
|------|-------------|
| Function Tools | Custom C# methods via `AIFunctionFactory.Create()` |
| Tool Approval | Human-in-the-loop approval for tool invocations |
| Code Interpreter | Execute code in sandboxed environment |
| File Search | Search through uploaded files |
| Web Search | Search the web |
| Hosted MCP Tools | MCP tools hosted by Microsoft Foundry |
| Local MCP Tools | MCP tools on local/custom servers |

## Workflows

Workflows connect executors and edges into a directed graph:

- **Executors**: Individual processing units (agents or functions).
- **Edges**: Define message flow with optional conditions.
- **Events**: Provide observability (lifecycle, executor, custom).
- Supports checkpointing, human-in-the-loop, streaming, and time-travel.

## Environment Variables

| Variable | Description |
|----------|-------------|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI resource endpoint URL |
| `AZURE_OPENAI_DEPLOYMENT_NAME` | Model deployment name (e.g., `gpt-4o-mini`) |
| `OPENAI_API_KEY` | OpenAI API key (non-Azure) |

## Observability

- Built-in **OpenTelemetry** integration for distributed tracing.
- See [AgentOpenTelemetry sample](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/AgentOpenTelemetry).

## Common Mistakes

- **Mixing Semantic Kernel / AutoGen APIs** — Agent Framework is the successor; don't combine SDKs.
- **Managing raw chat history** — use `AgentSession` instead.
- **Missing CancellationToken** — always propagate cancellation tokens.
- **Using DefaultAzureCredential in production** — use a specific credential.
- **Forgetting tool descriptions** — agents rely on `[Description]` attributes to choose tools.
