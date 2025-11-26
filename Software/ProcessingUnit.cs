using YamlDotNet.Serialization;

namespace ConsoleDeck;

internal static class ProcessingUnit
{
    private static int _countActions = 9;

    private static List<Action?> Actions = [.. Enumerable.Repeat<Action?>(null, _countActions)];

    internal static void ReadConfiguration(string filePath = "actions.yml")
    {
        var deserializer = new DeserializerBuilder().Build();
        using var reader = new StreamReader(filePath);
        var actions = deserializer.Deserialize<List<Action?>>(reader);

        for (var i = 0; i < _countActions; i++)
            Actions[i] = i < actions.Count ? actions[i] : null;
    }

    internal static List<Action?> GetActions() => Actions;

    internal static void UpdateAction(int index, Action? newAction)
    {
        if (index >= 0 && index < _countActions)
            Actions[index] = newAction;
    }

    internal static void SaveConfiguration(string filePath = "actions.yml")
    {
        var serializer = new SerializerBuilder().Build();
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, Actions);
    }

    internal static void ProcessKeyEvent(HashSet<int> pressedKeys)
    {
        // Check if the pressed keys match any predefined shortcuts
        // and execute corresponding actions.
        if (pressedKeys.Count == 3 && pressedKeys.SetContainsAll((int)Keys.RShiftKey, (int)Keys.RControlKey))
        {
            var index = pressedKeys.Except([(int)Keys.RShiftKey, (int)Keys.RControlKey]).First() switch
            {
                (int)Keys.F13 => 8,
                (int)Keys.F14 => 7,
                (int)Keys.F15 => 6,
                (int)Keys.F16 => 5,
                (int)Keys.F17 => 4,
                (int)Keys.F18 => 3,
                (int)Keys.F19 => 2,
                (int)Keys.F20 => 1,
                (int)Keys.F21 => 0,
                _ => -1
            };

            if (index >= 0 && index < Actions.Count)
                Actions[index]?.Execute();
        }
    }
}