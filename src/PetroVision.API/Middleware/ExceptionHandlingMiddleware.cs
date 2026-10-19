using PetroVision.BLL.Exceptions;

namespace PetroVision.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Server error")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                status,
                title,
                detail = status == 500 ? "An unexpected error occurred." : ex.Message
            });
        }
    }
}
