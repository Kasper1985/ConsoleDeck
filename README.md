# ConsoleDeck

A customizable macro deck solution featuring a custom HID hardware device powered by a Raspberry Pi Pico and companion software applications. Execute commands, launch applications, open URLs, and control media playback with configurable buttons and a rotary encoder.

## Features

- **Custom Hardware**: A 10-button HID device with rotary encoder for volume control and media playback
- **Cross-Platform Service**: Background service with system tray integration supporting Windows, Linux, and macOS
- **Windows Forms Application**: Native Windows configuration interface (legacy)
- **Configurable Actions**: Support for launching applications, opening URLs, executing scripts, and macros
- **Hot Reload Configuration**: Changes to configuration take effect immediately without restart
- **RGB LED Support**: WS2812 addressable LED integration for visual feedback

## Project Structure

```
ConsoleDeck/
├── Hardware/               # Raspberry Pi Pico firmware
│   ├── src/               # C source code for HID device
│   │   ├── main.c         # Main program entry point
│   │   ├── board.c/h      # Button handling (10 buttons)
│   │   ├── rotary_encoder.c/h  # KY-040 rotary encoder support
│   │   ├── led.c/h        # WS2812 LED control
│   │   ├── hid_app.c/h    # USB HID implementation
│   │   └── usb_descriptors.c/h # USB device descriptors
│   ├── CMakeLists.txt     # Build configuration
│   └── .devcontainer/     # Development container setup
│
├── ConsoleDeckService/     # Cross-platform background service
│   ├── Core/
│   │   ├── Interfaces/    # Service abstractions
│   │   ├── Services/      # Platform-specific implementations
│   │   ├── Models/        # Data models
│   │   └── UI/            # Avalonia UI components
│   ├── Worker.cs          # Background service implementation
│   ├── Program.cs         # Service entry point
│   └── appsettings.json   # Configuration file
│
└── Software/              # Windows Forms application (legacy)
    ├── MainForm.cs        # Main application window
    ├── DeckButton.cs      # Custom button control
    ├── DeckRotary.cs      # Rotary encoder control
    ├── actions.yml        # Action configuration
    └── button_images/     # Button icons
```

## Hardware

### Components
- **Microcontroller**: Raspberry Pi Pico (RP2040)
- **Buttons**: 10 programmable keys
- **Rotary Encoder**: KY-040 module for volume/media control
- **LEDs**: WS2812 addressable RGB LEDs
- **USB Interface**: HID Consumer Control device

### Building the Firmware

Prerequisites:
- [Raspberry Pi Pico SDK](https://github.com/raspberrypi/pico-sdk)
- CMake 3.20+
- ARM GCC Toolchain

```bash
cd Hardware
mkdir build && cd build
cmake ..
make
```

The resulting `HID_Device.uf2` file can be flashed to the Pico by holding BOOTSEL during connection.

### Button Mapping

| Button | Key Code | Description |
|--------|----------|-------------|
| 1-9    | 0xF1-0xF9 (241-249) | Programmable action buttons |
| 10     | Media Play/Pause | Media control |
| Rotary Press | Volume Mute | Mute audio |
| Rotary CW | Volume Up | Increase volume |
| Rotary CCW | Volume Down | Decrease volume |

## Software

### ConsoleDeckService (Recommended)

A modern cross-platform background service with system tray integration built with .NET 10 and Avalonia UI.

**Supported Platforms:**
- Windows
- Linux
- macOS

**Features:**
- System tray icon with context menu
- Settings window for configuration
- HID device monitoring
- Configurable action mappings
- Serilog logging

#### Building

```bash
cd ConsoleDeckService
dotnet restore
dotnet build
```

#### Running

```bash
dotnet run
```

#### Configuration

Edit `appsettings.json` to configure button actions. See [CONFIGURATION.md](ConsoleDeckService/Core/CONFIGURATION.md) for detailed options.

Example action mapping:
```json
{
  "ConsoleDeck": {
    "vendorId": 51966,
    "keyMappings": [
      {
        "keyCode": 241,
        "action": {
          "name": "Open VS Code",
          "type": "LaunchApplication",
          "target": "code",
          "enabled": true
        }
      }
    ]
  }
}
```

**Action Types:**
- `LaunchApplication` - Launch an executable or file
- `OpenUrl` - Open a URL in the default browser
- `ExecuteScript` - Run a script or command

### Windows Forms Application (Legacy)

A native Windows application with visual button configuration.

```bash
cd Software
dotnet restore
dotnet build
dotnet run
```

## Getting Started

1. **Build and flash the hardware firmware** to your Raspberry Pi Pico
2. **Connect the ConsoleDeck** via USB
3. **Start the ConsoleDeckService** or Windows Forms application
4. **Configure your actions** via the settings window or configuration file
5. **Press buttons** to execute your configured actions

## Documentation

- [Configuration Guide](ConsoleDeckService/Core/CONFIGURATION.md)
- [Cross-Platform Support](ConsoleDeckService/Core/CROSS_PLATFORM.md)
- [Logging Guide](ConsoleDeckService/Core/LOGGING.md)
- [Troubleshooting](ConsoleDeckService/Core/TROUBLESHOOTING.md)
- [Diagnostics](ConsoleDeckService/Core/README_DIAGNOSTICS.md)

## Dependencies

### Hardware
- Raspberry Pi Pico SDK
- TinyUSB

### ConsoleDeckService
- .NET 10.0
- Avalonia 11.3
- HidSharp 2.6.4
- Serilog

### Windows Forms Application
- .NET 10.0
- Windows Forms
- YamlDotNet

## License

This project is provided as-is for personal and educational use.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
