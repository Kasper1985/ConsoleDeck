using System.Text.Json;
using ConsoleDeckService.Core.Models;
using ConsoleDeckService.Core.Interfaces;


namespace ConsoleDeckService.Core.Services;

/// <summary>
/// Service for managing ConsoleDeck configuration.
/// Loads from appsettings.json and supports hot-reload.
/// </summary>
public class ConfigurationService(ILogger<ConfigurationService> logger, IConfiguration configuration) : IConfigurationService
{    
    private readonly string _configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    private FileSystemWatcher? _fileWatcher;
    private ConsoleDeckConfiguration _currentConfig = new();

    public event EventHandler<ConsoleDeckConfiguration>? ConfigurationReloaded;

    public ConsoleDeckConfiguration Configuration => _currentConfig;


    public async Task LoadConfigurationAsync()
    {
        logger.LogInformation("Loading ConsoleDeck configuration from {ConfigFile}", _configFilePath);

        try
        {
            // Load from IConfiguration (appsettings.json)
            var configSection = configuration.GetSection("ConsoleDeck");
            
            if (!configSection.Exists())
            {
                logger.LogWarning("ConsoleDeck configuration section not found, using defaults");
                _currentConfig = CreateDefaultConfiguration();
            }
            else
            {
                _currentConfig = configSection.Get<ConsoleDeckConfiguration>() ?? CreateDefaultConfiguration();
                logger.LogInformation("Loaded configuration with {Count} key mappings", 
                    _currentConfig.KeyMappings.Count);
            }

            // Set up file watcher for hot-reload
            SetupFileWatcher();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load configuration, using defaults");
            _currentConfig = CreateDefaultConfiguration();
        }
    }

    public async Task SaveConfigurationAsync()
    {
        logger.LogInformation("Saving ConsoleDeck configuration to {ConfigFile}", _configFilePath);

        try
        {
            // Read existing appsettings.json
            var json = File.Exists(_configFilePath) ? await File.ReadAllTextAsync(_configFilePath) : "{}";

            var jsonDoc = JsonDocument.Parse(json);
            var rootDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];

            // Update ConsoleDeck section
            rootDict["ConsoleDeck"] = _currentConfig;

            // Write back
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var updatedJson = JsonSerializer.Serialize(rootDict, options);
            await File.WriteAllTextAsync(_configFilePath, updatedJson);

            logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save configuration");
            throw;
        }
    }

    public async Task ReloadConfigurationAsync()
    {
        logger.LogInformation("Reloading configuration");

        var oldConfig = _currentConfig;
        await LoadConfigurationAsync();

        if (_currentConfig != oldConfig)
            ConfigurationReloaded?.Invoke(this, _currentConfig);
    }

    public ActionDefinition? GetActionForKey(int keyCode)
    {
        var mapping = _currentConfig.GetMapping(keyCode);
        return mapping?.Action;
    }

    public async Task<IEnumerable<string>> ValidateConfigurationAsync()
    {
        var errors = new List<string>();

        // Validate function key numbers are in range (13-21)
        foreach (var mapping in _currentConfig.KeyMappings)
        {
            if (mapping.KeyCode < 0xF1 || mapping.KeyCode > 0xF9)
                errors.Add($"Invalid key code: 0x{mapping.KeyCode:X2}. Must be 0xF1 (241) - 0xF9 (249).");

            // Validate action
            if (mapping.Action == null)
                errors.Add($"0x{mapping.KeyCode:X2}: Action is required");

            if (string.IsNullOrWhiteSpace(mapping?.Action?.Name))
                errors.Add($"0x{mapping?.KeyCode:X2}: Action name is required");

            if (mapping?.Action?.Type != ActionType.None && string.IsNullOrWhiteSpace(mapping?.Action?.Target))
                errors.Add($"0x{mapping?.KeyCode:X2}: Action target is required for {mapping?.Action?.Type}");
        }

        // Check for duplicate function key mappings
        var duplicates = _currentConfig.KeyMappings
            .GroupBy(m => m.KeyCode)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicates)
            errors.Add($"Duplicate mapping found for key code: 0x{duplicate:X2}");

        // Validate debounce time
        if (_currentConfig.DebounceMs < 0 || _currentConfig.DebounceMs > 5000)
            errors.Add($"Invalid debounce time: {_currentConfig.DebounceMs}ms. Must be 0-5000ms.");

        await Task.CompletedTask;
        return errors;
    }

    private void SetupFileWatcher()
    {
        try
        {
            _fileWatcher?.Dispose();

            _fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_configFilePath) ?? AppContext.BaseDirectory,
                Filter = Path.GetFileName(_configFilePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            Timer? debounceTimer = null;
            _fileWatcher.Changed += async (sender, e) =>
            {
                logger.LogInformation("Configuration file changed, reloading...");

                // Dispose of any existing timer to reset the debounce period
                debounceTimer?.Dispose();

                // Create a new timer with 500ms delay
                debounceTimer = new(async _ =>
                {
                    debounceTimer?.Dispose();
                    debounceTimer = null;
                    try
                    {
                        await ReloadConfigurationAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to reload configuration after file change.");
                    }
                }, null, 500, Timeout.Infinite);
            };

            _fileWatcher.EnableRaisingEvents = true;
            logger.LogDebug("Configuration file watcher enabled");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not set up configuration file watcher");
        }
    }

    private static ConsoleDeckConfiguration CreateDefaultConfiguration()
    {
        return new ConsoleDeckConfiguration
        {
            VendorId = 0xCAFE, // ConsoleDeck Vendor ID
            ProductId = null,
            DebounceMs = 200,
            ShowNotifications = true,
            KeyMappings =
            [
                new()
                {
                    KeyCode = 0xF1,
                    Action = new ActionDefinition
                    {
                        Name = "Open Notepad",
                        Description = "Opens Windows Notepad",
                        Type = ActionType.LaunchApplication,
                        Target = "notepad.exe",
                        Enabled = true
                    }
                },
                new()
                {
                    KeyCode = 0xF2,
                    Action = new ActionDefinition
                    {
                        Name = "Open Calculator",
                        Description = "Opens Windows Calculator",
                        Type = ActionType.LaunchApplication,
                        Target = "calc.exe",
                        Enabled = true
                    }
                },
                new()
                {
                    KeyCode = 0xF3,
                    Action = new ActionDefinition
                    {
                        Name = "Open GitHub",
                        Description = "Opens GitHub in default browser",
                        Type = ActionType.OpenUrl,
                        Target = "https://github.com",
                        Enabled = true
                    }
                }
            ]
        };
    }
}
