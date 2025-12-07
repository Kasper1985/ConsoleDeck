using ConsoleDeckService.Core.Interfaces;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ConsoleDeckService.Core.Services.Windows;

/// <summary>
/// Windows implementation of auto-start service using Registry Run key.
/// Adds/removes application entry in HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsAutoStartService(ILogger<WindowsAutoStartService> logger) : IAutoStartService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ConsoleDeckService";

    public async Task<bool> EnableAutoStartAsync()
    {
        try
        {
            var executablePath = GetExecutablePath();
            if (string.IsNullOrEmpty(executablePath))
            {
                logger.LogError("Failed to determine executable path for auto-start");
                return false;
            }

            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
            if (key == null)
            {
                logger.LogError("Failed to open registry key for auto-start: {KeyPath}", RegistryKeyPath);
                return false;
            }

            key.SetValue(AppName, $"\"{executablePath}\"", RegistryValueKind.String);
            logger.LogInformation("Auto-start enabled successfully: {Path}", executablePath);

            return await Task.FromResult(true);
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
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
            if (key == null)
            {
                logger.LogError("Failed to open registry key for auto-start: {KeyPath}", RegistryKeyPath);
                return false;
            }

            var currentValue = key.GetValue(AppName);
            if (currentValue == null)
            {
                logger.LogDebug("Auto-start was not enabled, nothing to disable");
                return await Task.FromResult(true);
            }

            key.DeleteValue(AppName, throwOnMissingValue: false);
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
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
            if (key == null)
            {
                logger.LogWarning("Failed to open registry key for auto-start check: {KeyPath}", RegistryKeyPath);
                return false;
            }

            var value = key.GetValue(AppName);
            var isEnabled = value != null;

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
        var process = Process.GetCurrentProcess();
        var mainModule = process.MainModule;
        
        if (mainModule?.FileName == null)
            return string.Empty;

        return mainModule.FileName;
    }
}
