using ConsoleDeckService.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.MacOS;

/// <summary>
/// macOS implementation of notification provider using osascript.
/// Displays notifications via the macOS Notification Center.
/// </summary>
[SupportedOSPlatform("macos")]
public class MacOSNotificationProvider(ILogger<MacOSNotificationProvider> logger, IConfigurationService configurationService) : INotificationProvider
{
    public void ShowNotification(string title, string message, int duration = 1000)
    {
        try
        {
            if (!configurationService.Configuration.ShowNotifications)
            {
                logger.LogDebug("Notifications are disabled in configuration");
                return;
            }

            if (TryShowOsascriptNotification(title, message))
            {
                logger.LogDebug("macOS notification shown: {Title}", title);
                return;
            }

            logger.LogInformation("Notification (osascript failed): {Title} - {Message}", title, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to show macOS notification: {Title} - {Message}", title, message);
        }
    }

    private bool TryShowOsascriptNotification(string title, string message)
    {
        try
        {
            // Escape backslashes and double-quotes for AppleScript string literals
            var safeTitle = title.Replace("\\", "\\\\").Replace("\"", "\\\"");
            var safeMessage = message.Replace("\\", "\\\\").Replace("\"", "\\\"");

            var script = $"display notification \"{safeMessage}\" with title \"{safeTitle}\"";

            var psi = new ProcessStartInfo
            {
                FileName = "osascript",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.ArgumentList.Add("-e");
            psi.ArgumentList.Add(script);

            using var process = Process.Start(psi);
            if (process == null)
            {
                logger.LogWarning("Failed to start osascript process for notification");
                return false;
            }

            if (!process.WaitForExit(5000))
            {
                logger.LogWarning("osascript notification process timed out");
                try { process.Kill(); } catch { /* ignore */ }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Exception while showing osascript notification");
            return false;
        }
    }
}
