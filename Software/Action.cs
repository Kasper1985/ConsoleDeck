using System.Diagnostics;

namespace ConsoleDeck;

public class Action(string name, string description, ActionType type, string? payload = null, string? imagePath = null)
{
    public Action() : this("", "", ActionType.Command, null, null) { }

    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public ActionType Type { get; set; } = type;
    public string? Payload { get; set; } = payload;
    public string? ImagePath { get; set; } = imagePath;

    public void Execute()
    {
        switch (Type)
        {
            case ActionType.Command when !string.IsNullOrWhiteSpace(Payload):
                ExecuteCommand(Payload);
                break;
            case ActionType.WebUrl when !string.IsNullOrWhiteSpace(Payload):
                OpenUrl(Payload);
                break;
            case ActionType.Macro:
                throw new NotImplementedException("Macro execution is not implemented yet.");
            case ActionType.Script:
                throw new NotImplementedException("Script execution is not implemented yet.");
        }
    }

    public static void ExecuteCommand(string command)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = command.Trim(),
                UseShellExecute = true,

            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing command: {ex.Message}");
        }
    }
    
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening URL: {ex.Message}");
        }
    }
}

public enum ActionType
{
    Command,
    WebUrl,
    Macro,
    Script
}