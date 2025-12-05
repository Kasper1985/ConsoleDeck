namespace ConsoleDeckService.Core.Models;

/// <summary>
/// Root configuration for the ConsoleDeck service.
/// Contains all key mappings and HID device settings.
/// </summary>
public class ConsoleDeckConfiguration
{
    /// <summary>
    /// List of key-to-action mappings.
    /// Each mapping corresponds to one of the 9 ConsoleDeck buttons (F13-F21).
    /// </summary>
    public List<KeyActionMapping> KeyMappings { get; set; } = [];

    /// <summary>
    /// HID device vendor ID to monitor (optional, for filtering specific devices).
    /// Example: 0x2E8A for Raspberry Pi Pico
    /// </summary>
    public int? VendorId { get; set; }

    /// <summary>
    /// HID device product ID to monitor (optional, for filtering specific devices).
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Debounce time in milliseconds to prevent double-triggering.
    /// Default: 200ms
    /// </summary>
    public int DebounceMs { get; set; } = 200;

    /// <summary>
    /// Enable verbose logging for debugging.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Automatically start with Windows (Windows only).
    /// </summary>
    public bool AutoStart { get; set; } = false;

    /// <summary>
    /// Show system tray notifications when actions are executed.
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Gets a mapping by key code.
    /// </summary>
    public KeyActionMapping? GetMapping(int keyCode)
    {
        return KeyMappings.FirstOrDefault(m => m.KeyCode == keyCode);
    }
}
