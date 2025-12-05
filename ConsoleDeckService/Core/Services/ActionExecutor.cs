using ConsoleDeckService.Core.Models;
using ConsoleDeckService.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleDeckService.Core.Services;

/// <summary>
/// Cross-platform action executor for ConsoleDeck actions.
/// Handles launching applications, opening URLs, and executing scripts.
/// </summary>
public class ActionExecutor : IActionExecutor
{
    private readonly ILogger<ActionExecutor> _logger;
    private readonly OperatingSystemType _osType;

    public ActionExecutor(ILogger<ActionExecutor> logger)
    {
        _logger = logger;
        _osType = DetectOperatingSystem();
        
        _logger.LogInformation("ActionExecutor initialized for OS: {OS}", _osType);
    }

    public OperatingSystemType GetOperatingSystem() => _osType;

    public async Task<bool> ExecuteAsync(ActionDefinition action, CancellationToken cancellationToken = default)
    {
        if (!action.Enabled)
        {
            _logger.LogDebug("Action {ActionName} is disabled, skipping execution", action.Name);
            return false;
        }

        _logger.LogInformation("Executing action: {ActionName} ({Type}: {Target})", 
            action.Name, action.Type, action.Target);

        try
        {
            return action.Type switch
            {
                ActionType.LaunchApplication => await LaunchApplicationAsync(action, cancellationToken),
                ActionType.OpenUrl => await OpenUrlAsync(action, cancellationToken),
                ActionType.ExecuteScript => await ExecuteScriptAsync(action, cancellationToken),
                ActionType.SendKeystrokes => await SendKeystrokesAsync(action, cancellationToken),
                ActionType.None => true,
                _ => throw new NotSupportedException($"Action type {action.Type} is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute action: {ActionName}", action.Name);
            return false;
        }
    }

    public async Task<ActionValidationResult> ValidateAsync(ActionDefinition action)
    {
        return action.Type switch
        {
            ActionType.LaunchApplication => await ValidateLaunchApplicationAsync(action),
            ActionType.OpenUrl => await ValidateOpenUrlAsync(action),
            ActionType.ExecuteScript => await ValidateExecuteScriptAsync(action),
            ActionType.SendKeystrokes => await ValidateSendKeystrokesAsync(action),
            ActionType.None => ActionValidationResult.Success(),
            _ => ActionValidationResult.Failure($"Unknown action type: {action.Type}")
        };
    }

    #region Launch Application

    private static async Task<bool> LaunchApplicationAsync(ActionDefinition action, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = action.Target,
            Arguments = action.Arguments ?? string.Empty,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        if (!string.IsNullOrEmpty(action.WorkingDirectory))
        {
            startInfo.WorkingDirectory = action.WorkingDirectory;
        }

        var process = Process.Start(startInfo);
        await Task.CompletedTask;
        
        return process != null;
    }

    private async Task<ActionValidationResult> ValidateLaunchApplicationAsync(ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            return ActionValidationResult.Failure("Application path is required");
        }

        // Check if it's a system command (like notepad, calc, etc.)
        if (!Path.IsPathRooted(action.Target))
        {
            // System command, assume it's valid
            return await Task.FromResult(ActionValidationResult.Success());
        }

        // Check if file exists
        if (!File.Exists(action.Target))
        {
            return ActionValidationResult.Failure($"Application not found: {action.Target}");
        }

        // Check working directory if specified
        if (!string.IsNullOrEmpty(action.WorkingDirectory) && !Directory.Exists(action.WorkingDirectory))
        {
            return ActionValidationResult.Failure($"Working directory not found: {action.WorkingDirectory}");
        }

        return ActionValidationResult.Success();
    }

    #endregion

    #region Open URL

    private async Task<bool> OpenUrlAsync(ActionDefinition action, CancellationToken cancellationToken)
    {
        var url = action.Target;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            // Platform-specific URL opening
            if (_osType == OperatingSystemType.Linux)
            {
                startInfo.FileName = "xdg-open";
                startInfo.Arguments = url;
            }
            else if (_osType == OperatingSystemType.MacOS)
            {
                startInfo.FileName = "open";
                startInfo.Arguments = url;
            }

            var process = Process.Start(startInfo);
            await Task.CompletedTask;
            
            return process != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open URL: {Url}", url);
            return false;
        }
    }

    private async Task<ActionValidationResult> ValidateOpenUrlAsync(ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            return ActionValidationResult.Failure("URL is required");
        }

        if (!Uri.TryCreate(action.Target, UriKind.Absolute, out var uri))
        {
            return ActionValidationResult.Failure($"Invalid URL format: {action.Target}");
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ActionValidationResult.Failure($"Only HTTP and HTTPS URLs are supported: {action.Target}");
        }

        return await Task.FromResult(ActionValidationResult.Success());
    }

    #endregion

    #region Execute Script

    private async Task<bool> ExecuteScriptAsync(ActionDefinition action, CancellationToken cancellationToken)
    {
        var scriptPath = action.Target;

        if (!File.Exists(scriptPath))
        {
            _logger.LogError("Script file not found: {ScriptPath}", scriptPath);
            return false;
        }

        var extension = Path.GetExtension(scriptPath).ToLowerInvariant();
        
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Determine script executor based on extension and OS
        switch (extension)
        {
            case ".ps1" when _osType == OperatingSystemType.Windows:
                startInfo.FileName = "powershell.exe";
                startInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {action.Arguments}";
                break;

            case ".bat" or ".cmd" when _osType == OperatingSystemType.Windows:
                startInfo.FileName = scriptPath;
                startInfo.Arguments = action.Arguments ?? string.Empty;
                break;

            case ".sh" when _osType is OperatingSystemType.Linux or OperatingSystemType.MacOS:
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = $"\"{scriptPath}\" {action.Arguments}";
                break;

            case ".py":
                startInfo.FileName = "python";
                startInfo.Arguments = $"\"{scriptPath}\" {action.Arguments}";
                break;

            default:
                _logger.LogError("Unsupported script type: {Extension}", extension);
                return false;
        }

        if (!string.IsNullOrEmpty(action.WorkingDirectory))
        {
            startInfo.WorkingDirectory = action.WorkingDirectory;
        }

        try
        {
            var process = Process.Start(startInfo);
            if (process == null) return false;

            // Log output asynchronously
            _ = Task.Run(async () =>
            {
                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogDebug("Script output: {Output}", output);
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogWarning("Script error: {Error}", error);
                }
            }, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute script: {ScriptPath}", scriptPath);
            return false;
        }
    }

    private async Task<ActionValidationResult> ValidateExecuteScriptAsync(ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            return ActionValidationResult.Failure("Script path is required");
        }

        if (!File.Exists(action.Target))
        {
            return ActionValidationResult.Failure($"Script file not found: {action.Target}");
        }

        var extension = Path.GetExtension(action.Target).ToLowerInvariant();
        var supportedExtensions = _osType switch
        {
            OperatingSystemType.Windows => new[] { ".ps1", ".bat", ".cmd", ".py" },
            OperatingSystemType.Linux or OperatingSystemType.MacOS => new[] { ".sh", ".py" },
            _ => Array.Empty<string>()
        };

        if (!supportedExtensions.Contains(extension))
        {
            return ActionValidationResult.Failure(
                $"Unsupported script type '{extension}' for {_osType}. " +
                $"Supported: {string.Join(", ", supportedExtensions)}");
        }

        if (!string.IsNullOrEmpty(action.WorkingDirectory) && !Directory.Exists(action.WorkingDirectory))
        {
            return ActionValidationResult.Failure($"Working directory not found: {action.WorkingDirectory}");
        }

        return await Task.FromResult(ActionValidationResult.Success());
    }

    #endregion

    #region Send Keystrokes

    private async Task<bool> SendKeystrokesAsync(ActionDefinition action, CancellationToken cancellationToken)
    {
        _logger.LogWarning("SendKeystrokes action is not yet implemented");
        await Task.CompletedTask;
        return false;
    }

    private async Task<ActionValidationResult> ValidateSendKeystrokesAsync(ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            return ActionValidationResult.Failure("Keystroke sequence is required");
        }

        // TODO: Implement keystroke validation
        return await Task.FromResult(ActionValidationResult.Success());
    }

    #endregion

    #region OS Detection

    private static OperatingSystemType DetectOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OperatingSystemType.Windows;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OperatingSystemType.Linux;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OperatingSystemType.MacOS;

        return OperatingSystemType.Unknown;
    }

    #endregion
}
