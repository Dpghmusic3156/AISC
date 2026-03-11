using System.Net.Http;
using System.Text.Json;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Anthropic Claude Vision API client.
/// Uses x-api-key header and anthropic-version header.
/// </summary>
public class ClaudeClient : AIClientBase
{
    private const string DefaultBaseUrl = "https://api.anthropic.com";
    private const string AnthropicVersion = "2023-06-01";

    public ClaudeClient(string apiKey, string? customBaseUrl = null)
        : base(apiKey, customBaseUrl) { }

    public override async Task<string> SendImageAsync(byte[] imageData, string systemPrompt, string modelName, CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        var url = $"{baseUrl}/v1/messages";
        var base64Image = ToBase64(imageData);

        var payload = new
        {
            model = modelName,
            max_tokens = 4096,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = "image/png",
                                data = base64Image
                            }
                        },
                        new { type = "text", text = systemPrompt }
                    }
                }
            }
        };

        var responseBody = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent(payload)
            };
            if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
            {
                request.Headers.Add("x-api-key", ApiKey);
            }
            request.Headers.Add("anthropic-version", AnthropicVersion);
            return request;
        }, ct);

        return ExtractResponse(responseBody);
    }

    public override async Task<List<string>> GetModelsAsync(CancellationToken ct = default)
    {
        var baseUrl = (CustomBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        var url = $"{baseUrl}/v1/models";

        try
        {
            var responseBody = await SendWithRetryAsync(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "dummy-key")
                {
                    request.Headers.Add("x-api-key", ApiKey);
                }
                request.Headers.Add("anthropic-version", AnthropicVersion);
                return request;
            }, ct);

            using var doc = JsonDocument.Parse(responseBody);
            var data = doc.RootElement.GetProperty("data");
            var models = new List<string>();
            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("id", out var idProp))
                {
                    models.Add(idProp.GetString() ?? "");
                }
            }
            return models.Where(m => !string.IsNullOrEmpty(m)).ToList();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(CustomBaseUrl)) throw;
            return new List<string> { "claude-3-5-sonnet-20241022", "claude-3-opus-20240229", "claude-3-haiku-20240307" }; 
        }
    }

    private static string ExtractResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var content = doc.RootElement.GetProperty("content");
        if (content.GetArrayLength() > 0)
        {
            return content[0].GetProperty("text").GetString() ?? "(empty response)";
        }
        return "(no content returned)";
    }
}
