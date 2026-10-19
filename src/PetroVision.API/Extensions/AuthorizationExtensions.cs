namespace PetroVision.API.Extensions;

public static class Policies
{
    public const string Staff = "Staff";                         // Manager or Agent (not Owner)
    public const string ManagerOnly = "ManagerOnly";
    public const string ManageCameras = "ManageCameras";
    public const string EditModelSettings = "EditModelSettings";
}

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Staff, p => p.RequireRole("Manager", "Agent"))
            .AddPolicy(Policies.ManagerOnly, p => p.RequireRole("Manager"))
            .AddPolicy(Policies.ManageCameras, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.HasClaim("permission", "ManageCameras")))
            .AddPolicy(Policies.EditModelSettings, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.HasClaim("permission", "EditModelSettings")));

        return services;
    }
}
