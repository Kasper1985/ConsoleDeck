using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.UI.ViewModels;

/// <summary>
/// ViewModel for the settings window.
/// Manages configuration state and user interactions.
/// </summary>
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly IConfigurationService _configService;
    private readonly IAutoStartService _autoStartService;
    private readonly ILogger<SettingsViewModel> _logger;
    
    private int _vendorId;
    private int? _productId;
    private int _debounceMs;
    private bool _verboseLogging;
    private bool _autoStart;
    private bool _showNotifications;
    private bool _hasUnsavedChanges;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsViewModel(IConfigurationService configService, IAutoStartService autoStartService, ILogger<SettingsViewModel> logger)
    {
        _configService = configService;
        _autoStartService = autoStartService;
        _logger = logger;
        
        KeyMappings = [];
        
        // Load initial configuration
        LoadConfiguration();
    }

    #region Properties

    public int VendorId
    {
        get => _vendorId;
        set
        {
            if (_vendorId != value)
            {
                _vendorId = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public int? ProductId
    {
        get => _productId;
        set
        {
            if (_productId != value)
            {
                _productId = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public int DebounceMs
    {
        get => _debounceMs;
        set
        {
            if (_debounceMs != value)
            {
                _debounceMs = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public bool VerboseLogging
    {
        get => _verboseLogging;
        set
        {
            if (_verboseLogging != value)
            {
                _verboseLogging = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            if (_autoStart != value)
            {
                _autoStart = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set
        {
            if (_showNotifications != value)
            {
                _showNotifications = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set
        {
            if (_hasUnsavedChanges != value)
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<KeyActionMappingViewModel> KeyMappings { get; }

    public static Array ActionTypes => Enum.GetValues<ActionType>();

    #endregion

    #region Methods

    public void LoadConfiguration()
    {
        try
        {
            var config = _configService.Configuration;

            _vendorId = config.VendorId ?? 0xCAFE;
            _productId = config.ProductId;
            _debounceMs = config.DebounceMs;
            _verboseLogging = config.VerboseLogging;
            _showNotifications = config.ShowNotifications;

            //_autoStart = config.AutoStart;
            _autoStart = _autoStartService.IsAutoStartEnabledAsync().GetAwaiter().GetResult();

            KeyMappings.Clear();
            
            // Load existing mappings
            foreach (var mapping in config.KeyMappings.OrderBy(m => m.KeyCode))
            {
                KeyMappings.Add(new KeyActionMappingViewModel(mapping, this));
            }

            // Ensure we have all 9 key codes (0xF1-0xF9)
            for (int keyCode = 0xF1; keyCode <= 0xF9; keyCode++)
            {
                if (!KeyMappings.Any(m => m.KeyCode == keyCode))
                {
                    KeyMappings.Add(new KeyActionMappingViewModel(new KeyActionMapping
                    {
                        KeyCode = keyCode,
                        Action = new ActionDefinition
                        {
                            Name = $"#{0xFA - keyCode} (Not Configured)",
                            Type = ActionType.None,
                            Enabled = false
                        }
                    }, this));
                }
            }

            // Sort by key code
            var sorted = KeyMappings.OrderByDescending(m => m.KeyCode).ToList();
            KeyMappings.Clear();
            foreach (var item in sorted)
            {
                KeyMappings.Add(item);
            }

            _hasUnsavedChanges = false;

            OnPropertyChanged(nameof(VendorId));
            OnPropertyChanged(nameof(ProductId));
            OnPropertyChanged(nameof(DebounceMs));
            OnPropertyChanged(nameof(VerboseLogging));
            OnPropertyChanged(nameof(AutoStart));
            OnPropertyChanged(nameof(ShowNotifications));
            OnPropertyChanged(nameof(HasUnsavedChanges));

            _logger.LogDebug("Configuration loaded into ViewModel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration into ViewModel");
        }
    }

    public async Task<bool> SaveConfigurationAsync()
    {
        try
        {
            _logger.LogInformation("Saving configuration from ViewModel");

            // Update configuration object
            var config = _configService.Configuration;
            config.VendorId = VendorId;
            config.ProductId = ProductId;
            config.DebounceMs = DebounceMs;
            config.VerboseLogging = VerboseLogging;
            config.ShowNotifications = ShowNotifications;

            //config.AutoStart = AutoStart;

            // Update key mappings
            config.KeyMappings.Clear();
            foreach (var mappingVm in KeyMappings)
            {
                config.KeyMappings.Add(new KeyActionMapping
                {
                    KeyCode = mappingVm.KeyCode,
                    Action = new ActionDefinition
                    {
                        Name = mappingVm.ActionName ?? string.Empty,
                        Description = mappingVm.ActionDescription,
                        Type = mappingVm.ActionType,
                        Target = mappingVm.ActionTarget ?? string.Empty,
                        Arguments = mappingVm.ActionArguments,
                        WorkingDirectory = mappingVm.WorkingDirectory,
                        Enabled = mappingVm.IsEnabled
                    }
                });
            }

            // Validate
            var errors = await _configService.ValidateConfigurationAsync();
            if (errors.Any())
            {
                _logger.LogWarning("Configuration validation failed with {Count} errors", errors.Count());
                foreach (var error in errors)
                {
                    _logger.LogWarning("  - {Error}", error);
                }
                return false;
            }

            // Apply auto-start setting
            if (AutoStart)
            {
                var enabled = await _autoStartService.EnableAutoStartAsync();
                if (!enabled)
                {
                    _logger.LogWarning("Failed to enable auto-start");
                }
            }
            else
            {
                var disabled = await _autoStartService.DisableAutoStartAsync();
                if (!disabled)
                {
                    _logger.LogWarning("Failed to disable auto-start");
                }
            }

            // Save to file
            await _configService.SaveConfigurationAsync();

            HasUnsavedChanges = false;
            _logger.LogInformation("Configuration saved successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            return false;
        }
    }

    public void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }

    #endregion

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
