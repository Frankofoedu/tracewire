from __future__ import annotations

from typing import Any

from tracewire.adapters import TracewireAdapter
from tracewire.trace import TraceContext


class TracewireAutoGenMiddleware:
    """AutoGen middleware adapter.

    Usage:
        from tracewire.adapters.autogen import TracewireAutoGenMiddleware
        middleware = TracewireAutoGenMiddleware(trace_context)
        agent.register_middleware(middleware)
    """

    def __init__(self, ctx: TraceContext):
        self._adapter = TracewireAdapter.__new__(TracewireAdapter)
        self._adapter._ctx = ctx

    async def on_message(self, message: Any, sender: Any, **kwargs: Any) -> None:
        self._adapter.on_llm_start(str(message), sender=str(sender))

    async def on_response(self, response: Any, **kwargs: Any) -> None:
        self._adapter.on_llm_end(str(response))

    async def on_tool_call(self, tool_name: str, arguments: Any, **kwargs: Any) -> None:
        self._adapter.on_tool_start(tool_name, arguments)

    async def on_tool_result(self, tool_name: str, result: Any, **kwargs: Any) -> None:
        self._adapter.on_tool_end(tool_name, result)
