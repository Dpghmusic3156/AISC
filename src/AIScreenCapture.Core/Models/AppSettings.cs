using System.Collections.Generic;

namespace AIScreenCapture.Core.Models;

public class AppSettings
{
    public string ApiKeyOpenAI { get; set; } = string.Empty;
    public string ApiKeyGemini { get; set; } = string.Empty;
    public string ApiKeyClaude { get; set; } = string.Empty;

    public bool SendDirectlyToAI { get; set; } = true;
    public int ActivePresetIndex { get; set; } = 0;
    public string? BaseUrlOpenAI { get; set; }

    // Window Persist State
    public double ResultWindowWidth { get; set; } = 600;
    public double ResultWindowHeight { get; set; } = 500;
    public double ResultWindowLeft { get; set; } = -1;
    public double ResultWindowTop { get; set; } = -1;
    
    public double LoadingWindowLeft { get; set; } = -1;
    public double LoadingWindowTop { get; set; } = -1;

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
