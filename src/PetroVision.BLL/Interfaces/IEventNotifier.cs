using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

// Pushes real-time changes to connected dashboards (server -> client only).
// The implementation lives in the API (SignalR); the BLL only knows this contract.
// Call it only after SaveChangesAsync succeeded. Implementations must not throw.
public interface IEventNotifier
{
    Task AlertCreatedAsync(int departmentId, AlertDto alert);
    Task AlertUpdatedAsync(int departmentId, AlertDto alert);

    Task IncidentCreatedAsync(int departmentId, IncidentDto incident);
    Task IncidentUpdatedAsync(int departmentId, IncidentDto incident);

    Task CameraStatusChangedAsync(int departmentId, int cameraId, CameraHealth health, DateTime? lastSeenAt);

    // action: "created" | "updated" | "deleted" (camera is null when deleted)
    Task CamerasChangedAsync(int departmentId, string action, int cameraId, CameraDto? camera);
}

public static class CameraChangeActions
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
}
