using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace PetroVision.API.Extensions;

public static class RateLimits
{
    public const string Auth = "auth";

    // Login, sign-up and password reset: 10 requests per minute per IP (slows brute force on codes/passwords).
    public static IServiceCollection AddAppRateLimits(this IServiceCollection services)
        => services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(Auth, ctx => RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
        });
}
