using Avalonia;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace library_management;

sealed class Program
{
    public static IConfiguration Configuration { get; private set; }
    
    [STAThread]
    public static void Main(string[] args)
    {
        // Чтение конфигурации из appsettings.json
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}