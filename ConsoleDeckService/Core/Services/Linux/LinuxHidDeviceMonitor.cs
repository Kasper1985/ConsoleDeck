using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.Services.Linux;

/// <summary>
/// Linux implementation of HID device monitoring (placeholder for future implementation).
/// Will use libusb or similar Linux HID libraries.
/// </summary>
public class LinuxHidDeviceMonitor : IHidDeviceMonitor
{
    private readonly ILogger<LinuxHidDeviceMonitor> _logger;

    public event EventHandler<int>? ConsoleDeckKeyPressed;
    public event EventHandler<string>? DeviceConnected;
    public event EventHandler<string>? DeviceDisconnected;

    public bool IsMonitoring { get; private set; }

    public LinuxHidDeviceMonitor(ILogger<LinuxHidDeviceMonitor> logger)
    {
        _logger = logger;
    }

    public Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Linux HID monitoring is not yet implemented");
        
        // TODO: Implement using libusb or hidapi for Linux
        // - Use /dev/hidraw* devices
        // - Parse input events from evdev
        // - Detect udev device add/remove events
        
        IsMonitoring = true;
        return Task.CompletedTask;
    }

    public Task StopMonitoringAsync()
    {
        IsMonitoring = false;
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetConnectedDevices()
    {
        // TODO: Implement device enumeration for Linux
        // - Read from /sys/class/hidraw/
        // - Parse udev information
        
        return Enumerable.Empty<string>();
    }
}
