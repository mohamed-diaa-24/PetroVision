using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetroVision.API.Filters;

// Protects the endpoints called by the AI service (machine-to-machine, no user login).
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private const string HeaderName = "X-Api-Key";
    private readonly string _apiKey;

    public ApiKeyAuthFilter(IConfiguration configuration)
        => _apiKey = configuration["Detection:ApiKey"] ?? string.Empty;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

        // If no key is configured (or it is too short) every request is rejected.
        if (_apiKey.Length < 16 || !Matches(provided))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                status = 401,
                title = "Unauthorized",
                detail = "Invalid or missing API key."
            });
        }
    }

    private bool Matches(string provided)
    {
        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(_apiKey);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}
