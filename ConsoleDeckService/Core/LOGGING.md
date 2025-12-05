# ConsoleDeck Logging and Error Handling

## Logging Configuration

ConsoleDeck uses Microsoft.Extensions.Logging for comprehensive logging throughout the application.

### Log Levels

Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "ConsoleDeckService": "Debug"
    }
  }
}
```

**Log Levels:**
- `Trace` - Very detailed logs, use for deep debugging
- `Debug` - Detailed information for debugging
- `Information` - General informational messages (default)
- `Warning` - Warnings that don't stop execution
- `Error` - Errors that stopped an operation
- `Critical` - Critical failures requiring immediate attention

### Verbose Logging

Enable verbose HID and action logging:

```json
{
  "ConsoleDeck": {
    "verboseLogging": true
  }
}
```

This logs:
- HID raw data packets
- Detailed key combination detection
- Action execution steps
- Configuration validation details

## What Gets Logged

### Startup
- Configuration loading and validation
- Service initialization (Tray, HID Monitor, Message Processor)
- Avalonia UI initialization
- Connected device detection

### Runtime
- HID device connection/disconnection events
- Key combination detection (F13-F21 presses)
- Action execution (success/failure)
- Configuration hot-reload events
- System tray menu interactions

### Errors
- HID communication failures
- Action execution errors
- Configuration validation errors
- Platform compatibility issues

## Log Locations

### Console Output
When running from command line:
```bash
dotnet run
```
Logs appear in the console with timestamps and log levels.

### Windows Event Viewer
When running as a Windows Service:
1. Open Event Viewer (`eventvwr.msc`)
2. Navigate to: `Windows Logs` > `Application`
3. Filter for Source: `.NET Runtime` or `ConsoleDeckService`

### File Logging (Optional)
To add file logging, update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "File": {
      "Path": "C:\\Logs\\ConsoleDeck\\log-.txt",
      "RollingInterval": "Day"
    }
  }
}
```

*Note: Requires `Serilog.Extensions.Logging.File` NuGet package.*

## Error Handling

### HID Device Errors

**Device Not Found:**
```
Error: No ConsoleDeck device detected (VendorId: 0x2E8A)
```
**Resolution:**
- Check USB connection
- Verify device shows in Device Manager as HID device
- Check VendorId in configuration matches your device

**Device Disconnected:**
```
Warning: ConsoleDeck device disconnected: RPi Pico
```
**Resolution:**
- Service automatically reconnects when device is plugged back in
- Check USB cable and port

**Permission Denied:**
```
Error: Access denied to HID device
```
**Resolution (Windows):**
- Run as Administrator
- Check Windows Security hasn't blocked the application

### Action Execution Errors

**Application Not Found:**
```
Error: Application not found: C:\path\to\app.exe
```
**Resolution:**
- Verify file path in configuration
- Check application is installed
- Use full path for non-system applications

**Script Execution Failed:**
```
Error: Failed to execute script: access-denied.ps1
PowerShell execution policy may be blocking script execution
```
**Resolution (Windows PowerShell):**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**URL Open Failed:**
```
Error: Failed to open URL: https://example.com
```
**Resolution:**
- Check internet connection
- Verify URL is valid
- Ensure default browser is configured

### Configuration Errors

**Invalid JSON:**
```
Error: Failed to load configuration: Invalid JSON at line 15
```
**Resolution:**
- Use a JSON validator to check syntax
- Check for missing commas, quotes, or brackets

**Validation Errors:**
```
Warning: Invalid function key number: F25. Must be F13-F21.
Warning: Duplicate mapping found for F13
```
**Resolution:**
- Fix configuration according to error messages
- See CONFIGURATION.md for valid values

## Debugging Tips

### 1. Enable Verbose Logging
```json
{
  "Logging": {
    "LogLevel": {
      "ConsoleDeckService": "Trace"
    }
  },
  "ConsoleDeck": {
    "verboseLogging": true
  }
}
```

### 2. Test HID Communication
Check logs for:
```
Information: Connected to ConsoleDeck device: Raspberry Pi Pico
Debug: Key combination detected: RCtrl+RShift+F13
```

### 3. Test Action Execution
Verify action logs:
```
Information: Executing action: Open Notepad (LaunchApplication: notepad.exe)
Information: Action 'Open Notepad' executed successfully
```

### 4. Monitor Configuration Reloads
Watch for:
```
Information: Configuration file changed, reloading...
Information: Configuration reloaded with 9 key mappings
```

## Common Issues and Solutions

### Issue: Service Starts But No Actions Execute

**Check:**
1. Device connected: Look for "Connected to ConsoleDeck device" log
2. Key detection: Look for "Key combination detected" logs
3. Action mapping: Verify configuration has mapping for pressed key
4. Action enabled: Check `"enabled": true` in action configuration

**Debug:**
```powershell
# Run with verbose logging
$env:ConsoleDeckService__VerboseLogging="true"
dotnet run
```

### Issue: Tray Icon Not Appearing

**Check:**
1. Avalonia initialization logs
2. Platform support (Windows/Linux/macOS)
3. System tray enabled in OS settings

**Debug:**
Look for:
```
Information: Initializing Avalonia system tray
Information: System tray initialized successfully
```

### Issue: Configuration Not Reloading

**Check:**
1. File watcher logs
2. JSON syntax errors
3. File permissions

**Debug:**
```
Debug: Configuration file watcher enabled
Information: Configuration file changed, reloading...
```

## Performance Monitoring

### Key Metrics Logged

- **Device Connection Time:** How long to detect and connect
- **Action Execution Time:** Time to execute each action
- **Message Queue Depth:** Pending key events
- **Debounce Effectiveness:** Repeated key press filtering

### Monitoring Commands

**Windows:**
```powershell
# Tail logs in real-time (requires Get-Content)
Get-Content -Path "C:\Logs\ConsoleDeck\log.txt" -Wait -Tail 50
```

**Linux/macOS:**
```bash
# Tail logs
tail -f /var/log/consoledeck/log.txt
```

## Support and Troubleshooting

### Collecting Logs for Support

1. Enable verbose logging
2. Reproduce the issue
3. Collect logs from startup to error
4. Include configuration (remove sensitive data)

### Log Format

```
[Timestamp] [LogLevel] [Category] Message
[2024-01-15 10:30:45] [Information] [ConsoleDeckService.Worker] ConsoleDeck Service starting...
[2024-01-15 10:30:45] [Debug] [ConsoleDeckService.Services.ConfigurationService] Loading configuration...
```

### Additional Diagnostics

**Check HID Device:**
```powershell
# Windows Device Manager
devmgmt.msc

# Look for: Human Interface Devices > HID-compliant device
```

**Check Process:**
```powershell
# Is service running?
Get-Process | Where-Object {$_.ProcessName -like "*ConsoleDeck*"}
```

## Further Help

- Review `CONFIGURATION.md` for configuration issues
- Review `CROSS_PLATFORM.md` for platform-specific issues
- Check GitHub issues for known problems
- Enable verbose logging before reporting bugs
