using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// OpenAI Vision API client. Supports custom baseUrl for OpenAI-compatible APIs.
/// </summary>
public class OpenAIClient : AIClientBase
{
    private const string DefaultBaseUrl = "https://api.openai.com";

    public OpenAIClient(string apiKey, string? customBaseUrl = null)
        : base(apiKey, customBaseUrl) { }

    public override async Task<string> SendImageAsync(byte[] imageData, string systemPrompt, string modelName, CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        if (baseUrl.EndsWith("/v1")) baseUrl = baseUrl.Substring(0, baseUrl.Length - 3);

        var url = $"{baseUrl}/v1/chat/completions";
        var base64Image = ToBase64(imageData);

        var payload = new
        {
            model = modelName,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = systemPrompt },
                        new { type = "image_url", image_url = new { url = $"data:image/png;base64,{base64Image}" } }
                    }
                }
            },
            max_tokens = 1000
        };

        var responseBody = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            }
            request.Content = JsonContent(payload);
            return request;
        }, ct);

        return ExtractResponse(responseBody);
    }

    public override async Task<List<string>> GetModelsAsync(CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        if (baseUrl.EndsWith("/v1")) baseUrl = baseUrl.Substring(0, baseUrl.Length - 3);

        var url = $"{baseUrl}/v1/models";

        try
        {
            var responseBody = await SendWithRetryAsync(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                }
                return request;
            }, ct);

            using var doc = JsonDocument.Parse(responseBody);
            var data = doc.RootElement.GetProperty("data");
            var models = new List<string>();
            foreach (var item in data.EnumerateArray())
            {
                models.Add(item.GetProperty("id").GetString() ?? "");
            }
            return models.Where(m => !string.IsNullOrEmpty(m)).ToList();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(CustomBaseUrl)) throw; // Show the real error for custom URLs
            return new List<string> { "gpt-4o", "gpt-4-turbo", "gpt-3.5-turbo" }; // Fallback
        }
    }

    private static string ExtractResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() > 0)
        {
            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "(empty response)";
        }
        return "(no choices returned)";
    }
}
