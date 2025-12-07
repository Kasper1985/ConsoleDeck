using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.UI.ViewModels;

namespace ConsoleDeckService.Core.UI;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    private readonly ILogger<SettingsWindow> _logger;

    public SettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        _viewModel = null!;
        _logger = null!;
    }

    public SettingsWindow(IConfigurationService configService, IAutoStartService autoStartService, ILogger<SettingsWindow> logger, ILogger<SettingsViewModel> viewModelLogger)
    {
        _logger = logger;
        _viewModel = new SettingsViewModel(configService, autoStartService, viewModelLogger);
        
        DataContext = _viewModel;
        
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        _logger.LogDebug("SettingsWindow initialized");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Save button clicked");

        try
        {
            var success = await _viewModel.SaveConfigurationAsync();

            if (success)
            {
                _logger.LogInformation("Configuration saved successfully");
                await ShowMessageBox("Success", "Settings saved successfully!");
            }
            else
            {
                _logger.LogWarning("Configuration validation failed");
                await ShowMessageBox("Validation Error", "Configuration validation failed. Please check the logs for details.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            await ShowMessageBox("Error", $"Failed to save settings: {ex.Message}");
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Cancel button clicked");

        if (_viewModel.HasUnsavedChanges)
        {
            _logger.LogDebug("Closing window with unsaved changes");
        }

        Close();
    }

    private void OnReloadClick(object? sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Reload button clicked");

        try
        {
            _viewModel.LoadConfiguration();
            _logger.LogInformation("Configuration reloaded from file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration");
        }
    }

    private async Task ShowMessageBox(string title, string message)
    {
        var messageBox = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            Background = this.Background,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Width = 100
        };
        okButton.Click += (s, e) => messageBox.Close();

        messageBox.Content = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(20),
            RowSpacing = 16,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14,
                },
                okButton
            }
        };

        await messageBox.ShowDialog(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.LogDebug("SettingsWindow closed");
        base.OnClosed(e);
    }
}
