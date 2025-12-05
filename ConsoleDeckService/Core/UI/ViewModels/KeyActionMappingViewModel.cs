using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConsoleDeckService.Core.Models;

namespace ConsoleDeckService.Core.UI.ViewModels;

/// <summary>
/// ViewModel for individual key action mappings.
/// </summary>
public class KeyActionMappingViewModel : INotifyPropertyChanged
{
    private readonly SettingsViewModel _parent;
    private string? _actionName;
    private string? _actionDescription;
    private ActionType _actionType;
    private string? _actionTarget;
    private string? _actionArguments;
    private string? _workingDirectory;
    private bool _isEnabled;

    public event PropertyChangedEventHandler? PropertyChanged;

    public KeyActionMappingViewModel(KeyActionMapping mapping, SettingsViewModel parent)
    {
        _parent = parent;
        KeyCode = mapping.KeyCode;
        _actionName = mapping.Action.Name;
        _actionDescription = mapping.Action.Description;
        _actionType = mapping.Action.Type;
        _actionTarget = mapping.Action.Target;
        _actionArguments = mapping.Action.Arguments;
        _workingDirectory = mapping.Action.WorkingDirectory;
        _isEnabled = mapping.Action.Enabled;
    }

    public int KeyCode { get; }

    public string KeyDisplayName => $"#{0xFA - KeyCode} (0x{KeyCode:X2})";

    public string? ActionName
    {
        get => _actionName;
        set
        {
            if (_actionName != value)
            {
                _actionName = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public string? ActionDescription
    {
        get => _actionDescription;
        set
        {
            if (_actionDescription != value)
            {
                _actionDescription = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public ActionType ActionType
    {
        get => _actionType;
        set
        {
            if (_actionType != value)
            {
                _actionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowTargetFields));
                _parent.MarkAsChanged();
            }
        }
    }

    public string? ActionTarget
    {
        get => _actionTarget;
        set
        {
            if (_actionTarget != value)
            {
                _actionTarget = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public string? ActionArguments
    {
        get => _actionArguments;
        set
        {
            if (_actionArguments != value)
            {
                _actionArguments = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public string? WorkingDirectory
    {
        get => _workingDirectory;
        set
        {
            if (_workingDirectory != value)
            {
                _workingDirectory = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
                _parent.MarkAsChanged();
            }
        }
    }

    public bool ShowTargetFields => ActionType != ActionType.None;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
