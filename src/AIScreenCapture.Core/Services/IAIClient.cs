namespace AIScreenCapture.Core.Services;

/// <summary>
/// Common interface for all AI provider clients.
/// </summary>
public interface IAIClient
{
    /// <summary>
    /// Sends an image to the AI provider with the given prompt and returns the text response.
    /// </summary>
    /// <param name="imageData">PNG image bytes</param>
    /// <param name="systemPrompt">The prompt/instruction to send with the image</param>
    /// <param name="modelName">Model identifier (e.g. "gpt-4o", "gemini-3-flash")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>AI response text</returns>
    Task<string> SendImageAsync(byte[] imageData, string systemPrompt, string modelName, CancellationToken ct = default);

    /// <summary>
    /// Fetches the list of available models from the provider.
    /// </summary>
    Task<List<string>> GetModelsAsync(CancellationToken ct = default);
}
