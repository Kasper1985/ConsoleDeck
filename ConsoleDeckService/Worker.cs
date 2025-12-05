using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.Services;

namespace ConsoleDeckService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfigurationService _configService;
    private readonly IHidDeviceMonitor _hidMonitor;
    private readonly ISystemTrayProvider _trayProvider;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly HidMessageProcessor _messageProcessor;

    public Worker(ILogger<Worker> logger, IConfigurationService configService, IHidDeviceMonitor hidMonitor, ISystemTrayProvider trayProvider, HidMessageProcessor messageProcessor, IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _configService = configService;
        _hidMonitor = hidMonitor;
        _trayProvider = trayProvider;
        _messageProcessor = messageProcessor;
        _hostLifetime = hostLifetime;

        // Subscribe to tray events
        _trayProvider.ExitRequested += OnExitRequested;
        _trayProvider.SettingsRequested += OnSettingsRequested;
        _trayProvider.StatusRequested += OnStatusRequested;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ConsoleDeck Service starting...");

        try
        {
            // 1. Load configuration
            _logger.LogInformation("Loading configuration...");
            await _configService.LoadConfigurationAsync();

            // Validate configuration
            var errors = await _configService.ValidateConfigurationAsync();
            if (errors.Any())
            {
                _logger.LogWarning("Configuration validation errors:");
                foreach (var error in errors)
                {
                    _logger.LogWarning("  - {Error}", error);
                }
            }

            // 2. Initialize system tray
            _logger.LogInformation("Initializing system tray...");
            await _trayProvider.InitializeAsync();
            _trayProvider.UpdateTooltip("ConsoleDeck - Starting...");

            // 3. Start HID message processor
            _logger.LogInformation("Starting message processor...");
            await _messageProcessor.StartProcessingAsync(stoppingToken);

            // 4. Start HID device monitoring
            _logger.LogInformation("Starting HID device monitoring...");
            await _hidMonitor.StartMonitoringAsync(stoppingToken);

            _logger.LogInformation("ConsoleDeck Service started successfully!");
            _trayProvider.UpdateTooltip("ConsoleDeck - Not Connected");
            _trayProvider.ShowNotification("ConsoleDeck Started",
                $"Monitoring for ConsoleDeck device... ({_configService.Configuration.KeyMappings.Select(km => km.Action.Enabled).Count()} actions configured)");

            // Keep the background service running until cancellation
            while (!stoppingToken.IsCancellationRequested)
            {
                // Periodic status logging
                /*if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var devices = _hidMonitor.GetConnectedDevices();
                    var deviceCount = devices.Count();

                    if (deviceCount > 0)
                    {
                        _logger.LogDebug("ConsoleDeck service running. Connected devices: {Count}", deviceCount);
                    }
                }*/

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ConsoleDeck Service is stopping (cancellation requested)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in ConsoleDeck Service");
            _trayProvider.ShowNotification("ConsoleDeck Error",
                "Service encountered a fatal error. Check logs for details.");
            throw;
        }
        finally
        {
            await ShutdownServicesAsync();
        }
    }

    private async Task ShutdownServicesAsync()
    {
        _logger.LogInformation("Shutting down ConsoleDeck services...");

        try
        {
            // Stop message processor
            await _messageProcessor.StopProcessingAsync();

            // Stop HID monitoring
            await _hidMonitor.StopMonitoringAsync();

            // Shutdown tray
            await _trayProvider.ShutdownAsync();

            _logger.LogInformation("ConsoleDeck Service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service shutdown");
        }
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _logger.LogInformation("Exit requested from system tray");

        // Trigger application shutdown
        var lifetime = Avalonia.Application.Current?.ApplicationLifetime
            as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

        lifetime?.Shutdown();

        // Stop the host gracefully
        _hostLifetime.StopApplication();
    }

    private void OnSettingsRequested(object? sender, EventArgs e)
    {
        _logger.LogInformation("Settings requested from system tray");

        // TODO: Open settings window/editor
        // For now, just show a notification
        _trayProvider.ShowNotification("Settings",
            "Settings editor coming soon!\nEdit appsettings.json manually for now.");
    }

    private void OnStatusRequested(object? sender, EventArgs e)
    {
        _logger.LogInformation("Status requested from system tray");

        var devices = _hidMonitor.GetConnectedDevices().ToList();
        var deviceCount = devices.Count;
        var actionCount = _configService.Configuration.KeyMappings.Count;

        var statusMessage = deviceCount > 0
            ? $"Connected: {string.Join(", ", devices)}\n{actionCount} actions configured"
            : $"No devices connected\n{actionCount} actions configured";

        _trayProvider.ShowNotification("ConsoleDeck Status", statusMessage, 5000);
    }

    public override void Dispose()
    {
        _trayProvider.ExitRequested -= OnExitRequested;
        _trayProvider.SettingsRequested -= OnSettingsRequested;
        _trayProvider.StatusRequested -= OnStatusRequested;

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
