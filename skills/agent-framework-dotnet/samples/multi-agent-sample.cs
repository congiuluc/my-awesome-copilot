// Sample: Multi-agent composition using Agent-as-Tool pattern
// Shows how to compose agents where one agent delegates to specialized sub-agents.

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
/// Demonstrates multi-agent composition where a coordinator agent
/// delegates to specialized sub-agents exposed as function tools.
/// </summary>
public static class MultiAgentSample
{
    #region Public Methods

    /// <summary>
    /// Creates a coordinator agent that delegates to weather and search sub-agents.
    /// </summary>
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
            ?? "gpt-4o-mini";

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());

        // --- 1. Create specialized sub-agents ---
        AIAgent weatherAgent = client
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: "You answer questions about weather conditions.",
                name: "WeatherAgent",
                description: "An agent that answers weather-related questions.",
                tools: [AIFunctionFactory.Create(GetWeather)]);

        AIAgent calculatorAgent = client
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: "You perform mathematical calculations.",
                name: "CalculatorAgent",
                description: "An agent that performs math calculations.",
                tools: [AIFunctionFactory.Create(Calculate)]);

        // --- 2. Create coordinator agent with sub-agents as tools ---
        AIAgent coordinator = client
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: "You are a helpful assistant. Delegate to "
                    + "specialized agents when needed.",
                name: "CoordinatorAgent",
                tools:
                [
                    weatherAgent.AsAIFunction(),
                    calculatorAgent.AsAIFunction()
                ]);

        // --- 3. Run the coordinator ---
        Console.WriteLine(await coordinator.RunAsync(
            "What is the weather in Rome, and what is 42 * 17?",
            cancellationToken: cancellationToken));
    }

    #endregion

    #region Function Tools

    [Description("Get the current weather for a location.")]
    private static string GetWeather(
        [Description("The city or location.")] string location)
    {
        return $"The weather in {location} is sunny, 24°C.";
    }

    [Description("Evaluate a mathematical expression.")]
    private static string Calculate(
        [Description("The math expression to evaluate.")] string expression)
    {
        // In production, use a safe expression evaluator.
        return $"Result of {expression} = 714";
    }

    #endregion
}
