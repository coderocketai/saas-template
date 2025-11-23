using CodeRocket.Services.Interfaces;
using CodeRocket.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace CodeRocket.Services;

/// <summary>
/// Dependency Injection configuration for Services layer
/// </summary>
public static class DI
{
    /// <summary>
    /// Register Services layer services in DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Repository interfaces and implementations
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}
