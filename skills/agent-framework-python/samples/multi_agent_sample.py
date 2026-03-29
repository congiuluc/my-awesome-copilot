# Sample: Multi-agent composition using Agent-as-Tool pattern
# Shows how to compose agents where one agent delegates to specialized sub-agents.

import asyncio
import os
from typing import Annotated

from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential


# --- Function Tools ---

def get_weather(
    location: Annotated[str, "The city or location."],
) -> str:
    """Get the current weather for a location."""
    return f"The weather in {location} is sunny, 24°C."


def calculate(
    expression: Annotated[str, "The math expression to evaluate."],
) -> str:
    """Evaluate a mathematical expression."""
    # In production, use a safe expression evaluator.
    return f"Result of {expression} = 714"


# --- Multi-Agent Composition ---

async def main() -> None:
    """Create a coordinator agent that delegates to sub-agents."""
    client = AzureOpenAIResponsesClient(
        endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
        deployment_name=os.environ.get(
            "AZURE_OPENAI_RESPONSES_DEPLOYMENT_NAME", "gpt-4o-mini"
        ),
        credential=AzureCliCredential(),
    )

    # 1. Create specialized sub-agents
    weather_agent = client.as_agent(
        name="WeatherAgent",
        instructions="You answer questions about weather conditions.",
        description="An agent that answers weather-related questions.",
        tools=[get_weather],
    )

    calculator_agent = client.as_agent(
        name="CalculatorAgent",
        instructions="You perform mathematical calculations.",
        description="An agent that performs math calculations.",
        tools=[calculate],
    )

    # 2. Create coordinator with sub-agents as tools
    coordinator = client.as_agent(
        name="CoordinatorAgent",
        instructions="You are a helpful assistant. Delegate to specialized agents when needed.",
        tools=[
            weather_agent.as_tool(),
            calculator_agent.as_tool(),
        ],
    )

    # 3. Run the coordinator
    result = await coordinator.run(
        "What is the weather in Rome, and what is 42 * 17?"
    )
    print(result)


if __name__ == "__main__":
    asyncio.run(main())
