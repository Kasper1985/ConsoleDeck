using ConsoleDeckService.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace ConsoleDeckService.Core.Services.Windows;

/// <summary>
/// Windows implementation of notification provider using PowerShell Toast notifications.
/// Falls back to command-line msg.exe if toast notifications fail.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class WindowsNotificationProvider(ILogger<WindowsNotificationProvider> logger) : INotificationProvider
{
    private const string AppId = "ConsoleDeck.Service";
    
    public void ShowNotification(string title, string message, int duration = 3000)
    {
        try
        {
            // Try to show toast notification via PowerShell
            if (TryShowToastNotification(title, message))
            {
                logger.LogDebug("Windows toast notification shown: {Title}", title);
                return;
            }

            // Fallback: Log the notification
            logger.LogInformation("Notification (toast failed): {Title} - {Message}", title, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to show Windows notification: {Title} - {Message}", title, message);
        }
    }

    private bool TryShowToastNotification(string title, string message)
    {
        try
        {
            // Escape strings for XML
            var escapedTitle = EscapeXmlString(title);
            var escapedMessage = EscapeXmlString(message);
            
            // Build the XML template with double quotes to avoid PowerShell single-quote escaping issues
            var xmlTemplate = $"<toast><visual><binding template=\"ToastText02\"><text id=\"1\">{escapedTitle}</text><text id=\"2\">{escapedMessage}</text></binding></visual></toast>";
            
            // Escape the XML template for PowerShell (escape double quotes with backticks)
            var escapedXml = xmlTemplate.Replace("\"", "`\"");
            
            // Build the PowerShell script
            var script = $@"
Add-Type -AssemblyName System.Runtime.WindowsRuntime
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

$APP_ID = '{AppId}'
$template = ""{escapedXml}""

try {{
    $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
    $xml.LoadXml($template)
    $toast = New-Object Windows.UI.Notifications.ToastNotification $xml
    [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($APP_ID).Show($toast)
    Write-Output 'SUCCESS'
}} catch {{
    Write-Error $_.Exception.Message
    exit 1
}}
";

            // Encode the script in Base64 to avoid all escaping issues
            var scriptBytes = Encoding.Unicode.GetBytes(script);
            var encodedScript = Convert.ToBase64String(scriptBytes);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -EncodedCommand {encodedScript}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                logger.LogWarning("Failed to start PowerShell process for notification");
                return false;
            }

            // Read output asynchronously
            var output = new StringBuilder();
            var error = new StringBuilder();
            
            process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // Wait for completion with timeout
            if (!process.WaitForExit(5000))
            {
                logger.LogWarning("PowerShell notification process timed out");
                try { process.Kill(); } catch { /* ignore */ }
                return false;
            }

            var outputText = output.ToString();
            var errorText = error.ToString();

            if (process.ExitCode != 0 || !outputText.Contains("SUCCESS"))
            {
                logger.LogWarning("Toast notification failed. Exit code: {ExitCode}, Output: {Output}, Error: {Error}", 
                    process.ExitCode, outputText, errorText);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Exception while showing toast notification");
            return false;
        }
    }

    private static string EscapeXmlString(string input)
    {
        // Escape special XML characters
        return input
            .Replace("&", "&amp;")   // Must be first
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;")
            .Replace("\n", " ")
            .Replace("\r", "");
    }
}
