namespace ConsoleDeckService.Core.Models;

/// <summary>
/// Defines a single action that can be triggered by a key combination.
/// </summary>
public class ActionDefinition
{
    /// <summary>
    /// Friendly name for this action.
    /// Example: "Open GitHub", "Launch VS Code", "Run Backup Script"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this action does (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The type of action to perform.
    /// </summary>
    public ActionType Type { get; set; }

    /// <summary>
    /// The target for the action.
    /// - For LaunchApplication: path to executable (e.g., "notepad.exe")
    /// - For OpenUrl: URL (e.g., "https://github.com")
    /// - For ExecuteScript: path to script file (e.g., "C:\Scripts\backup.ps1")
    /// - For SendKeystrokes: keystroke sequence (e.g., "{CTRL}C")
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Optional arguments for LaunchApplication or ExecuteScript.
    /// Example: For notepad.exe, could be "C:\temp\file.txt"
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// Working directory for LaunchApplication or ExecuteScript (optional).
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Indicates if this action is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public override string ToString() => $"{Name} ({Type}: {Target})";
}
