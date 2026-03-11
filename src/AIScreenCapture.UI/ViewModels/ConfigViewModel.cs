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

    [ObservableProperty]
    private string? _baseUrlOpenAI;

    [ObservableProperty]
    private int _activePresetIndex;

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
        BaseUrlOpenAI = _appSettings.BaseUrlOpenAI;
        ActivePresetIndex = _appSettings.ActivePresetIndex;

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
        _appSettings.BaseUrlOpenAI = BaseUrlOpenAI;
        _appSettings.ActivePresetIndex = ActivePresetIndex;

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

    [RelayCommand]
    private async Task FetchModelsAsync(AIPreset? preset)
    {
        if (preset == null) return;

        try
        {
            // Try to get API key from current ViewModel if not saved
            string apiKey = preset.CustomApiKey ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = AIServiceFactory.GetApiKey(_appSettings, preset.Provider);
            }
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = preset.Provider switch
                {
                    AIScreenCapture.Core.Models.AIProvider.OpenAI => ApiKeyOpenAI,
                    AIScreenCapture.Core.Models.AIProvider.Gemini => ApiKeyGemini,
                    AIScreenCapture.Core.Models.AIProvider.Claude => ApiKeyClaude,
                    _ => string.Empty
                };
            }

            // Get Base URL
            string? baseUrl = preset.CustomBaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl) && preset.Provider == AIScreenCapture.Core.Models.AIProvider.OpenAI)
            {
                baseUrl = BaseUrlOpenAI;
            }

            if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(baseUrl))
            {
                System.Windows.MessageBox.Show($"Please enter an API Key or Custom Base URL for {preset.Provider} first.", "Missing Key", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var client = AIServiceFactory.CreateClient(preset.Provider, apiKey, baseUrl);
            var models = await client.GetModelsAsync();

            preset.AvailableModels.Clear();
            foreach (var m in models)
            {
                preset.AvailableModels.Add(m);
            }

            if (models.Count > 0 && !models.Contains(preset.ModelName))
            {
                preset.ModelName = models[0]; // Auto select first if current is invalid
            }
            else if (models.Count == 0)
            {
                System.Windows.MessageBox.Show("No models found from provider.", "Fetch Models", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error fetching models: {ex.Message}", "Fetch Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
