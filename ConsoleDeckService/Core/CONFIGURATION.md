# ConsoleDeck Configuration Guide

## Basic Configuration Structure

The `appsettings.json` file contains the ConsoleDeck configuration under the `"ConsoleDeck"` section:

```json
{
  "ConsoleDeck": {
    "vendorId": 51966,
    "productId": null,
    "debounceMs": 200,
    "verboseLogging": true,
    "autoStart": false,
    "showNotifications": false,
    "keyMappings": [
      {
        "keyCode": 241,
        "action": {
          "name": "Open Notepad",
          "description": "Opens Windows Notepad for quick notes",
          "type": "LaunchApplication",
          "target": "notepad.exe",
          "arguments": null,
          "workingDirectory": null,
          "enabled": true
        }
      }
    ]
  }
}
```

## Key Code Mapping

ConsoleDeck buttons send key codes 0xF1 through 0xF9 (241-249 decimal):

- **Button 9**: 0xF1 (241)
- **Button 8**: 0xF2 (242)
- **Button 7**: 0xF3 (243)
- **Button 6**: 0xF4 (244)
- **Button 5**: 0xF5 (245)
- **Button 4**: 0xF6 (246)
- **Button 3**: 0xF7 (247)
- **Button 2**: 0xF8 (248)
- **Button 1**: 0xF9 (249)

## Action Types

### LaunchApplication
Launches an executable or opens a file:

```json
{
  "keyCode": 241,
  "action": {
    "name": "Open VS Code",
    "type": "LaunchApplication",
    "target": "code",
    "arguments": ".",
    "workingDirectory": "C:\\Projects\\MyProject",
    "enabled": true
  }
}
```

### OpenUrl
Opens a URL in the default browser:

```json
{
  "keyCode": 242,
  "action": {
    "name": "Open GitHub",
    "type": "OpenUrl",
    "target": "https://github.com/myorg/myproject",
    "enabled": true
  }
}
```

### ExecuteScript
Runs a script or command:

```json
{
  "keyCode": 243,
  "action": {
    "name": "Build Project",
    "type": "ExecuteScript",
    "target": "C:\\Projects\\build.ps1",
    "enabled": true
  }
}
```

## Example Configurations

### Development Workflow
```json
{
  "keyMappings": [
    {
      "keyCode": 241,
      "action": {
        "name": "Open VS Code",
        "type": "LaunchApplication",
        "target": "code",
        "arguments": ".",
        "workingDirectory": "C:\\Projects\\MyProject",
        "enabled": true
      }
    },
    {
      "keyCode": 242,
      "action": {
        "name": "Build Project",
        "type": "ExecuteScript",
        "target": "C:\\Projects\\build.ps1",
        "enabled": true
      }
    },
    {
      "keyCode": 243,
      "action": {
        "name": "Run Tests",
        "type": "ExecuteScript",
        "target": "C:\\Projects\\test.ps1",
        "enabled": true
      }
    },
    {
      "keyCode": 244,
      "action": {
        "name": "Open GitHub",
        "type": "OpenUrl",
        "target": "https://github.com/myorg/myproject",
        "enabled": true
      }
    }
  ]
}
```

### Media & Content Creation
```json
{
  "keyMappings": [
    {
      "keyCode": 241,
      "action": {
        "name": "Launch OBS Studio",
        "type": "LaunchApplication",
        "target": "C:\\Program Files\\obs-studio\\bin\\64bit\\obs64.exe",
        "enabled": true
      }
    },
    {
      "keyCode": 242,
      "action": {
        "name": "Open Spotify",
        "type": "OpenUrl",
        "target": "https://open.spotify.com",
        "enabled": true
      }
    }
  ]
}
```

### System Administration
```json
{
  "keyMappings": [
    {
      "keyCode": 241,
      "action": {
        "name": "Task Manager",
        "type": "LaunchApplication",
        "target": "taskmgr.exe",
        "enabled": true
      }
    },
    {
      "keyCode": 242,
      "action": {
        "name": "System Monitoring",
        "type": "LaunchApplication",
        "target": "perfmon.exe",
        "enabled": true
      }
    }
  ]
}
```

## Configuration Tips

### 1. Vendor ID for Raspberry Pi Zero
The default Vendor ID is `51966` (hex: `0xCAFE`) for Raspberry Pi Zero. If your device uses a different VID, update the `vendorId` field.

### 2. Hot Reload
Changes to `appsettings.json` are detected automatically. Save the file and your new configuration takes effect immediately.

### 3. Debouncing
The `debounceMs` setting prevents accidental double-triggers. Default is 200ms. Increase if you experience unwanted multiple triggers.

### 4. Disabling Actions
Set `"enabled": false` to temporarily disable an action without removing its configuration.

### 5. Testing Actions
After changing the configuration:
1. Press the corresponding button on your ConsoleDeck
2. Watch the system tray notifications
3. Check the logs if something doesn't work as expected

### 6. Path Formatting
Use double backslashes (`\\`) in JSON for Windows paths:
- ? `"C:\\Projects\\MyProject"`
- ? `"C:\Projects\MyProject"` (invalid JSON)

Alternatively, use forward slashes (works on Windows too):
- ? `"C:/Projects/MyProject"`

## Troubleshooting

### Action Not Executing
1. Check logs for errors
2. Verify the `target` path exists
3. Ensure `enabled` is set to `true`
4. Test the command manually first

### Device Not Detected
1. Verify `vendorId` matches your device
2. Check Windows Device Manager for HID device
3. Try unplugging and reconnecting
4. Check logs for connection messages

### Configuration Errors
1. Validate JSON syntax (use a JSON validator)
2. Check for duplicate `functionKeyNumber` values
3. Ensure function keys are in range (13-21)
4. Review logs for validation errors on startup

## Log Locations

Logs are written to:
- Console output (when running from terminal)
- Windows Event Viewer (when running as service)
- Check Worker service logs for detailed diagnostics

Enable verbose logging:
```json
{
  "ConsoleDeck": {
    "verboseLogging": true
  }
}
```

## Further Customization

For advanced customization, you can:
- Edit the source code in `ConsoleDeckService/`
- Create custom action types by extending `IActionExecutor`
- Add additional configuration properties
- Implement platform-specific behaviors

See `CROSS_PLATFORM.md` for details on extending the service.
