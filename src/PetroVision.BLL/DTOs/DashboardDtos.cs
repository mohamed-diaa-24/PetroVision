using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class EventDto
{
    public int Id { get; set; }
    public string Kind { get; set; } = string.Empty;   // "Alert" or "Incident"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;  // LOW / MEDIUM / INCIDENT
    public DateTime Time { get; set; }
}

public class DashboardDto
{
    public int ActiveAlerts { get; set; }
    public int OpenIncidents { get; set; }
    public int CamerasOnline { get; set; }
    public int CamerasTotal { get; set; }
    public int ModelsRunning { get; set; }
    public List<DetectionType> ModelTypes { get; set; } = new();
    public List<EventDto> LatestEvents { get; set; } = new();
    public List<CameraDto> Cameras { get; set; } = new();
}
