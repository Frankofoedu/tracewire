using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tracewire.Sdk;

public class TracewireClient : IDisposable
{
    private readonly HttpClient _http;
    internal string BaseUrl { get; }
    internal string ApiKey { get; }
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TracewireClient(string baseUrl = "http://localhost:5185", string apiKey = "")
    {
        BaseUrl = baseUrl.TrimEnd('/');
        ApiKey = apiKey;
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    public TracewireClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<TraceResponse> CreateTraceAsync(string agentName, string? metadata = null)
    {
        var request = new CreateTraceRequest(agentName, metadata);
        var resp = await _http.PostAsJsonAsync("/v1/traces", request, JsonOpts);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<TraceResponse>(JsonOpts))!;
    }

    public async Task<EventResponse> CreateEventAsync(CreateEventRequest request)
    {
        var resp = await _http.PostAsJsonAsync("/v1/events", request, JsonOpts);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<EventResponse>(JsonOpts))!;
    }

    public async Task PauseEventAsync(Guid eventId, int timeoutSeconds = 60)
    {
        var resp = await _http.PostAsJsonAsync($"/v1/events/{eventId}/pause",
            new { timeoutSeconds }, JsonOpts);
        resp.EnsureSuccessStatusCode();
    }

    public async Task ResumeEventAsync(Guid eventId, string decision, string? comments = null)
    {
        var resp = await _http.PostAsJsonAsync($"/v1/events/{eventId}/resume",
            new { decision, comments }, JsonOpts);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<TraceDetailResponse> GetTraceAsync(Guid traceId)
    {
        return (await _http.GetFromJsonAsync<TraceDetailResponse>($"/v1/traces/{traceId}", JsonOpts))!;
    }

    public void Dispose() => _http.Dispose();
}
