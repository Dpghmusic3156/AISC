using System.Collections.Generic;

namespace AIScreenCapture.Core.Models;

public class AppSettings
{
    public string ApiKeyOpenAI { get; set; } = string.Empty;
    public string ApiKeyGemini { get; set; } = string.Empty;
    public string ApiKeyClaude { get; set; } = string.Empty;

    public List<AIPreset> Presets { get; set; } = new();

    public AppSettings()
    {
        Presets.Add(new AIPreset
        {
            Name = "Giải thích nội dung trong ảnh này",
            Provider = AIProvider.Gemini,
            ModelName = "gemini-3-flash",
            SystemPrompt = "Hãy giải thích chi tiết nội dung của bức ảnh này. Nếu có văn bản, hãy phiên dịch và tóm tắt."
        });
    }
}
