namespace AIScreenCapture.Core.Models;

public class AIPreset
{
    public string Name { get; set; } = string.Empty;
    public AIProvider Provider { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
}
