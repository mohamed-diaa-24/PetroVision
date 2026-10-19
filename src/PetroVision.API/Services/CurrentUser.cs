using System.Security.Claims;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal Principal => _accessor.HttpContext?.User ?? new ClaimsPrincipal();

    public int UserId => int.TryParse(Principal.FindFirst("sub")?.Value, out var id) ? id : 0;

    public string Role => Principal.FindFirst("role")?.Value ?? string.Empty;

    public int? DepartmentId => int.TryParse(Principal.FindFirst("department")?.Value, out var id) ? id : null;
}
