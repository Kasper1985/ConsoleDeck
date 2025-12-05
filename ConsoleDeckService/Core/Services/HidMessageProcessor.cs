using ConsoleDeckService.Core.Models;
using ConsoleDeckService.Core.Interfaces;
using System.Collections.Concurrent;

namespace ConsoleDeckService.Core.Services;

/// <summary>
/// Processes HID messages from the device monitor and executes corresponding actions.
/// Acts as the bridge between HID input and action execution.
/// </summary>
public class HidMessageProcessor : IDisposable
{
    private readonly ILogger<HidMessageProcessor> _logger;
    private readonly IHidDeviceMonitor _hidMonitor;
    private readonly IActionExecutor _actionExecutor;
    private readonly IConfigurationService _configService;
    private readonly ISystemTrayProvider? _trayProvider;
    
    private readonly ConcurrentQueue<int> _messageQueue;
    private readonly CancellationTokenSource _processingCts;
    private Task? _processingTask;
    private bool _isProcessing;

    public HidMessageProcessor(
        ILogger<HidMessageProcessor> logger,
        IHidDeviceMonitor hidMonitor,
        IActionExecutor actionExecutor,
        IConfigurationService configService,
        ISystemTrayProvider? trayProvider = null)
    {
        _logger = logger;
        _hidMonitor = hidMonitor;
        _actionExecutor = actionExecutor;
        _configService = configService;
        _trayProvider = trayProvider;
        
        _messageQueue = new ConcurrentQueue<int>();
        _processingCts = new CancellationTokenSource();

        // Subscribe to HID events
        _hidMonitor.ConsoleDeckKeyPressed += OnConsoleDeckKeyPressed;
        _hidMonitor.DeviceConnected += OnDeviceConnected;
        _hidMonitor.DeviceDisconnected += OnDeviceDisconnected;

        // Subscribe to configuration changes
        _configService.ConfigurationReloaded += OnConfigurationReloaded;
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        if (_isProcessing)
        {
            _logger.LogWarning("Message processor is already running");
            return;
        }

        _logger.LogInformation("Starting HID message processor");
        _isProcessing = true;

        // Start the processing loop
        _processingTask = Task.Run(async () => await ProcessMessageQueueAsync(_processingCts.Token), cancellationToken);

        await Task.CompletedTask;
    }

    public async Task StopProcessingAsync()
    {
        if (!_isProcessing)
        {
            return;
        }

        _logger.LogInformation("Stopping HID message processor");
        _isProcessing = false;

        _processingCts.Cancel();

        if (_processingTask != null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    private void OnConsoleDeckKeyPressed(object? sender, int keyCode)
    {
        _logger.LogDebug("Key pressed detected, queuing for processing: 0x{KeyCode:X2} ({Decimal})", keyCode, keyCode);
        _messageQueue.Enqueue(keyCode);
    }

    private void OnDeviceConnected(object? sender, string deviceName)
    {
        _logger.LogInformation("ConsoleDeck device connected: {DeviceName}", deviceName);
        _trayProvider?.UpdateConnectionStatus(deviceName);
        _trayProvider?.ShowNotification("ConsoleDeck Connected", $"Device connected: {deviceName}");
    }

    private void OnDeviceDisconnected(object? sender, string deviceName)
    {
        _logger.LogWarning("ConsoleDeck device disconnected: {DeviceName}", deviceName);
        _trayProvider?.UpdateConnectionStatus();
        _trayProvider?.ShowNotification("ConsoleDeck Disconnected", 
            $"Device disconnected: {deviceName}");
    }

    private void OnConfigurationReloaded(object? sender, ConsoleDeckConfiguration config)
    {
        _logger.LogInformation("Configuration reloaded with {Count} key mappings", 
            config.KeyMappings.Count);
        
        _trayProvider?.ShowNotification("Configuration Reloaded", 
            $"Loaded {config.KeyMappings.Count} key mappings");
    }

    private async Task ProcessMessageQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Message processing loop started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_messageQueue.TryDequeue(out var keyCode))
                {
                    await ProcessKeyCodeAsync(keyCode, cancellationToken);
                }
                else
                {
                    // No messages, wait a bit
                    await Task.Delay(50, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message queue");
                await Task.Delay(1000, cancellationToken);
            }
        }

        _logger.LogDebug("Message processing loop stopped");
    }

    private async Task ProcessKeyCodeAsync(int keyCode, CancellationToken cancellationToken)
    {
        // Validate it's a ConsoleDeck key code
        if (keyCode < 0xF1 || keyCode > 0xF9)
        {
            _logger.LogWarning("Ignoring invalid ConsoleDeck key code: 0x{KeyCode:X2} ({Decimal}).", keyCode, keyCode);
            return;
        }

        _logger.LogInformation("Processing key: 0x{KeyCode:X2} ({Decimal})", keyCode, keyCode);

        // Get action from configuration
        var action = _configService.GetActionForKey(keyCode);

        if (action == null)
        {
            _logger.LogWarning("No action configured for key code: 0x{keyCode:X2} ({Decimal})", keyCode, keyCode);
            _trayProvider?.ShowNotification("No Action Configured", $"Button #{0xFA - keyCode} with key code 0x{keyCode:X2} ({keyCode}) has no action assigned");
            return;
        }

        if (!action.Enabled)
        {
            _logger.LogDebug("Action '{ActionName}' for key code: 0x{keyCode:X2} ({Decimal}) is disabled", action.Name, keyCode, keyCode);
            return;
        }
        _logger.LogInformation("Executing action: {ActionName} for key code: 0x{keyCode:X2} ({Decimal})", action.Name, keyCode, keyCode);

        // Execute the action
        try
        {
            var success = await _actionExecutor.ExecuteAsync(action, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Action '{ActionName}' executed successfully", action.Name);
                
                if (_configService.Configuration.ShowNotifications)
                {
                    _trayProvider?.ShowNotification($"{action.Name}", $"{GetActionDescription(action)}\nCommand for key code 0x{keyCode:X2} ({keyCode}) was executed.");
                }
            }
            else
            {
                _logger.LogWarning("Action '{ActionName}' execution failed", action.Name);
                
                _trayProvider?.ShowNotification("Action Failed", $"{action.Name} could not be executed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action '{ActionName}'", action.Name);
            
            _trayProvider?.ShowNotification("Action Error", $"Error executing {action.Name}: {ex.Message}");
        }
    }

    private static string GetActionDescription(ActionDefinition action) => action.Type switch
    {
        ActionType.LaunchApplication => $"Launching {Path.GetFileNameWithoutExtension(action.Target)}",
        ActionType.OpenUrl => $"Opening {new Uri(action.Target).Host}",
        ActionType.ExecuteScript => $"Running {Path.GetFileName(action.Target)}",
        ActionType.SendKeystrokes => "Sending keystrokes",
        _ => action.Description ?? "Executing action"
    };

    public void Dispose()
    {
        // Unsubscribe from events
        _hidMonitor.ConsoleDeckKeyPressed -= OnConsoleDeckKeyPressed;
        _hidMonitor.DeviceConnected -= OnDeviceConnected;
        _hidMonitor.DeviceDisconnected -= OnDeviceDisconnected;
        _configService.ConfigurationReloaded -= OnConfigurationReloaded;

        _processingCts.Cancel();
        _processingCts.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
