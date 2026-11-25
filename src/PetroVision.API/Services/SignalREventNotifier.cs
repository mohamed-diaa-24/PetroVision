using Microsoft.AspNetCore.SignalR;
using PetroVision.API.Hubs;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Enums;

namespace PetroVision.API.Services;

// Routing:
//   alerts and cameras -> "dept-{id}" only (the Owner never sees alerts or cameras)
//   incidents          -> "dept-{id}" and "owners"
// Payloads are the same DTOs the REST endpoints return.
// Never throws: a failed push is logged and must not fail the request or the detection.
public class SignalREventNotifier : IEventNotifier
{
    private readonly IHubContext<EventsHub> _hub;
    private readonly ILogger<SignalREventNotifier> _logger;

    public SignalREventNotifier(IHubContext<EventsHub> hub, ILogger<SignalREventNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task AlertCreatedAsync(int departmentId, AlertDto alert)
        => SendAsync(EventNames.AlertCreated, alert, EventGroups.Department(departmentId));

    public Task AlertUpdatedAsync(int departmentId, AlertDto alert)
        => SendAsync(EventNames.AlertUpdated, alert, EventGroups.Department(departmentId));

    public Task IncidentCreatedAsync(int departmentId, IncidentDto incident)
        => SendAsync(EventNames.IncidentCreated, incident, EventGroups.Department(departmentId), EventGroups.Owners);

    public Task IncidentUpdatedAsync(int departmentId, IncidentDto incident)
        => SendAsync(EventNames.IncidentUpdated, incident, EventGroups.Department(departmentId), EventGroups.Owners);

    public Task CameraStatusChangedAsync(int departmentId, int cameraId, CameraHealth health, DateTime? lastSeenAt)
        => SendAsync(EventNames.CameraStatusChanged, new CameraStatusChangedPayload(cameraId, health, lastSeenAt),
            EventGroups.Department(departmentId));

    public Task CamerasChangedAsync(int departmentId, string action, int cameraId, CameraDto? camera)
        => SendAsync(EventNames.CamerasChanged, new CamerasChangedPayload(action, cameraId, camera),
            EventGroups.Department(departmentId));

    private async Task SendAsync(string eventName, object payload, params string[] groups)
    {
        try
        {
            await _hub.Clients.Groups(groups).SendAsync(eventName, payload);
            _logger.LogDebug("Realtime {Event} sent to {Groups}", eventName, string.Join(", ", groups));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Realtime {Event} to {Groups} failed", eventName, string.Join(", ", groups));
        }
    }
}

// Serialized as camelCase: { cameraId, health, lastSeenAt } and { action, cameraId, camera }
public record CameraStatusChangedPayload(int CameraId, CameraHealth Health, DateTime? LastSeenAt);

public record CamerasChangedPayload(string Action, int CameraId, CameraDto? Camera);
