using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PetroVision.API.Hubs;

// Server -> client only. Clients connect to /hubs/events with their JWT and never invoke methods.
// Manager/Agent join "dept-{department}", Owner joins "owners"; anything else is disconnected.
[Authorize]
public class EventsHub : Hub
{
    private readonly ILogger<EventsHub> _logger;

    public EventsHub(ILogger<EventsHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var role = user?.FindFirst("role")?.Value;
        var group = role switch
        {
            "Manager" or "Agent" when int.TryParse(user!.FindFirst("department")?.Value, out var departmentId)
                => EventGroups.Department(departmentId),
            "Owner" => EventGroups.Owners,
            _ => null
        };

        if (group is null)
        {
            _logger.LogWarning("Realtime connection {ConnectionId} rejected (role: {Role})", Context.ConnectionId, role ?? "none");
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogInformation("Realtime connection {ConnectionId} (user {UserId}, {Role}) joined {Group}",
            Context.ConnectionId, user!.FindFirst("sub")?.Value, role, group);

        await base.OnConnectedAsync();
    }
}

public static class EventGroups
{
    public const string Owners = "owners";

    public static string Department(int departmentId) => $"dept-{departmentId}";
}

// Client event names (keep in sync with frontend/js/realtime.js).
public static class EventNames
{
    public const string AlertCreated = "AlertCreated";
    public const string AlertUpdated = "AlertUpdated";
    public const string IncidentCreated = "IncidentCreated";
    public const string IncidentUpdated = "IncidentUpdated";
    public const string CameraStatusChanged = "CameraStatusChanged";
    public const string CamerasChanged = "CamerasChanged";
}
