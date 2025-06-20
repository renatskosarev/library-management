using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using library_management.Data;
using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Services;
using library_management.Services.Interfaces;
using library_management.ViewModels;
using library_management.Models;

namespace library_management.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<LibraryDbContext>(options =>
        {
            options.UseNpgsql("Host=localhost;Port=5432;Database=library;Username=admin;Password=pass", 
                npgsqlOptions => 
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                });
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ILibraryService, LibraryService>();

        // ViewModels
        services.AddTransient<BooksViewModel>();
        services.AddTransient<AuthorsViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<MainViewModel>();
    }
} 