namespace Tracewire.Domain.Enums;

public enum EventType
{
    Prompt,
    ToolCall,
    ModelResponse,
    MemoryWrite,
    Error,
    Retry
}
