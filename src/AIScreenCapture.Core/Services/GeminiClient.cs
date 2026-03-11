using System.Net.Http;
using System.Text.Json;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Google Gemini Vision API client using the REST generateContent endpoint.
/// API key is passed as query parameter.
/// </summary>
public class GeminiClient : AIClientBase
{
    private const string DefaultBaseUrl = "https://generativelanguage.googleapis.com";

    public GeminiClient(string apiKey, string? customBaseUrl = null)
        : base(apiKey, customBaseUrl) { }

    public override async Task<string> SendImageAsync(byte[] imageData, string systemPrompt, string modelName, CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        var url = $"{baseUrl}/v1beta/models/{modelName}:generateContent";
        if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
        {
            url += $"?key={ApiKey}";
        }
        var base64Image = ToBase64(imageData);

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = systemPrompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "image/png",
                                data = base64Image
                            }
                        }
                    }
                }
            }
        };

        var responseBody = await SendWithRetryAsync(() =>
        {
            return new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent(payload)
            };
        }, ct);

        return ExtractResponse(responseBody);
    }

    public override async Task<List<string>> GetModelsAsync(CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        var url = $"{baseUrl}/v1beta/models";
        if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
        {
            url += $"?key={ApiKey}";
        }

        try
        {
            var responseBody = await SendWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, url), ct);

            using var doc = JsonDocument.Parse(responseBody);
            var modelsNode = doc.RootElement.GetProperty("models");
            var models = new List<string>();
            foreach (var item in modelsNode.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "";
                if (name.StartsWith("models/")) name = name.Substring(7);
                models.Add(name);
            }
            return models.Where(m => !string.IsNullOrEmpty(m)).ToList();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(CustomBaseUrl)) throw;
            return new List<string> { "gemini-1.5-flash", "gemini-1.5-pro", "gemini-2.0-flash" }; // Fallback
        }
    }

    private static string ExtractResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var candidates = doc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() > 0)
        {
            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            if (parts.GetArrayLength() > 0)
            {
                return parts[0].GetProperty("text").GetString() ?? "(empty response)";
            }
        }
        return "(no candidates returned)";
    }
}
