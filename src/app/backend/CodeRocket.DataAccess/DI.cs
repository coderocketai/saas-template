using Microsoft.Extensions.DependencyInjection;
using CodeRocket.DataAccess.Database;
using CodeRocket.DataAccess.Interfaces;
using CodeRocket.DataAccess.Repositories;

namespace CodeRocket.DataAccess;

/// <summary>
/// Dependency Injection configuration for DataAccess layer
/// </summary>
public static class DI
{
    /// <summary>
    /// Register DataAccess layer services in DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        // Database infrastructure
        services.AddScoped<ConnectionFactory>();
        
        // Repository interfaces and implementations
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}
