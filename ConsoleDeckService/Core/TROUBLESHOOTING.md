# Troubleshooting ConsoleDeck Key Detection

## Changes Made

### 1. Enhanced HID Monitoring with Full Diagnostics
The `WindowsHidDeviceMonitor` now logs:
- **ALL HID devices** found on the system with their VID/PID
- **RAW HID data** in hexadecimal format for every report received
- **Modifier keys** pressed (LCtrl, RCtrl, LShift, RShift, etc.)
- **All key codes** received, not just F13-F21
- **Report structure** detection (with or without Report ID byte)
- **Warnings** when F13-F21 are pressed WITHOUT RCtrl+RShift

### 2. Updated Configuration
- Set `vendorId` to `52478` (0xCAFE) to match your Raspberry Pi Zero device
- Enabled `verboseLogging` for detailed diagnostics

## How to Diagnose the Issue

### Step 1: Run the Service and Watch Logs

```powershell
cd ConsoleDeckService
dotnet run
```

### Step 2: Press a Media Key (Volume Up/Down)
Watch the logs - you should see:
```
[Debug] RAW HID Report (X bytes): 01 00 E9 00 00 00 00 00
[Info] Modifiers pressed: ...
[Info] Key pressed - Scan code: 0xE9 (233)
```

**This confirms:**
- ? Device is connected
- ? HID communication works
- ? We're receiving reports

### Step 3: Press a ConsoleDeck Button (RCtrl+RShift+F13)
Watch for:
```
[Debug] RAW HID Report (X bytes): 30 00 68 00 00 00 00 00
[Info] Modifiers pressed: RCtrl+RShift
[Info] Key pressed - Scan code: 0x68 (104)
[Info] ? FUNCTION KEY DETECTED: F13
[Info] ?? KEY COMBINATION TRIGGERED: RCtrl+RShift+F13
```

## Common Issues and Solutions

### Issue 1: No HID Reports Received At All

**Symptoms:**
- No "RAW HID Report" logs appear
- Service says "Connected" but nothing happens

**Solution:**
Your device might have multiple HID interfaces. Check logs for:
```
[Debug] Enumerating all HID devices:
  - VID: 0xCAFE, PID: 0x4004 - Manufacturer Product
```

If you see multiple devices with the same VID, you may need to specify the `productId` in configuration.

### Issue 2: Wrong Vendor/Product ID

**Symptoms:**
- Service never connects to device
- Logs show "No matching device found"

**Check Windows Device Manager:**
1. Open Device Manager (`devmgmt.msc`)
2. Expand "Human Interface Devices"
3. Right-click your ConsoleDeck device ? Properties
4. Hardware Ids tab
5. Look for `HID\VID_XXXX&PID_YYYY`

**Update appsettings.json:**
```json
{
  "ConsoleDeck": {
    "vendorId": YOUR_VID_DECIMAL,  // 0xCAFE = 52478
    "productId": YOUR_PID_DECIMAL   // Optional but helps
  }
}
```

### Issue 3: Device Sends Different Key Codes

**Symptoms:**
- Media keys work (logs show scan codes like 0xE9)
- F13-F21 don't work (different scan codes appear)

**Your RPi Pico firmware might be sending:**
- **Wrong scan codes** - Check logs to see what's actually sent
- **Different modifiers** - Logs will show which modifiers are active
- **Consumer Control reports** instead of Keyboard reports

**Expected F13-F21 Scan Codes (HID Usage Table):**
- F13 = 0x68 (104)
- F14 = 0x69 (105)
- F15 = 0x6A (106)
- F16 = 0x6B (107)
- F17 = 0x6C (108)
- F18 = 0x6D (109)
- F19 = 0x6E (110)
- F20 = 0x6F (111)
- F21 = 0x70 (112)

**If your device sends different codes:**
1. Note the actual scan codes from logs
2. Update `ProcessHidReport` method to match your device's codes

### Issue 4: Modifiers Not Detected

**Symptoms:**
- Logs show: "F13 pressed without RCtrl+RShift modifiers"
- Modifiers byte is 0x00 or wrong value

**Expected Modifiers Byte:**
```
RCtrl + RShift = 0x30
  Bit 4 (0x10) = Right Control
  Bit 5 (0x20) = Right Shift
  Together: 0x10 | 0x20 = 0x30
```

**Your firmware should send:**
```
Byte 0 (or 1 after Report ID): 0x30
Byte 2+: Key scan code (0x68 for F13)
```

### Issue 5: Report ID Confusion

**Symptoms:**
- RAW data shows strange patterns
- First byte is always 0x01, 0x02, etc.

**HID reports may include a Report ID:**
```
WITH Report ID:    01 30 00 68 00 00 00 00
                   ^^ Report ID

WITHOUT Report ID: 30 00 68 00 00 00 00 00
                   ^^ Modifiers directly
```

The code now auto-detects this!

## Testing with Temporary Workaround

If F13-F21 aren't detected, you can temporarily test with regular keys:

### Modify KeyCombination.cs (TEMPORARY TEST ONLY):
```csharp
// Add support for F1-F12 for testing
if (keyCode >= 0x3A && keyCode <= 0x45)
{
    int functionKeyNumber = (keyCode - 0x3A) + 1; // F1=1, F2=2, etc.
    _logger.LogInformation("? FUNCTION KEY DETECTED: F{Number}", functionKeyNumber);
    // ... rest of code
}
```

Then test with F1-F12 which are standard keys.

## Firmware Debugging Checklist

If keys still aren't detected, your RPi Pico firmware might need changes:

### ? Check Firmware HID Descriptor
Your device should report as:
- **HID Keyboard** (Usage Page 0x01, Usage 0x06)
- **Report size**: Minimum 8 bytes (1 modifier + 1 reserved + 6 keys)

### ? Verify Key Codes Sent
Use a HID monitoring tool:
- **Windows**: [HidMonitor](https://github.com/todbot/hidpico)
- **Cross-platform**: `hidapi` test programs

### ? Check Modifier Byte Format
```c
// RPi Pico firmware should set bit flags:
uint8_t modifiers = 0;
if (right_ctrl_pressed)  modifiers |= 0x10;
if (right_shift_pressed) modifiers |= 0x20;
```

## Getting Help

When asking for help, provide:
1. **Full log output** (especially "RAW HID Report" lines)
2. **Device VID/PID** from Device Manager
3. **What keys work** (media keys?) vs what doesn't (F13-F21?)
4. **RPi Pico firmware** you're using (link or code snippet)

## Next Steps

1. **Run the service** with enhanced logging
2. **Press keys** and observe the logs
3. **Share the RAW HID Report lines** so we can see exactly what your device sends
4. We'll adjust the code based on your device's actual behavior

The diagnostic version will tell us exactly what your ConsoleDeck is sending!
