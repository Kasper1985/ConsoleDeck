using ConsoleDeckService.Core.Interfaces;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.MacOS;

/// <summary>
/// macOS implementation of auto-start service using LaunchAgents.
/// Creates/removes plist file in ~/Library/LaunchAgents/
/// </summary>
[SupportedOSPlatform("macos")]
public class MacOSAutoStartService(ILogger<MacOSAutoStartService> logger) : IAutoStartService
{
    private const string LaunchAgentFileName = "com.consoledeck.service.plist";

    public async Task<bool> EnableAutoStartAsync()
    {
        try
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var launchAgentsDir = Path.Combine(homeDir, "Library", "LaunchAgents");
            
            if (!Directory.Exists(launchAgentsDir))
            {
                Directory.CreateDirectory(launchAgentsDir);
                logger.LogInformation("Created LaunchAgents directory: {Path}", launchAgentsDir);
            }

            var plistFilePath = Path.Combine(launchAgentsDir, LaunchAgentFileName);
            var executablePath = GetExecutablePath();

            if (string.IsNullOrEmpty(executablePath))
            {
                logger.LogError("Failed to determine executable path for auto-start");
                return false;
            }

            var plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.consoledeck.service</string>
    <key>ProgramArguments</key>
    <array>
        <string>{executablePath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <false/>
</dict>
</plist>
";

            await File.WriteAllTextAsync(plistFilePath, plistContent);
            logger.LogInformation("Auto-start enabled successfully: {Path}", plistFilePath);

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
            var plistFilePath = Path.Combine(homeDir, "Library", "LaunchAgents", LaunchAgentFileName);

            if (!File.Exists(plistFilePath))
            {
                logger.LogDebug("Auto-start was not enabled, nothing to disable");
                return true;
            }

            File.Delete(plistFilePath);
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
            var plistFilePath = Path.Combine(homeDir, "Library", "LaunchAgents", LaunchAgentFileName);

            var isEnabled = File.Exists(plistFilePath);
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
