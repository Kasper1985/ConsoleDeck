# ConsoleDeck Assets

## Tray Icon

The system tray icon should be placed in this directory as `tray-icon.ico` (Windows) or `tray-icon.png` (Linux/macOS).

### Icon Requirements:
- **Format**: ICO (Windows), PNG (Linux/macOS)
- **Size**: 16x16, 32x32, 48x48 (multi-size ICO recommended)
- **Style**: Simple, recognizable icon representing a macro keypad or console deck

### Recommended Design:
- A grid of keys/buttons
- A keyboard symbol
- Or use a simple "CD" text logo

### Temporary Solution:
If no icon is provided, the system will use the default application icon.

### Adding the Icon:
1. Create your icon file
2. Save it as `ConsoleDeckService/UI/Assets/tray-icon.ico` (or .png)
3. Update the project file to include it as an embedded resource
4. The icon will be automatically loaded by AvaloniaTrayProvider
