namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for managing the settings window.
/// Abstracts platform-specific window management.
/// </summary>
public interface ISettingsWindowProvider
{
    /// <summary>
    /// Opens the settings window. If already open, brings it to front.
    /// This method handles UI thread marshalling internally.
    /// </summary>
    Task OpenSettingsWindowAsync();

    /// <summary>
    /// Closes the settings window if it is open.
    /// </summary>
    Task CloseSettingsWindowAsync();
}
