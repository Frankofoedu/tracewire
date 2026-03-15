# Tracewire

**Navigate your AI agent's path.**

A developer platform for observing, replaying, and controlling AI agent executions with branching DAG visualization, human-in-the-loop (HITL) support, and side-effect tracking.

## Quick Start

```bash
docker compose up
```

- **API:** http://localhost:5185/swagger
- **Frontend:** http://localhost:5173
- **Health:** http://localhost:5185/v1/health

## Architecture

| Component | Tech | Port |
|-----------|------|------|
| API Server | .NET 9 / ASP.NET Minimal API | 5185 |
| Database | PostgreSQL 16 | 5432 |
| Frontend | React + Vite + React Flow | 5173 |
| Python SDK | httpx + Pydantic | - |
| TypeScript SDK | fetch + TypeScript | - |
| .NET SDK | HttpClient + System.Text.Json | - |

## Project Structure

```
Tracewire/
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ backend/          .NET 9 API + EF Core + PostgreSQL
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ sdk/python/       Python SDK with framework adapters
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ sdk/typescript/   TypeScript SDK with framework adapters
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ sdk/dotnet/       .NET SDK with Semantic Kernel adapter
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ frontend/         React + Vite SPA with DAG visualization
ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬ÂÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ docker-compose.yml
```

## Development

### Backend
```bash
cd backend
dotnet run --project src/Tracewire.Api
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Python SDK
```bash
cd sdk/python
pip install -e ".[dev]"
pytest
```

### TypeScript SDK
```bash
cd sdk/typescript
npm install
npm test
```

### .NET SDK
```bash
cd sdk/dotnet
dotnet test
```

## SDK Usage

### Python ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Instrument an Agent

```python
from tracewire import TracewireClient, trace

client = TracewireClient(base_url="http://localhost:5185", api_key="your-key")

async with trace(client, agent_name="my-agent") as t:
    await t.add_event(event_type="Prompt", payload='{"prompt": "hello"}')
    await t.add_event(event_type="ToolCall", payload='{"tool": "search"}')
```

### Python ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â LangChain Auto-Instrumentation

```python
from tracewire.adapters.langchain import TracewireCallbackHandler

handler = TracewireCallbackHandler(client=client)
chain.invoke({"input": "hello"}, config={"callbacks": [handler]})
```

### TypeScript ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Instrument an Agent

```typescript
import { TracewireClient, trace } from "Tracewire-sdk";

const client = new TracewireClient({
  baseUrl: "http://localhost:5185",
  apiKey: "your-key",
});

await trace(client, "my-agent", async (t) => {
  await t.addEvent({ eventType: "Prompt", payload: '{"prompt": "hello"}' });
  await t.addEvent({ eventType: "ToolCall", payload: '{"tool": "search"}' });
});
```

### .NET ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Instrument an Agent

```csharp
using Tracewire.Sdk;

await using var t = await TracewireTrace.StartAsync("my-agent", apiKey: "your-key");

t.LogEvent(EventType.Prompt, new { role = "user", content = "hello" });
t.LogEvent(EventType.ModelResponse, new { content = response }, latencyMs: 450);
t.RegisterSideEffect("email", new { to = "team@acme.com" });
```
