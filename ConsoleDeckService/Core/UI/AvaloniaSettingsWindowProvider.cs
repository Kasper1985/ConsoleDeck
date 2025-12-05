using Avalonia.Threading;
using ConsoleDeckService.Core.Interfaces;

namespace ConsoleDeckService.Core.UI;

/// <summary>
/// Avalonia-based settings window provider.
/// Manages the lifecycle of the settings window.
/// </summary>
public class AvaloniaSettingsWindowProvider(IConfigurationService configService, ILogger<SettingsWindow> windowLogger, ILogger<AvaloniaSettingsWindowProvider> logger) : ISettingsWindowProvider
{
    private SettingsWindow? _settingsWindow;

    public async Task OpenSettingsWindowAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                if (_settingsWindow != null)
                {
                    // Window already exists, bring to front
                    logger.LogDebug("Settings window already open, activating");
                    _settingsWindow.Activate();
                    return;
                }

                // Create new settings window
                logger.LogInformation("Opening settings window");
                _settingsWindow = new SettingsWindow(configService, windowLogger);

                // Handle window closed event
                _settingsWindow.Closed += (s, e) =>
                {
                    logger.LogDebug("Settings window closed");
                    _settingsWindow = null;
                };

                _settingsWindow.Show();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open settings window");
            }
        });
    }

    public async Task CloseSettingsWindowAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                if (_settingsWindow != null)
                {
                    logger.LogInformation("Closing settings window");
                    _settingsWindow.Close();
                    _settingsWindow = null;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to close settings window");
            }
        });
    }
}
