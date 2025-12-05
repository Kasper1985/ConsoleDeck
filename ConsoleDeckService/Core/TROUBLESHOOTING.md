# Troubleshooting ConsoleDeck Key Detection

## Changes Made

### 1. Enhanced HID Monitoring with Full Diagnostics
The `WindowsHidDeviceMonitor` now logs:
- **ALL HID devices** found on the system with their VID/PID
- **RAW HID data** in hexadecimal format for every report received
- **All key codes** received (0xF1-0xF9 for ConsoleDeck buttons)
- **Report structure** detection (3 bytes expected)
- **Warnings** when key codes are outside the expected range

### 2. Updated Configuration
- Set `vendorId` to `51966` (0xCAFE) to match your Raspberry Pi Zero device
- Enabled `verboseLogging` for detailed diagnostics

## How to Diagnose the Issue

### Step 1: Run the Service and Watch Logs

```
cd ConsoleDeckService
dotnet run
```

### Step 2: Press a Media Key (Volume Up/Down)
Watch the logs - you should see:
```
[Debug] RAW HID Report (8 bytes): 01 00 E9 00 00 00 00 00
[Info] Modifiers pressed: ...
[Info] Key pressed - Scan code: 0xE9 (233)
```

**This confirms:**
- ? Device is connected
- ? HID communication works
- ? We're receiving reports

### Step 3: Press a ConsoleDeck Button
Watch for:
```
[Debug] RAW HID Report (3 bytes): 02 F1 FF
[Info] Key pressed - Scan code: 0xF1 (241)
[Info] Executing action: Open Notepad
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
    "vendorId": YOUR_VID_DECIMAL,  // 0xCAFE = 51966
    "productId": YOUR_PID_DECIMAL   // Optional but helps
  }
}
```

### Issue 3: Device Sends Different Key Codes

**Symptoms:**
- Media keys work (logs show scan codes like 0xE9)
- ConsoleDeck buttons don't work (different scan codes appear)

**Expected Key Codes for ConsoleDeck Buttons:**
- Button 1 = 0xF1 (241)
- Button 2 = 0xF2 (242)
- Button 3 = 0xF3 (243)
- Button 4 = 0xF4 (244)
- Button 5 = 0xF5 (245)
- Button 6 = 0xF6 (246)
- Button 7 = 0xF7 (247)
- Button 8 = 0xF8 (248)
- Button 9 = 0xF9 (249)

**If your device sends different codes:**
1. Note the actual scan codes from logs
2. Update `ProcessHidReport` method to match your device's codes

### Issue 4: Unexpected HID Report Format

**Symptoms:**
- RAW data shows different byte counts or patterns

**Expected HID Report Format:**
```
3 bytes: [Report ID] [Key Code] [Press/Release]
Example: 02 F1 FF (Report ID 2, Key 0xF1, Press)
```

**If your device sends different format:**
- Check firmware for correct HID descriptor
- Report should be 3 bytes with Report ID = 2

### Issue 5: Report ID Confusion

**Symptoms:**
- RAW data shows strange patterns
- First byte is not 0x02

**HID reports include a Report ID:**
```
WITH Report ID:    02 F1 FF
                   ^^ Report ID (expected: 2)

WITHOUT Report ID: F1 FF 00
                   ^^ Key code directly
```

The code expects Report ID in byte 0!

## Testing with Temporary Workaround

If buttons aren't detected, you can temporarily test with different codes:

### Modify ProcessHidReport.cs (TEMPORARY TEST ONLY):
```
// Add support for different key codes for testing
if (code >= 0xF1 && code <= 0xF9)
{
    int buttonNumber = (code - 0xF0); // 0xF1 = 1, etc.
    _logger.LogInformation("? BUTTON DETECTED: {Number}", buttonNumber);
    // ... rest of code
}
```

Then test with your device's actual codes.

## Firmware Debugging Checklist

If keys still aren't detected, your RPi Pico firmware might need changes:

### ? Check Firmware HID Descriptor
Your device should report as:
- **HID Keyboard** (Usage Page 0x01, Usage 0x06)
- **Report size**: 3 bytes (Report ID + Key + Press/Release)

### ? Verify Key Codes Sent
Use a HID monitoring tool:
- **Windows**: [HidMonitor](https://github.com/todbot/hidpico)
- **Cross-platform**: `hidapi` test programs

### ? Check Report Format
```
// RPi Pico firmware should send:
uint8_t report[3] = {2, key_code, 0xFF}; // Press
uint8_t report[3] = {2, key_code, 0x00}; // Release
```

## Getting Help

When asking for help, provide:
1. **Full log output** (especially "RAW HID Report" lines)
2. **Device VID/PID** from Device Manager
3. **What keys work** (media keys?) vs what doesn't (ConsoleDeck buttons?)
4. **RPi Pico firmware** you're using (link or code snippet)

## Next Steps

1. **Run the service** with enhanced logging
2. **Press keys** and observe the logs
3. **Share the RAW HID Report lines** so we can see exactly what your device sends
4. We'll adjust the code based on your device's actual behavior

The diagnostic version will tell us exactly what your ConsoleDeck is sending!
