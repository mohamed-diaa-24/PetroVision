using Microsoft.Extensions.DependencyInjection;
using PetroVision.DAL;

namespace PetroVision.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDataAccess(connectionString);

        // Services will be registered here later (AuthService, CameraService, ...)
        return services;
    }

    public static Task InitializeDatabaseAsync(this IServiceProvider provider)
        => provider.InitializeDataAccessAsync();
}
