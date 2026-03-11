using System;
using System.IO;
using System.Text.Json;
using AIScreenCapture.Core.Models;

namespace AIScreenCapture.Core.Services;

public class SettingsManager
{
    private readonly string _configFilePath;

    public SettingsManager()
    {
        string appDataFolderr = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appDataFolderr, "AIScreenCapture");
        _configFilePath = Path.Combine(appFolder, "config.json");
    }

    public AppSettings Load()
    {
        if (File.Exists(_configFilePath))
        {
            try
            {
                string json = File.ReadAllText(_configFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    return settings;
                }
            }
            catch
            {
                // Fall back to default if corrupted
            }
        }
        
        var defaultSettings = new AppSettings();
        Save(defaultSettings);
        return defaultSettings;
    }

    public void Save(AppSettings settings)
    {
        string? directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(_configFilePath, json);
    }
}
