namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for managing application auto-start on system startup.
/// Platform-specific implementations handle OS-specific startup mechanisms.
/// </summary>
public interface IAutoStartService
{
    /// <summary>
    /// Enables the application to start automatically on system startup.
    /// </summary>
    /// <returns>True if auto-start was enabled successfully, false otherwise.</returns>
    Task<bool> EnableAutoStartAsync();

    /// <summary>
    /// Disables the application from starting automatically on system startup.
    /// </summary>
    /// <returns>True if auto-start was disabled successfully, false otherwise.</returns>
    Task<bool> DisableAutoStartAsync();

    /// <summary>
    /// Checks if auto-start is currently enabled.
    /// </summary>
    /// <returns>True if auto-start is enabled, false otherwise.</returns>
    Task<bool> IsAutoStartEnabledAsync();
}
