using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Base class for AI clients with shared HttpClient, timeout, and retry logic.
/// </summary>
public abstract class AIClientBase : IAIClient
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = 
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(9)
    };

    protected string ApiKey { get; }
    protected string? CustomBaseUrl { get; }

    protected AIClientBase(string apiKey, string? customBaseUrl = null)
    {
        ApiKey = apiKey;
        CustomBaseUrl = customBaseUrl;
    }

    public abstract Task<string> SendImageAsync(byte[] imageData, string systemPrompt, string modelName, CancellationToken ct = default);

    public abstract Task<List<string>> GetModelsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sends an HTTP request with retry logic for 429 (rate limit) responses.
    /// </summary>
    protected async Task<string> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            using var request = requestFactory();
            using var response = await SharedHttpClient.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelays[attempt], ct);
                continue;
            }

            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"AI API Error ({(int)response.StatusCode}): {responseBody}");
            }

            return responseBody;
        }

        throw new HttpRequestException("Max retries exceeded due to rate limiting (429).");
    }

    /// <summary>
    /// Converts PNG bytes to a base64 string.
    /// </summary>
    protected static string ToBase64(byte[] imageData) => Convert.ToBase64String(imageData);

    /// <summary>
    /// Creates a JSON StringContent with proper content type.
    /// </summary>
    protected static StringContent JsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
