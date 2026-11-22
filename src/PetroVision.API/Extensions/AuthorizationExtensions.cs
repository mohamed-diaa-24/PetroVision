namespace PetroVision.API.Extensions;

public static class Policies
{
    public const string Staff = "Staff";                         // Manager or Agent (not Owner)
    public const string ManagerOnly = "ManagerOnly";
    public const string OwnerOnly = "OwnerOnly";
    public const string ManageCameras = "ManageCameras";
    public const string EditModelSettings = "EditModelSettings";
    public const string ReassignIncidents = "ReassignIncidents";
    public const string ViewTeamLogs = "ViewTeamLogs";
}

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Staff, p => p.RequireRole("Manager", "Agent"))
            .AddPolicy(Policies.ManagerOnly, p => p.RequireRole("Manager"))
            .AddPolicy(Policies.OwnerOnly, p => p.RequireRole("Owner"))
            .AddPolicy(Policies.ManageCameras, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.HasClaim("permission", "ManageCameras")))
            .AddPolicy(Policies.EditModelSettings, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.HasClaim("permission", "EditModelSettings")))
            .AddPolicy(Policies.ReassignIncidents, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.HasClaim("permission", "ReassignIncidents")))
            .AddPolicy(Policies.ViewTeamLogs, p => p.RequireAssertion(ctx =>
                ctx.User.IsInRole("Manager") || ctx.User.IsInRole("Owner")
                || ctx.User.HasClaim("permission", "ViewTeamLogs")));

        return services;
    }
}
