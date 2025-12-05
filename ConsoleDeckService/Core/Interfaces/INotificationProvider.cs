namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for platform-specific notification providers.
/// Implementations will use appropriate notification systems (Windows Toast, Linux libnotify, macOS NSUserNotificationCenter).
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Shows a notification to the user.
    /// </summary>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message.</param>
    /// <param name="duration">Duration in milliseconds (may be ignored by some platforms)</param>
    void ShowNotification(string title, string message, int duration = 1000);
}
