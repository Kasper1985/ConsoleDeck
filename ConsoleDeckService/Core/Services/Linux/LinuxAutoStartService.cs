using ConsoleDeckService.Core.Interfaces;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.Linux;

/// <summary>
/// Linux implementation of auto-start service using .desktop files in ~/.config/autostart/
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxAutoStartService(ILogger<LinuxAutoStartService> logger) : IAutoStartService
{
    private const string AppName = "consoledeck-service";
    private const string DesktopFileName = "consoledeck-service.desktop";

    public async Task<bool> EnableAutoStartAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var autostartDir = Path.Combine(homeDir, ".config", "autostart");
            
            if (!Directory.Exists(autostartDir))
            {
                Directory.CreateDirectory(autostartDir);
                logger.LogInformation("Created autostart directory: {Path}", autostartDir);
            }

            var desktopFilePath = Path.Combine(autostartDir, DesktopFileName);
            var executablePath = GetExecutablePath();

            if (string.IsNullOrEmpty(executablePath))
            {
                logger.LogError("Failed to determine executable path for auto-start");
                return false;
            }

            var desktopFileContent = $@"[Desktop Entry]
Type=Application
Version=1.0
Name=ConsoleDeck Service
Comment=ConsoleDeck HID Device Service
Exec={executablePath}
Icon=consoledeck
Terminal=false
Categories=Utility;
StartupNotify=false
X-GNOME-Autostart-enabled=true
";

            await File.WriteAllTextAsync(desktopFilePath, desktopFileContent);
            logger.LogInformation("Auto-start enabled successfully: {Path}", desktopFilePath);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enable auto-start");
            return false;
        }
    }

    public async Task<bool> DisableAutoStartAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var desktopFilePath = Path.Combine(homeDir, ".config", "autostart", DesktopFileName);

            if (!File.Exists(desktopFilePath))
            {
                logger.LogDebug("Auto-start was not enabled, nothing to disable");
                return true;
            }

            File.Delete(desktopFilePath);
            logger.LogInformation("Auto-start disabled successfully");

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to disable auto-start");
            return false;
        }
    }

    public async Task<bool> IsAutoStartEnabledAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var desktopFilePath = Path.Combine(homeDir, ".config", "autostart", DesktopFileName);

            var isEnabled = File.Exists(desktopFilePath);
            logger.LogDebug("Auto-start status: {Status}", isEnabled ? "Enabled" : "Disabled");

            return await Task.FromResult(isEnabled);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check auto-start status");
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var mainModule = process.MainModule;
        
        if (mainModule?.FileName == null)
            return string.Empty;

        return mainModule.FileName;
    }
}
