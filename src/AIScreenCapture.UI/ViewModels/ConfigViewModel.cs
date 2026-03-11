using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AIScreenCapture.UI.ViewModels;

public partial class ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private AppSettings _appSettings;

    [ObservableProperty]
    private string _apiKeyOpenAI = string.Empty;

    [ObservableProperty]
    private string _apiKeyGemini = string.Empty;

    [ObservableProperty]
    private string _apiKeyClaude = string.Empty;

    public ObservableCollection<AIPreset> Presets { get; } = new();

    public AIScreenCapture.Core.Models.AIProvider[] AvailableProviders => new[] 
    {
        AIScreenCapture.Core.Models.AIProvider.OpenAI,
        AIScreenCapture.Core.Models.AIProvider.Gemini,
        AIScreenCapture.Core.Models.AIProvider.Claude
    };

    public ConfigViewModel()
    {
        _settingsManager = new SettingsManager();
        _appSettings = _settingsManager.Load();

        ApiKeyOpenAI = _appSettings.ApiKeyOpenAI ?? string.Empty;
        ApiKeyGemini = _appSettings.ApiKeyGemini ?? string.Empty;
        ApiKeyClaude = _appSettings.ApiKeyClaude ?? string.Empty;

        foreach (var preset in _appSettings.Presets)
        {
            Presets.Add(preset);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _appSettings.ApiKeyOpenAI = ApiKeyOpenAI;
        _appSettings.ApiKeyGemini = ApiKeyGemini;
        _appSettings.ApiKeyClaude = ApiKeyClaude;

        _appSettings.Presets.Clear();
        foreach (var p in Presets)
        {
            _appSettings.Presets.Add(p);
        }

        _settingsManager.Save(_appSettings);
        System.Windows.MessageBox.Show("Settings saved successfully!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    [RelayCommand]
    private void AddPreset()
    {
        Presets.Add(new AIPreset { Name = "New Preset", Provider = AIScreenCapture.Core.Models.AIProvider.OpenAI, ModelName = "gpt-4o", SystemPrompt = "Enter prompt here" });
    }

    [RelayCommand]
    private void RemovePreset(AIPreset? preset)
    {
        if (preset != null && Presets.Contains(preset))
        {
            Presets.Remove(preset);
        }
    }
}
