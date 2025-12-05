using ConsoleDeckService.Core.Interfaces;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.Linux;

/// <summary>
/// Linux implementation of notification provider (placeholder for future implementation).
/// Will use libnotify (notify-send) or D-Bus notifications.
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxNotificationProvider(ILogger<LinuxNotificationProvider> logger) : INotificationProvider
{
    public void ShowNotification(string title, string message, int duration = 3000)
    {
        logger.LogWarning("Linux notifications are not yet implemented");
        logger.LogInformation("Notification: {Title} - {Message}", title, message);
        
        // TODO: Implement using one of:
        // 1. libnotify (notify-send command-line tool):
        //    Process.Start("notify-send", $"\"{title}\" \"{message}\" -t {duration}");
        //
        // 2. D-Bus notifications (org.freedesktop.Notifications):
        //    Use Tmds.DBus package to send notifications via D-Bus
        //
        // 3. Desktop-specific implementations:
        //    - GNOME: org.gnome.Shell.Notification
        //    - KDE: org.kde.plasma.notifications
    }
}
