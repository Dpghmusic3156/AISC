using System.Collections.Generic;

namespace AIScreenCapture.Core.Models;

public enum OverlayPosition
{
    BottomRight,
    BottomLeft,
    Center,
    TopRight,
    TopLeft
}

public enum AppTheme
{
    Light,
    Dark,
    System
}

public class AppSettings
{
    public string ApiKeyOpenAI { get; set; } = string.Empty;
    public string ApiKeyGemini { get; set; } = string.Empty;
    public string ApiKeyClaude { get; set; } = string.Empty;

    public bool SendDirectlyToAI { get; set; } = true;
    public int ActivePresetIndex { get; set; } = 0;
    public string? BaseUrlOpenAI { get; set; }

    // Window Persist State
    public double ResultWindowWidth { get; set; } = 400;
    public double ResultWindowHeight { get; set; } = 300;
    public double ResultWindowLeft { get; set; } = -1;
    public double ResultWindowTop { get; set; } = -1;
    
    public double LoadingWindowLeft { get; set; } = -1;
    public double LoadingWindowTop { get; set; } = -1;

    // Appearance Settings
    public double CheckmarkScale { get; set; } = 1.0;
    public OverlayPosition OverlayPosition { get; set; } = OverlayPosition.BottomLeft;
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public double Opacity { get; set; } = 1.0;
    public double Scale { get; set; } = 1.0;
    public double FontSize { get; set; } = 10;
    public bool ShowTimer { get; set; } = true;
    public double SelectionOpacity { get; set; } = 0.4;

    // Shortcuts
    public bool UseMouseShortcuts { get; set; } = true;
    public string CaptureShortcut { get; set; } = "Control+Shift+C";
    public string ToggleShortcut { get; set; } = "Control+Shift+V";

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
