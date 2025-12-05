using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.Interfaces;

/// <summary>
/// Interface for monitoring HID devices and detecting key combinations.
/// Platform-specific implementations will handle Windows/Linux/macOS differences.
/// </summary>
public interface IHidDeviceMonitor
{
    /// <summary>
    /// Event raised when a valid ConsoleDeck key is detected.
    /// </summary>
    event EventHandler<int>? ConsoleDeckKeyPressed;

    /// <summary>
    /// Event raised when a ConsoleDeck device is connected.
    /// </summary>
    event EventHandler<string>? DeviceConnected;

    /// <summary>
    /// Event raised when a ConsoleDeck device is disconnected.
    /// </summary>
    event EventHandler<string>? DeviceDisconnected;

    /// <summary>
    /// Starts monitoring for HID device input.
    /// </summary>
    /// <param name="cancellationToken">Token to stop monitoring.</param>
    Task StartMonitoringAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops monitoring HID device input.
    /// </summary>
    Task StopMonitoringAsync();

    /// <summary>
    /// Gets the list of currently connected ConsoleDeck devices.
    /// </summary>
    IEnumerable<string> GetConnectedDevices();

    /// <summary>
    /// Checks if the monitor is currently running.
    /// </summary>
    bool IsMonitoring { get; }
}
