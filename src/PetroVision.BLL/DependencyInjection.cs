using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Services;
using PetroVision.BLL.Settings;
using PetroVision.DAL;

namespace PetroVision.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDataAccess(connectionString);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICameraService, CameraService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    public static Task InitializeDatabaseAsync(this IServiceProvider provider)
        => provider.InitializeDataAccessAsync();
}
