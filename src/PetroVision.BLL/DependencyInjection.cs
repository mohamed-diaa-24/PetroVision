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
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IDetectionService, DetectionService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ISessionValidator, SessionValidator>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddSingleton<IStreamProbe, StreamProbe>();
        services.AddScoped<ICameraRequestService, CameraRequestService>();
        services.AddScoped<ISignupReviewService, SignupReviewService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }

    public static Task InitializeDatabaseAsync(this IServiceProvider provider)
        => provider.InitializeDataAccessAsync();
}
