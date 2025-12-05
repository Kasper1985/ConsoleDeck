namespace ConsoleDeckService.Core.Models;

/// <summary>
/// Maps a specific key combination to an action.
/// For ConsoleDeck: Maps F13-F21 (with Right Ctrl + Right Shift) to actions.
/// </summary>
public class KeyActionMapping
{
    /// <summary>
    /// The function key number (13-21 for ConsoleDeck).
    /// This corresponds to F13 through F21.
    /// </summary>
    public int KeyCode { get; set; }

    /// <summary>
    /// The action to perform when this key combination is detected.
    /// </summary>
    public ActionDefinition Action { get; set; } = new();

    /// <summary>
    /// Gets the full key combination string for display.
    /// Example: "RCtrl+RShift+F13"
    /// </summary>
    public string GetDisplayName()
    {
        return $"0xFF{KeyCode:X2}";
    }

    public override string ToString() => $"{GetDisplayName()} -> {Action.Name}";
}
