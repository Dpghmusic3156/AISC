using AIScreenCapture.Core.Models;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Creates the appropriate AI client based on the provider type.
/// </summary>
public static class AIServiceFactory
{
    /// <summary>
    /// Creates an IAIClient for the given provider with the specified API key and optional base URL.
    /// </summary>
    public static IAIClient CreateClient(AIProvider provider, string apiKey, string? customBaseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(customBaseUrl))
            throw new InvalidOperationException($"API key for {provider} is not configured. Please set it in Settings.");

        return provider switch
        {
            AIProvider.OpenAI => new OpenAIClient(apiKey, customBaseUrl),
            AIProvider.Gemini => new GeminiClient(apiKey, customBaseUrl),
            AIProvider.Claude => new ClaudeClient(apiKey, customBaseUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), $"Unsupported provider: {provider}")
        };
    }

    /// <summary>
    /// Gets the API key for the given provider from app settings.
    /// </summary>
    public static string GetApiKey(AppSettings settings, AIProvider provider)
    {
        return provider switch
        {
            AIProvider.OpenAI => settings.ApiKeyOpenAI,
            AIProvider.Gemini => settings.ApiKeyGemini,
            AIProvider.Claude => settings.ApiKeyClaude,
            _ => string.Empty
        };
    }
}
