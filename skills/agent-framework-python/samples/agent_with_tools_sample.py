# Sample: AI Agent with function tools, middleware, and multi-turn session
# Demonstrates the complete pattern for a production-ready agent in Python.

import asyncio
import os
from typing import Annotated

from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential


# --- Function Tools ---

def get_weather(
    location: Annotated[str, "The city or location to get weather for."],
) -> str:
    """Get the current weather for a given location."""
    # In production, call a real weather API here.
    return f"The weather in {location} is partly cloudy, 18°C with 60% humidity."


def get_forecast(
    location: Annotated[str, "The city or location."],
    days: Annotated[int, "Number of days to forecast (1-7)."] = 3,
) -> str:
    """Get a multi-day weather forecast for a location."""
    return (
        f"{days}-day forecast for {location}: "
        "Day 1: 18°C sunny, Day 2: 16°C cloudy, Day 3: 14°C rain."
    )


# --- Agent Setup ---

async def main() -> None:
    """Create and run a weather agent with tools and multi-turn conversation."""
    client = AzureOpenAIResponsesClient(
        endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
        deployment_name=os.environ.get(
            "AZURE_OPENAI_RESPONSES_DEPLOYMENT_NAME", "gpt-4o-mini"
        ),
        credential=AzureCliCredential(),
    )

    agent = client.as_agent(
        name="WeatherAgent",
        instructions="You are a helpful weather assistant. Answer concisely.",
        tools=[get_weather, get_forecast],
    )

    # --- Multi-turn conversation with session ---
    session = await agent.create_session()

    response1 = await agent.run(
        "What is the weather like in Amsterdam?", session=session
    )
    print(response1)

    response2 = await agent.run(
        "And what about the 3-day forecast?", session=session
    )
    print(response2)

    # --- Streaming example ---
    async for update in agent.run_streaming(
        "Give me a quick summary.", session=session
    ):
        print(update, end="", flush=True)
    print()


if __name__ == "__main__":
    asyncio.run(main())
