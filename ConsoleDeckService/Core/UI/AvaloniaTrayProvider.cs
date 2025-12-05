using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using ConsoleDeckService.Core.Interfaces;

namespace ConsoleDeckService.Core.UI;

/// <summary>
/// Avalonia-based system tray provider for ConsoleDeck.
/// Provides a cross-platform system tray icon with menu.
/// </summary>
public class AvaloniaTrayProvider(ILogger<AvaloniaTrayProvider> logger) : ISystemTrayProvider, IDisposable
{
    private TrayIcon? _trayIcon;
    private NativeMenu? _trayMenu;
    private NativeMenuItem? _statusMenuItem;
    private bool _isInitialized;

    public event EventHandler? ExitRequested;
    public event EventHandler? SettingsRequested;
    public event EventHandler? StatusRequested;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            logger.LogWarning("Tray provider is already initialized");
            return;
        }

        logger.LogInformation("Initializing Avalonia system tray");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // Load the tray icon from assets
                var iconUri = new Uri("avares://ConsoleDeckService/Core/UI/Assets/app-icon.ico");
                var iconStream = AssetLoader.Open(iconUri);

                // Create the tray icon with the .ico file (contains multiple sizes)
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(iconStream),
                    ToolTipText = "ConsoleDeck - Not Connected",
                    IsVisible = true
                };

                // Create the tray menu
                _trayMenu = [];

                // Status item (non-clickable, shows connection status)
                _statusMenuItem = new NativeMenuItem
                {
                    Header = "Not Connected",
                    IsEnabled = false
                };
                _trayMenu.Add(_statusMenuItem);

                // Separator
                _trayMenu.Add(new NativeMenuItemSeparator());

                // Settings menu item
                var settingsMenuItem = new NativeMenuItem
                {
                    Header = "Settings"
                };
                settingsMenuItem.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
                _trayMenu.Add(settingsMenuItem);

                // Exit menu item
                var exitMenuItem = new NativeMenuItem
                {
                    Header = "Exit"
                };
                exitMenuItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
                _trayMenu.Add(exitMenuItem);

                // Attach menu to tray icon
                _trayIcon.Menu = _trayMenu;

                _isInitialized = true;
                logger.LogInformation("System tray initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize system tray");
                throw;
            }
        });
    }

    public void UpdateTooltip(string text)
    {
        if (!_isInitialized || _trayIcon == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            _trayIcon.ToolTipText = text;
        });
    }

    public void ShowNotification(string title, string message, int duration = 3000)
    {
        if (!_isInitialized) return;

        // Log notification (Avalonia 11 TrayIcon doesn't have built-in notifications)
        // TODO: Implement platform-specific notifications (Windows Toast, etc.)
        logger.LogInformation("Notification: {Title} - {Message}", title, message);
    }

    public void UpdateConnectionStatus(string? deviceName = null)
    {
        if (!_isInitialized) return;

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                _statusMenuItem?.Header = deviceName ?? "Not Connected";
                _trayIcon?.ToolTipText = string.IsNullOrEmpty(deviceName) ? "ConsoleDeck - Not Connected" : $"ConsoleDeck - {deviceName}"; ;

                logger.LogInformation("Connection status updated: {Status}", deviceName ?? "Not Connected");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update connection status");
            }
        });
    }

    public async Task ShutdownAsync()
    {
        logger.LogInformation("Shutting down system tray");

        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    if (_trayIcon != null)
                    {
                        _trayIcon.IsVisible = false;
                        _trayIcon.Dispose();
                        _trayIcon = null;
                    }

                    _trayMenu = null;
                    _statusMenuItem = null;
                    _isInitialized = false;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error during tray shutdown");
                }
            });
        }
        catch (TaskCanceledException)
        {
            // Normal behavior on shutdown
        }
    }

    public void Dispose()
    {
        ShutdownAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}
