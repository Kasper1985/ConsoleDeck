# ConsoleDeck Service - Diagnostic Build

## ?? What Changed

I've updated the HID monitoring code to include **comprehensive diagnostics** to help us figure out why your ConsoleDeck buttons aren't being detected.

### Enhanced Logging

The service now logs:
- ? **All HID devices** on your system
- ? **RAW HID data** in hexadecimal (every key press)
- ? **Modifiers** detected (RCtrl, RShift, etc.)
- ? **All key scan codes** (not just F13-F21)
- ? **Warnings** if F13-F21 pressed without correct modifiers

### Configuration Updated
- **Vendor ID**: Changed to `52478` (0xCAFE) for RPi Zero
- **Verbose Logging**: Enabled by default

## ?? How to Run Diagnostics

### 1. Build and Run
```bash
cd ConsoleDeckService
dotnet build
dotnet run
```

### 2. Test Media Keys (Known Working)
Press Volume Up/Down on your ConsoleDeck.

**Look for logs like:**
```
[Debug] RAW HID Report (8 bytes): 00 00 E9 00 00 00 00 00
[Info] Key pressed - Scan code: 0xE9 (233)
```

This confirms the device is connected and HID communication works!

### 3. Test ConsoleDeck Buttons
Press **RCtrl + RShift + F13** (or any F13-F21 button).

**Expected logs:**
```
[Debug] RAW HID Report (8 bytes): 30 00 68 00 00 00 00 00
[Info] Modifiers pressed: RCtrl+RShift
[Info] Key pressed - Scan code: 0x68 (104)
[Info] ? FUNCTION KEY DETECTED: F13
[Info] ?? KEY COMBINATION TRIGGERED: RCtrl+RShift+F13
[Info] Executing action: Open Notepad
```

### 4. Share the Logs
Copy the **RAW HID Report** lines and share them! This will tell us:
- ? What scan codes your device actually sends
- ? If modifiers are being set correctly
- ? If there's a Report ID byte we need to handle

## ?? Quick Checks

### Is My Device Connected?
Look for:
```
[Info] Connected to ConsoleDeck device: Your Device Name
[Info] Device details: Max Input=8, Max Output=0, Max Feature=0
```

### Is HID Communication Working?
Press ANY key and look for:
```
[Debug] RAW HID Report (X bytes): ...
```
If you see this, communication works!

### What Scan Codes Is My Device Sending?
Every key press shows:
```
[Info] Key pressed - Scan code: 0xXX (decimal)
```

## ?? Common Issues

### Issue: Device Not Found
**Check Device Manager:**
1. Open `devmgmt.msc`
2. Look under "Human Interface Devices"
3. Find your ConsoleDeck device
4. Properties ? Hardware IDs
5. Note the VID and PID values

**Update appsettings.json:**
```json
{
  "ConsoleDeck": {
    "vendorId": YOUR_VID,  // Decimal value
    "productId": YOUR_PID  // Decimal value (optional)
  }
}
```

### Issue: Wrong Scan Codes
If logs show different scan codes than expected (0x68-0x70 for F13-F21):
- Note the actual codes from logs
- Your RPi Pico firmware might need adjustment
- OR we can modify the code to match your device

### Issue: No Modifiers Detected
If logs show `Modifiers byte: 0x00` when pressing RCtrl+RShift:
- Your firmware needs to set the modifier bits
- Expected value: `0x30` (RCtrl=0x10 + RShift=0x20)

## ?? Documentation

- **TROUBLESHOOTING.md** - Detailed diagnostic guide
- **CONFIGURATION.md** - How to configure actions
- **LOGGING.md** - Understanding the logs

## ?? Next Steps

1. **Run the service** with diagnostics
2. **Press your ConsoleDeck buttons**
3. **Copy the log output** (especially RAW HID Report lines)
4. **Share the logs** so we can identify the exact issue

The enhanced logging will show us exactly what your device is sending, and we'll adjust the code accordingly!

---

**Note:** The code is now more permissive - it will log warnings instead of silently ignoring key presses, so we can see everything your device sends.
