# ConsoleDeck Service - Diagnostic Build

## ?? What Changed

I've updated the HID monitoring code to include **comprehensive diagnostics** to help us figure out why your ConsoleDeck buttons aren't being detected.

### Enhanced Logging

The service now logs:
- ? **All HID devices** on your system
- ? **RAW HID data** in hexadecimal (every key press)
- ? **All key scan codes** (0xF1-0xF9 for ConsoleDeck buttons)
- ? **Warnings** for unexpected key codes

### Configuration Updated
- **Vendor ID**: Set to `51966` (0xCAFE) for RPi Zero
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
Press **any ConsoleDeck button** (there are 9 buttons total).

**Expected logs:**
```
[Debug] RAW HID Report (3 bytes): 02 F1 FF
[Info] Key pressed - Scan code: 0xF1 (241)
[Info] Executing action: Open Notepad
```

### 4. Share the Logs
Copy the **RAW HID Report** lines and share them! This will tell us:
- ? What scan codes your device actually sends
- ? If the report format matches (3 bytes: Report ID, Key Code, Press/Release)
- ? If key codes are in the expected range (0xF1-0xF9)

## ?? Quick Checks

### Is My Device Connected?
Look for:
```
[Info] Connected to ConsoleDeck device: Your Device Name
[Info] Device details: Max Input=3, Max Output=0, Max Feature=0
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
If logs show different scan codes than expected (0xF1-0xF9 for buttons 1-9):
- Note the actual codes from logs
- Your RPi Pico firmware might need adjustment
- OR we can modify the code to match your device

### Issue: Unexpected Report Length
If RAW HID Report shows more or less than 3 bytes:
- The firmware might be sending different report formats
- Check if Report ID is included or not

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

**Note:** The code expects 3-byte HID reports with Report ID in byte 0, key code in byte 1, and press/release in byte 2.
