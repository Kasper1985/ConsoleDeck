using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for executing actions triggered by key combinations.
/// Handles launching apps, opening URLs, executing scripts, etc.
/// </summary>
public interface IActionExecutor
{
    /// <summary>
    /// Executes an action definition.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the action executed successfully, false otherwise.</returns>
    Task<bool> ExecuteAsync(ActionDefinition action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if an action can be executed (e.g., file exists, URL is valid).
    /// </summary>
    /// <param name="action">The action to validate.</param>
    /// <returns>Validation result with any error messages.</returns>
    Task<ActionValidationResult> ValidateAsync(ActionDefinition action);

    /// <summary>
    /// Gets the current operating system type.
    /// </summary>
    OperatingSystemType GetOperatingSystem();
}

/// <summary>
/// Result of action validation.
/// </summary>
public class ActionValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static ActionValidationResult Success() => new() { IsValid = true };
    public static ActionValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
}

/// <summary>
/// Operating system types for cross-platform action execution.
/// </summary>
public enum OperatingSystemType
{
    Windows,
    Linux,
    MacOS,
    Unknown
}
