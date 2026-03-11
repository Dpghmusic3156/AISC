using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AIScreenCapture.Core.Models;

public class AIPreset : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private AIProvider _provider;
    private string _modelName = string.Empty;
    private string _systemPrompt = string.Empty;
    private string? _customBaseUrl;
    private string? _customApiKey;

    public string Name 
    { 
        get => _name; 
        set { if (_name != value) { _name = value; OnPropertyChanged(); } } 
    }
    
    public AIProvider Provider 
    { 
        get => _provider; 
        set { if (_provider != value) { _provider = value; OnPropertyChanged(); } } 
    }
    
    public string ModelName 
    { 
        get => _modelName; 
        set { if (_modelName != value) { _modelName = value; OnPropertyChanged(); } } 
    }
    
    public string SystemPrompt 
    { 
        get => _systemPrompt; 
        set { if (_systemPrompt != value) { _systemPrompt = value; OnPropertyChanged(); } } 
    }
    
    public string? CustomBaseUrl 
    { 
        get => _customBaseUrl; 
        set { if (_customBaseUrl != value) { _customBaseUrl = value; OnPropertyChanged(); } } 
    }

    public string? CustomApiKey 
    { 
        get => _customApiKey; 
        set { if (_customApiKey != value) { _customApiKey = value; OnPropertyChanged(); } } 
    }

    [JsonIgnore]
    public ObservableCollection<string> AvailableModels { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
