using Avalonia;
using Avalonia.Controls;
using ConsoleDeckService;
using ConsoleDeckService.Core.Interfaces;
using ConsoleDeckService.Core.Services;
using ConsoleDeckService.Core.Services.Windows;
using ConsoleDeckService.Core.Services.Linux;
using ConsoleDeckService.Core.Services.MacOS;
using ConsoleDeckService.Core.UI;
using Serilog;

// Start Avalonia on a background thread
var avaloniaThread = new Thread(() =>
{
    AppBuilder.Configure<TrayApplication>()
        .UsePlatformDetect()
        .LogToTrace()
        .StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
});

// Only set STA apartment state on Windows
if (OperatingSystem.IsWindows()) avaloniaThread.SetApartmentState(ApartmentState.STA);
avaloniaThread.IsBackground = false;
avaloniaThread.Start();

// Wait for Avalonia to initialize
await Task.Delay(2000);

// Create host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Reads Serilog setting from appsettings.json
    .CreateLogger();

// Integrate Serilog with the .NET logging system
#if !DEBUG
builder.Logging.ClearProviders(); // Optional: Removes default console/debug provider for file-only logging
#endif
builder.Logging.AddSerilog();

// Register configuration service
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

// Register platform-specific services
if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<IHidDeviceMonitor, WindowsHidDeviceMonitor>();
    builder.Services.AddSingleton<INotificationProvider, WindowsNotificationProvider>();
}
else if (OperatingSystem.IsLinux())
{
    builder.Services.AddSingleton<IHidDeviceMonitor, LinuxHidDeviceMonitor>();
    builder.Services.AddSingleton<INotificationProvider, LinuxNotificationProvider>();
}
else if (OperatingSystem.IsMacOS())
{
    builder.Services.AddSingleton<IHidDeviceMonitor, MacOSHidDeviceMonitor>();
    builder.Services.AddSingleton<INotificationProvider, MacOSNotificationProvider>();
}
else
{
    throw new PlatformNotSupportedException("Currently only Windows, Linux, and macOS are supported.");
}

// Register cross-platform services
builder.Services.AddSingleton<IActionExecutor, ActionExecutor>();
builder.Services.AddSingleton<ISystemTrayProvider, AvaloniaTrayProvider>();
builder.Services.AddSingleton<ISettingsWindowProvider, AvaloniaSettingsWindowProvider>();
builder.Services.AddSingleton<HidMessageProcessor>();

// Register the Worker service
builder.Services.AddHostedService<Worker>();

// Build and run
var host = builder.Build();
await host.RunAsync();
