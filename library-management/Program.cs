using Avalonia;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using library_management.Configuration;
using library_management.Views;
using library_management.ViewModels;

namespace library_management;

public class Program
{
    public static IConfiguration Configuration { get; private set; } = null!;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    [STAThread]
    public static void Main(string[] args)
    {
        // Чтение конфигурации из appsettings.json
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Configure services
        var services = new ServiceCollection();
        ServiceConfiguration.ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Вызов сидера для пересоздания и заполнения базы
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<library_management.Models.LibraryDbContext>();
            Console.WriteLine("[Seeder] Пересоздание и заполнение базы...");
            library_management.Data.SeedData.SeedDatabaseAsync(dbContext).GetAwaiter().GetResult();
            Console.WriteLine("[Seeder] Готово!");
        }

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