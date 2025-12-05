using ConsoleDeckService.Core.Interfaces;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.MacOS;

/// <summary>
/// macOS implementation of notification provider (placeholder for future implementation).
/// Will use NSUserNotificationCenter or UNUserNotificationCenter.
/// </summary>
[SupportedOSPlatform("macos")]
public class MacOSNotificationProvider(ILogger<MacOSNotificationProvider> logger) : INotificationProvider
{
    public void ShowNotification(string title, string message, int duration = 1000)
    {
        logger.LogWarning("macOS notifications are not yet implemented");
        logger.LogInformation("Notification: {Title} - {Message}", title, message);
        
        // TODO: Implement using one of:
        // 1. NSUserNotificationCenter (deprecated in macOS 10.14+):
        //    Use AppKit framework via P/Invoke or ObjCRuntime
        //
        // 2. UNUserNotificationCenter (modern, macOS 10.14+):
        //    Use UserNotifications framework
        //    - Request notification permissions
        //    - Create UNMutableNotificationContent
        //    - Schedule notification
        //
        // 3. terminal-notifier (command-line tool):
        //    Process.Start("terminal-notifier", $"-title \"{title}\" -message \"{message}\"");
        //
        // Note: macOS requires notification permissions to be granted by the user
    }
}
