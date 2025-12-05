namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for managing the system tray icon and menu.
/// Platform-specific implementations will use appropriate libraries (Avalonia TrayIcon, etc.).
/// </summary>
public interface ISystemTrayProvider
{
    /// <summary>
    /// Event raised when the user requests to exit the application via the tray menu.
    /// </summary>
    event EventHandler? ExitRequested;

    /// <summary>
    /// Event raised when the user requests to open settings via the tray menu.
    /// </summary>
    event EventHandler? SettingsRequested;

    /// <summary>
    /// Event raised when the user requests to view status/logs via the tray menu.
    /// </summary>
    event EventHandler? StatusRequested;

    /// <summary>
    /// Initializes and shows the system tray icon.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Updates the tray icon tooltip text.
    /// </summary>
    /// <param name="text">The tooltip text to display.</param>
    void UpdateTooltip(string text);

    /// <summary>
    /// Shows a notification balloon/popup from the tray icon.
    /// </summary>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message.</param>
    /// <param name="duration">Duration in milliseconds (optional).</param>
    void ShowNotification(string title, string message, int duration = 3000);

    /// <summary>
    /// Updates the connection status displayed in the tray menu.
    /// </summary>
    /// <param name="deviceName">Name of the connected device (optional).</param>
    void UpdateConnectionStatus(string? deviceName = null);

    /// <summary>
    /// Hides and disposes the system tray icon.
    /// </summary>
    Task ShutdownAsync();
}
