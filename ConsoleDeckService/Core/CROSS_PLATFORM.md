# ConsoleDeck Cross-Platform Support

## Current Status

### ? Windows (Fully Implemented)
- **HID Monitoring**: `WindowsHidDeviceMonitor` using HidSharp
- **System Tray**: Avalonia TrayIcon (cross-platform ready)
- **Action Executor**: Fully cross-platform
- **Status**: Production ready

### ?? Linux (Planned)
- **HID Monitoring**: `LinuxHidDeviceMonitor` (placeholder)
- **System Tray**: Avalonia TrayIcon (already works)
- **Action Executor**: Already supports Linux
- **Status**: Framework ready, HID implementation needed

#### Linux Implementation Notes:
```bash
# Required libraries
sudo apt-get install libhidapi-dev libudev-dev

# HID device access (add user to input group)
sudo usermod -a -G input $USER

# Device permissions (create udev rule)
echo 'KERNEL=="hidraw*", ATTRS{idVendor}=="2e8a", MODE="0666"' | \
  sudo tee /etc/udev/rules.d/99-consoledeck.rules
sudo udevadm control --reload-rules
```

**Libraries to use:**
- `libhidapi` or `hidapi-libusb` for HID communication
- `/dev/hidraw*` device files
- `udev` for device hotplug detection

### ?? macOS (Planned)
- **HID Monitoring**: `MacOSHidDeviceMonitor` (placeholder)
- **System Tray**: Avalonia TrayIcon (already works)
- **Action Executor**: Already supports macOS
- **Status**: Framework ready, HID implementation needed

#### macOS Implementation Notes:
**Frameworks to use:**
- `IOKit` framework for HID device access
- `IOHIDManager` for device discovery and input handling
- `NSStatusItem` (Avalonia handles this)

**Permissions:**
- Request Input Monitoring permission in System Preferences

## Architecture

The ConsoleDeck service is designed with platform abstraction:

```
???????????????????????????????????????????
?         Worker Service                   ?
?  (BackgroundService - cross-platform)   ?
???????????????????????????????????????????
                    ?
    ?????????????????????????????????
    ?               ?               ?
??????????    ????????????   ????????????
? Config ?    ?   Tray   ?   ? Actions  ?
?Service ?    ?(Avalonia)?   ? Executor ?
??????????    ????????????   ????????????
                    ?
            Platform-specific
    ?????????????????????????????????
    ?               ?               ?
?????????????  ????????????  ?????????????
?  Windows  ?  ?  Linux   ?  ?   macOS   ?
?    HID    ?  ?   HID    ?  ?    HID    ?
?????????????  ????????????  ?????????????
```

## Adding Platform Support

To add a new platform:

1. **Implement `IHidDeviceMonitor`** for your platform
2. **Update `Program.cs`** to register your implementation:
   ```csharp
   if (OperatingSystem.IsLinux())
   {
       builder.Services.AddSingleton<IHidDeviceMonitor, LinuxHidDeviceMonitor>();
   }
   ```
3. **Test HID communication** with your ConsoleDeck device
4. **Verify** key combinations are detected correctly
5. **Update** this README with implementation details

## Testing

### Manual Testing
1. Connect ConsoleDeck device via USB
2. Run the service
3. Press RCtrl+RShift+F13 through F21
4. Verify actions execute correctly

### HID Raw Data Testing
```bash
# Linux - monitor raw HID events
sudo cat /dev/hidraw0 | xxd

# macOS - use hidapi test tool
brew install hidapi
hidtest
```

## Contributing

When implementing platform support:
- Follow the existing interface contracts
- Use platform-native HID libraries when possible
- Add comprehensive logging for debugging
- Document any special permissions/setup required
- Update this README with your findings

## Resources

### HID Specifications
- [USB HID Usage Tables](https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf)
- [HID Keyboard Scan Codes](https://www.win.tue.nl/~aeb/linux/kbd/scancodes-14.html)

### Libraries
- **HidSharp**: Cross-platform (current, works on Linux/macOS too!)
- **hidapi**: C library with .NET bindings
- **IOKit**: macOS native framework
- **libusb**: Linux USB library

### ConsoleDeck Hardware
- **Device**: Raspberry Pi Pico (VID: 0x2E8A)
- **Interface**: USB HID Keyboard
- **Keys**: F13-F21 with RCtrl+RShift modifiers
