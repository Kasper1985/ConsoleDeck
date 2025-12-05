namespace ConsoleDeckService.Core.Models;

/// <summary>
/// Defines the types of actions that can be triggered by HID device key combinations.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Launch an application or executable file.
    /// Example: "notepad.exe", "C:\Program Files\MyApp\app.exe"
    /// </summary>
    LaunchApplication,

    /// <summary>
    /// Open a URL in the default web browser.
    /// Example: "https://github.com", "https://google.com"
    /// </summary>
    OpenUrl,

    /// <summary>
    /// Execute a script file (PowerShell, Bash, etc.).
    /// Example: "C:\Scripts\backup.ps1", "/home/user/scripts/deploy.sh"
    /// </summary>
    ExecuteScript,

    /// <summary>
    /// Send a sequence of keystrokes to the active window.
    /// Example: Could be used for text expansion or macro sequences.
    /// </summary>
    SendKeystrokes,

    /// <summary>
    /// No action - can be used to disable a key combination.
    /// </summary>
    None
}
