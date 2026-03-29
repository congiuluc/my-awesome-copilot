// Sample: AI Agent with function tools, middleware, and multi-turn session
// Demonstrates the complete pattern for a production-ready agent in C#.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace MyApp.Agents;

/// <summary>
/// Configures and runs a weather assistant agent with function tools and middleware.
/// </summary>
public static class WeatherAgentSample
{
    #region Public Methods

    /// <summary>
    /// Creates and runs the weather agent with multi-turn conversation support.
    /// </summary>
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // --- 1. Create the base agent with tools ---
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
            ?? "gpt-4o-mini";

        AIAgent baseAgent = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureCliCredential())
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: "You are a helpful weather assistant. Answer concisely.",
                name: "WeatherAgent",
                tools:
                [
                    AIFunctionFactory.Create(GetWeather),
                    AIFunctionFactory.Create(GetForecast)
                ]);

        // --- 2. Add middleware for logging and error handling ---
        AIAgent agent = baseAgent
            .AsBuilder()
                .Use(runFunc: LogAgentRun)
            .Build();

        // --- 3. Multi-turn conversation with session ---
        AgentSession session = await agent.CreateSessionAsync();

        Console.WriteLine(await agent.RunAsync(
            "What is the weather like in Amsterdam?", session, cancellationToken: cancellationToken));

        Console.WriteLine(await agent.RunAsync(
            "And what about the 3-day forecast?", session, cancellationToken: cancellationToken));

        // --- 4. Streaming example ---
        await foreach (var update in agent.RunStreamingAsync(
            "Give me a quick summary.", session, cancellationToken: cancellationToken))
        {
            Console.Write(update);
        }

        Console.WriteLine();
    }

    #endregion

    #region Function Tools

    /// <summary>
    /// Gets the current weather for a given location.
    /// </summary>
    [Description("Get the current weather for a given location.")]
    private static string GetWeather(
        [Description("The city or location to get weather for.")] string location)
    {
        // In production, call a real weather API here.
        return $"The weather in {location} is partly cloudy, 18°C with 60% humidity.";
    }

    /// <summary>
    /// Gets a multi-day weather forecast for a given location.
    /// </summary>
    [Description("Get a multi-day weather forecast for a location.")]
    private static string GetForecast(
        [Description("The city or location.")] string location,
        [Description("Number of days to forecast (1-7).")] int days = 3)
    {
        return $"{days}-day forecast for {location}: Day 1: 18°C sunny, Day 2: 16°C cloudy, Day 3: 14°C rain.";
    }

    #endregion

    #region Middleware

    /// <summary>
    /// Middleware that logs agent run input/output.
    /// </summary>
    private static async Task<AgentResponse> LogAgentRun(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Agent Run] Input messages: {messages.Count()}");
        var response = await innerAgent.RunAsync(messages, session, options, cancellationToken)
            .ConfigureAwait(false);
        Console.WriteLine($"[Agent Run] Output messages: {response.Messages.Count}");
        return response;
    }

    #endregion
}
