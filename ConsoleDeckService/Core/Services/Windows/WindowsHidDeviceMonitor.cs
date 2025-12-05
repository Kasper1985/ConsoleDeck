using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.Models;
using HidSharp;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace ConsoleDeckService.Core.Services.Windows;

/// <summary>
/// Windows implementation of HID device monitoring using HidSharp.
/// Detects ConsoleDeck devices and monitors for RCtrl+RShift+F13-F21 key combinations.
/// </summary>
public class WindowsHidDeviceMonitor(ILogger<WindowsHidDeviceMonitor> logger, IConfigurationService configService) : IHidDeviceMonitor
{
    private int? _vendorId = 0xCAFE; // Default Vendor ID
    private int? _productId;
    private int _debounceMs = 200; // Default debounce time in milliseconds

    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;
    private readonly Dictionary<int, DateTime> _lastKeyPressTime = [];
    private HidDevice? _currentDevice;
    private HidStream? _currentStream;

    public event EventHandler<int>? ConsoleDeckKeyPressed;
    public event EventHandler<string>? DeviceConnected;
    public event EventHandler<string>? DeviceDisconnected;

    public bool IsMonitoring { get; private set; }


    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        if (IsMonitoring)
        {
            logger.LogWarning("HID monitoring is already running");
            return;
        }

        _vendorId = configService.Configuration.VendorId ?? _vendorId;
        _productId = configService.Configuration.ProductId ?? _productId;
        _debounceMs = configService.Configuration.DebounceMs;

        logger.LogInformation("Starting HID device monitoring (VendorId: 0x{VendorId:X4})", _vendorId);
        
        _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IsMonitoring = true;

        _monitoringTask = Task.Run(async () => await MonitorDevicesAsync(_monitoringCts.Token), _monitoringCts.Token);
        
        await Task.CompletedTask;
    }

    public async Task StopMonitoringAsync()
    {
        if (!IsMonitoring) return;

        logger.LogInformation("Stopping HID device monitoring");
        
        IsMonitoring = false;
        _monitoringCts?.Cancel();

        if (_monitoringTask != null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        CloseCurrentDevice();
        _monitoringCts?.Dispose();
        _monitoringCts = null;
    }

    public IEnumerable<string> GetConnectedDevices()
    {
        IList<HidDevice> devices = [];
        
        // Log all HID devices for debugging
        var sb = new StringBuilder();
        foreach (var d in DeviceList.Local.GetHidDevices())
        {
            if (devices.Any(x => x.VendorID == d.VendorID && x.ProductID == d.ProductID)) continue;

            devices.Add(d);
            sb.AppendLine($"  - VID: 0x{d.VendorID:X4}, PID: 0x{d.ProductID:X4} - {d.GetManufacturer()} {d.GetProductName()}");
        }
        logger.LogDebug("Enumerating all HID devices: {DeviceList}", sb.ToString());

        return [.. devices
            .Where(d => d.VendorID == _vendorId && (_productId == null || d.ProductID == _productId))
            .Select(d => $"{d.GetManufacturer()} {d.GetProductName()} (VID: 0x{d.VendorID:X4}, PID: 0x{d.ProductID:X4})")];
    }

    private async Task MonitorDevicesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Try to find and connect to a ConsoleDeck device
                if (_currentDevice == null)
                {
                    var device = FindConsoleDeckDevice();
                    if (device != null)
                    {
                        await ConnectToDeviceAsync(device);
                    }
                }

                // If connected, read input
                if (_currentStream != null)
                {
                    await ReadDeviceInputAsync(cancellationToken);
                }
                else
                {
                    // Wait before retrying connection
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in HID monitoring loop");
                CloseCurrentDevice();
                await Task.Delay(2000, cancellationToken);
            }
        }
    }

    private HidDevice? FindConsoleDeckDevice()
    {
        var devices = DeviceList.Local.GetHidDevices();
        
        var device = devices.FirstOrDefault(d => 
            d.VendorID == _vendorId && 
            (_productId == null || d.ProductID == _productId));

        if (device != null)
        {
            logger.LogInformation("Found matching device: VID=0x{VendorId:X4}, PID=0x{ProductId:X4}, Max Input Report: {MaxInput} bytes", 
                device.VendorID, device.ProductID, device.GetMaxInputReportLength());
        }

        return device;
    }

    private async Task ConnectToDeviceAsync(HidDevice device)
    {
        try
        {
            _currentDevice = device;
            _currentStream = device.Open();
            _currentStream.ReadTimeout = 1000; // 1 second timeout
            
            var deviceFullName = $"{device.GetManufacturer()} {device.GetProductName()}";
            logger.LogInformation("Connected to ConsoleDeck device: {DeviceName}", deviceFullName);
            logger.LogInformation("Device details: Max Input={MaxInput}, Max Output={MaxOutput}, Max Feature={MaxFeature}",
                device.GetMaxInputReportLength(), device.GetMaxOutputReportLength(), device.GetMaxFeatureReportLength());
            
            DeviceConnected?.Invoke(this, device.GetProductName());
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to HID device");
            CloseCurrentDevice();
        }
    }

    private async Task ReadDeviceInputAsync(CancellationToken cancellationToken)
    {
        if (_currentStream == null) return;

        try
        {
            var buffer = new byte[_currentDevice!.GetMaxInputReportLength()];
            
            // Read with timeout
            var bytesRead = await _currentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            if (bytesRead > 0)
            {
                // Log RAW HID data for debugging
                var hex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " ");
                logger.LogDebug("RAW HID Report ({BytesRead} bytes): {Hex}", bytesRead, hex);
                
                ProcessHidReport(buffer, bytesRead);
            }
        }
        catch (TimeoutException)
        {
            // Normal timeout, continue
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error reading from HID device");
            CloseCurrentDevice();
            
            var deviceName = _currentDevice?.GetProductName() ?? "Unknown";
            DeviceDisconnected?.Invoke(this, deviceName);
        }
    }

    private void ProcessHidReport(byte[] buffer, int length)
    {
        if (length < 3)
        {
            logger.LogWarning("HID report too short: {Length} bytes", length);
            return;
        }

        if (length > 3)
        {
            logger.LogWarning("Unexpected HID report length: {Length} bytes", length);
            return;
        }

        // HID Keyboard report format (standard):
        // HID device reports key in Little Endian format
        // Byte 0: Report ID. From device specification: REPORT_ID_CONSUMER_CONTROL = 2
        // Byte 1: Keycode of the pressed key
        // Byte 2: Reserved (FF - for key press, 00 - for key release)
        var code = buffer[0];
        logger.LogDebug("Processing HID report with Report ID: 0x{ReportId:X2}...", code);

        code = buffer[2];
        if (code == 0x00)
        {
            logger.LogDebug("Key release detected, ignoring.");
            return;
        }

        // Check key bytes
        code = buffer[1];
        logger.LogInformation("Key pressed - Scan code: 0x{KeyCode:X2} ({Decimal}).", code, code);

        if (code < 0xF1 || code > 0xF9)
        {
            logger.LogWarning("Key code 0x{KeyCode:X2} ({Decimal}) is not expected...", code, code);
            return;
        }

        // Apply debouncing
        if (IsDebounced(code))
        {
            logger.LogDebug("Key with code 0x{KeyCode:x2} ({Decimal}) debounced (too soon after last press).", code, code);
            return;
        }

        // Store the last press time for current key
        _lastKeyPressTime[code] = DateTime.UtcNow;

        // Call respective action to the pressed key
        ConsoleDeckKeyPressed?.Invoke(this, code);
    }

    private bool IsDebounced(int code)
    {
        if (_lastKeyPressTime.TryGetValue(code, out var lastPress))
        {
            var elapsed = (DateTime.UtcNow - lastPress).TotalMilliseconds;
            return elapsed < _debounceMs;
        }
        return false;
    }

    private void CloseCurrentDevice()
    {
        try
        {
            _currentStream?.Close();
            _currentStream?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error closing HID stream");
        }
        finally
        {
            _currentStream = null;
            _currentDevice = null;
        }
    }
}
