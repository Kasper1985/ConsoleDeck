using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for loading and managing ConsoleDeck configuration.
/// Supports hot-reload when configuration files change.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Event raised when the configuration is reloaded.
    /// </summary>
    event EventHandler<ConsoleDeckConfiguration>? ConfigurationReloaded;

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    ConsoleDeckConfiguration Configuration { get; }

    /// <summary>
    /// Loads the configuration from the configuration file.
    /// </summary>
    Task LoadConfigurationAsync();

    /// <summary>
    /// Saves the current configuration to the configuration file.
    /// </summary>
    Task SaveConfigurationAsync();

    /// <summary>
    /// Reloads the configuration from the configuration file.
    /// </summary>
    Task ReloadConfigurationAsync();

    /// <summary>
    /// Gets an action for a specific function key number (13-21).
    /// </summary>
    /// <param name="functionKeyNumber">The function key number (13-21).</param>
    /// <returns>The action definition, or null if not mapped.</returns>
    ActionDefinition? GetActionForKey(int keyCode);

    /// <summary>
    /// Validates the current configuration.
    /// </summary>
    /// <returns>List of validation errors (empty if valid).</returns>
    Task<IEnumerable<string>> ValidateConfigurationAsync();
}
