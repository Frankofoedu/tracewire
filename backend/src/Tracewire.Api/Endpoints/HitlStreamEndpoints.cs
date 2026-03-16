using System.Text.Json;
using System.Text.Json.Serialization;
using Tracewire.Api.Middleware;
using Tracewire.Application.Services;

namespace Tracewire.Api.Endpoints;

public static class HitlStreamEndpoints
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void MapHitlStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/traces/{traceId:guid}/stream", async (
            Guid traceId,
            HttpContext ctx,
            HitlNotificationService notifications,
            CancellationToken ct) =>
        {
            if (!ctx.HasScope("read:traces"))
            {
                ctx.Response.StatusCode = 403;
                return;
            }

            ctx.Response.ContentType = "text/event-stream";
            ctx.Response.Headers.CacheControl = "no-cache";
            ctx.Response.Headers.Connection = "keep-alive";

            await ctx.Response.WriteAsync(": connected\n\n", ct);
            await ctx.Response.Body.FlushAsync(ct);

            var reader = notifications.Subscribe(traceId);
            try
            {
                await foreach (var notification in reader.ReadAllAsync(ct))
                {
                    var json = JsonSerializer.Serialize(notification, JsonOpts);
                    await ctx.Response.WriteAsync($"data: {json}\n\n", ct);
                    await ctx.Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                notifications.Unsubscribe(traceId, reader);
            }
        }).ExcludeFromDescription();
    }
}
