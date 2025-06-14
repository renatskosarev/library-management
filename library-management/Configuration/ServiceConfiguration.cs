using library_management.Data.Interfaces;
using library_management.Data.Repositories;
using library_management.Models;
using library_management.Services;
using library_management.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace library_management.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection AddLibraryServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<LibraryDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Debug output
            System.Diagnostics.Debug.WriteLine($"Connection string: {connectionString}");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to in-memory database for development
                System.Diagnostics.Debug.WriteLine("Using in-memory database");
                options.UseInMemoryDatabase("LibraryDb");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Using PostgreSQL database");
                options.UseNpgsql(connectionString);
            }
        });

        // Data Access Layer
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Services
        services.AddScoped<ILibraryService, LibraryService>();

        return services;
    }
} 