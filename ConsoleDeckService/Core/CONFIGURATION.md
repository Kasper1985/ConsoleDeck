# ConsoleDeck Configuration Guide

## Overview

ConsoleDeck is configured via the `appsettings.json` file. The configuration supports hot-reload, so changes take effect immediately without restarting the service.

## Configuration Structure

```json
{
  "ConsoleDeck": {
    "vendorId": 51966,              // USB Vendor ID (0xCAFE = 51966 for RPi Zero)
    "productId": null,              // Optional: USB Product ID filter
    "debounceMs": 200,              // Debounce time in milliseconds
    "verboseLogging": false,        // Enable detailed logging for debugging
    "autoStart": false,             // Start with Windows (not yet implemented)
    "showNotifications": true,      // Show toast notifications when actions execute
    "keyMappings": [ ... ]          // Array of key-to-action mappings
  }
}
```

## Key Mappings

Each ConsoleDeck button (F13-F21) can be mapped to an action:

```json
{
  "functionKeyNumber": 13,          // F13 through F21 (13-21)
  "action": {
    "name": "Action Name",          // Display name for the action
    "description": "Description",   // Optional description
    "type": "ActionType",           // See Action Types below
    "target": "target",             // The action target (path, URL, etc.)
    "arguments": "args",            // Optional arguments
    "workingDirectory": "path",     // Optional working directory
    "enabled": true                 // Enable/disable this action
  }
}
```

## Action Types

### 1. LaunchApplication

Launches an application or executable.

**Example - System Command:**
```json
{
  "type": "LaunchApplication",
  "target": "notepad.exe",
  "arguments": null,
  "workingDirectory": null
}
```

**Example - Full Path:**
```json
{
  "type": "LaunchApplication",
  "target": "C:\\Program Files\\MyApp\\app.exe",
  "arguments": "--debug",
  "workingDirectory": "C:\\Projects\\MyProject"
}
```

**Common Applications:**
- `notepad.exe` - Notepad
- `calc.exe` - Calculator
- `explorer.exe` - File Explorer
- `wt.exe` - Windows Terminal
- `code` - Visual Studio Code (if in PATH)

### 2. OpenUrl

Opens a URL in the default web browser.

**Example:**
```json
{
  "type": "OpenUrl",
  "target": "https://github.com",
  "arguments": null,
  "workingDirectory": null
}
```

**Use Cases:**
- Open frequently used websites
- Launch web-based dashboards
- Open project documentation
- Access CI/CD pipelines

### 3. ExecuteScript

Executes a script file (PowerShell, Batch, Python, etc.).

**PowerShell Example:**
```json
{
  "type": "ExecuteScript",
  "target": "C:\\Scripts\\build.ps1",
  "arguments": "-Configuration Release",
  "workingDirectory": "C:\\Projects\\MyProject"
}
```

**Batch File Example:**
```json
{
  "type": "ExecuteScript",
  "target": "C:\\Scripts\\backup.bat",
  "arguments": null,
  "workingDirectory": "C:\\Backups"
}
```

**Python Example:**
```json
{
  "type": "ExecuteScript",
  "target": "C:\\Scripts\\deploy.py",
  "arguments": "--environment prod",
  "workingDirectory": "C:\\Scripts"
}
```

**Supported Script Types:**
- **Windows**: `.ps1`, `.bat`, `.cmd`, `.py`
- **Linux**: `.sh`, `.py`
- **macOS**: `.sh`, `.py`

### 4. SendKeystrokes (Not Yet Implemented)

Will send a sequence of keystrokes to the active window.

```json
{
  "type": "SendKeystrokes",
  "target": "{CTRL}C",
  "arguments": null,
  "workingDirectory": null
}
```

### 5. None

Disables the button (no action).

```json
{
  "type": "None",
  "target": "",
  "enabled": false
}
```

## Example Configurations

### Developer Workflow
```json
{
  "keyMappings": [
    {
      "functionKeyNumber": 13,
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
      "functionKeyNumber": 14,
      "action": {
        "name": "Build Project",
        "type": "ExecuteScript",
        "target": "C:\\Projects\\build.ps1",
        "enabled": true
      }
    },
    {
      "functionKeyNumber": 15,
      "action": {
        "name": "Run Tests",
        "type": "ExecuteScript",
        "target": "C:\\Projects\\test.ps1",
        "enabled": true
      }
    },
    {
      "functionKeyNumber": 16,
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
      "functionKeyNumber": 13,
      "action": {
        "name": "Launch OBS Studio",
        "type": "LaunchApplication",
        "target": "C:\\Program Files\\obs-studio\\bin\\64bit\\obs64.exe",
        "enabled": true
      }
    },
    {
      "functionKeyNumber": 14,
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
      "functionKeyNumber": 13,
      "action": {
        "name": "Task Manager",
        "type": "LaunchApplication",
        "target": "taskmgr.exe",
        "enabled": true
      }
    },
    {
      "functionKeyNumber": 14,
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
