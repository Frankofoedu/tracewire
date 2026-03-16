# Tracewire

**Navigate your AI agent's path.**

AI agents make decisions, call tools, write to memory, and trigger real-world side effects — all in chains that are invisible by default.  Tracewire captures every step as a **branching DAG**, lets you **replay from any point**, and gives humans the power to **approve or reject** actions before they happen.

## Why Tracewire?

**Your agent just sent the wrong email to a customer. Can you tell which LLM call decided to do that?**

Most agent frameworks give you logs. Tracewire gives you a complete, interactive execution graph — every prompt, every model response, every tool call, with cost, latency, and the full payload. Then it lets you do something about it.

### See Everything

Every LLM call, tool invocation, memory write, and error is captured as a structured event in a branching DAG. The interactive visualization shows exactly how your agent made its decisions — which branch it took, what data it saw, and what it did next.

- Full payload capture for every event (prompts, responses, tool I/O)
- Per-step latency and cost tracking
- Color-coded DAG with branch visualization
- Click any node to inspect the complete execution context

### Replay From Any Point

Found a bug at step 7 of a 20-step trace? Click that event, modify the payload, and replay. Tracewire forks into a new branch so you can compare the original path with the modified one — without re-running the entire agent.

- Replay from any event node with modified inputs
- Automatic branch creation for A/B comparison
- **SDK replay callbacks** — register `on_replay` handlers in your agent to react to replays in real-time via SSE
- **Side-effect warnings**: Tracewire scans ancestor events and warns you about irreversible actions (emails sent, database writes, API calls) before you replay
- Workspace-level replay policies (Warn / Block / Require Approval)

### Human-in-the-Loop

Your agent is about to deploy to production. Should it? Tracewire lets agents pause and wait for human approval before executing high-stakes actions.

- SDK calls `pause_for_human()` → agent blocks → reviewer sees approval panel
- **Real-time SSE streaming** — decisions push to the SDK instantly via Server-Sent Events, no polling
- Three decisions: **Approve**, **Reject**, or **Escalate**
- Configurable timeout with fallback behavior (abort, continue, escalate)
- Full audit trail with reviewer comments and timestamps

### Zero-Touch Instrumentation

Drop in an adapter — your agent code stays unchanged. Tracewire captures everything automatically.

| Framework | Language | Adapter |
|-----------|----------|---------|
| LangChain | Python | `TracewireCallbackHandler` |
| AutoGen | Python | `TracewireAutoGenMiddleware` |
| CrewAI | Python | `TracewireCrewCallback` |
| Vercel AI SDK | TypeScript | `wrapLanguageModel()` |
| LangChain.js | TypeScript | `createLangChainCallback()` |
| Semantic Kernel | .NET | `SemanticKernelAdapter` |
| Any LLM client | .NET | `ChatClientAdapter` |

### Multi-Tenant by Default

Organizations → Workspaces → API Keys. Scoped permissions, SHA-256 hashed keys, and workspace-level policies for replay behavior.

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
| Python SDK | httpx + Pydantic | — |
| TypeScript SDK | fetch + TypeScript | — |
| .NET SDK | HttpClient + System.Text.Json | — |

## Project Structure

```
tracewire/
├── backend/          .NET 9 API + EF Core + PostgreSQL
├── sdk/python/       Python SDK with framework adapters
├── sdk/typescript/   TypeScript SDK with framework adapters
├── sdk/dotnet/       .NET SDK with Semantic Kernel adapter
├── frontend/         React + Vite SPA with DAG visualization
└── docker-compose.yml
```

## SDK Examples

### Python

```python
from tracewire import trace

async with trace("my-agent", api_key="your-key") as t:
    t.log_event("Prompt", {"role": "user", "content": "hello"})
    t.log_event("ModelResponse", {"content": response}, latency_ms=450, cost=0.003)
    t.register_side_effect("email", {"to": "team@acme.com"})
```

**LangChain** — zero-touch, pass the callback:

```python
from tracewire.adapters.langchain import TracewireCallbackHandler

handler = TracewireCallbackHandler(client=client)
chain.invoke({"input": "hello"}, config={"callbacks": [handler]})
```

### TypeScript

```typescript
import { trace } from "tracewire-sdk";

await trace("my-agent", async (t) => {
  t.logEvent("Prompt", { prompt: "hello" });
  t.logEvent("ToolCall", { tool: "search", query: "quantum computing" });
}, { apiKey: "your-key" });
```

**Vercel AI SDK** — wrap the model, everything is auto-captured:

```typescript
import { wrapLanguageModel } from "tracewire-sdk/adapters/ai-sdk";

const model = wrapLanguageModel(openai("gpt-4o"), traceContext);
const { text } = await generateText({ model, prompt: "Hello!" });
```

### .NET

```csharp
using Tracewire.Sdk;
using Tracewire.Sdk.Adapters;

await using var t = await TracewireTrace.StartAsync("my-agent", apiKey: "your-key");
var llm = new ChatClientAdapter(t, "gpt-4o");

var response = await llm.CallAsync("hello", async prompt =>
{
    var result = await openAiClient.CompleteChatAsync([new UserChatMessage(prompt)]);
    return result.Value.Content[0].Text;
});
```

### Human-in-the-Loop (any SDK)

```python
decision = await t.pause_for_human(timeout=120, fallback="abort")
if decision == "approve":
    send_email(to="customer@example.com", body=draft)
```

### Replay Callbacks (any SDK)

```python
# Python — react to replays in real-time
t.on_replay(lambda branch, payload: print(f"Replaying on {branch}"))
```

```typescript
// TypeScript — listen for replay events via SSE
t.onReplay((branch, payload) => console.log(`Replaying on ${branch}`));
```

```csharp
// .NET — register a replay handler
t.OnReplay((branch, payload) => Console.WriteLine($"Replaying on {branch}"));
```

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/v1/health` | Health check (no auth) |
| `POST` | `/v1/traces` | Create a new trace |
| `GET` | `/v1/traces` | List traces (cursor pagination, filters) |
| `GET` | `/v1/traces/{id}` | Get trace with all events |
| `POST` | `/v1/events` | Create an event |
| `POST` | `/v1/events/{id}/pause` | Pause for human-in-the-loop |
| `POST` | `/v1/events/{id}/resume` | Resume with decision |
| `POST` | `/v1/events/{id}/replay` | Replay from event (forks branch) |
| `GET` | `/v1/traces/{id}/stream` | SSE stream for real-time HITL & replay notifications |

## Development

### Backend
```bash
cd backend
dotnet run --project src/Tracewire.Api
```

### Frontend
```bash
cd frontend
npm install && npm run dev
```

### SDKs
```bash
cd sdk/python  && pip install -e ".[dev]" && pytest
cd sdk/typescript && npm install && npm test
cd sdk/dotnet && dotnet test
```

## License

[Apache 2.0](LICENSE)
