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

// Manager + Agent: one department
public class DashboardDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public bool NeedsSetup { get; set; }               // no cameras yet (shown to Manager only by the frontend)
    public int ActiveAlerts { get; set; }
    public int OpenIncidents { get; set; }
    public int CamerasOnline { get; set; }
    public int CamerasTotal { get; set; }
    public int ModelsRunning { get; set; }
    public List<DetectionType> ModelTypes { get; set; } = new();
    public List<EventDto> LatestEvents { get; set; } = new();
    public List<CameraDto> Cameras { get; set; } = new();
}

// Owner: all departments
public class ManagerRequestDto
{
    public int UserId { get; set; }
    public string Applicant { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
}

public class OwnerIncidentDto
{
    public int Id { get; set; }
    public DetectionType Type { get; set; }
    public string Camera { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public IncidentStatus Status { get; set; }
    public DateTime Time { get; set; }
}

public class OwnerDashboardDto
{
    public int Departments { get; set; }
    public int ManagersPending { get; set; }
    public int MajorIncidents { get; set; }
    public int IncidentsRequiringAttention { get; set; }
    public int DevicesOnline { get; set; }
    public int DevicesTotal { get; set; }
    public int PendingRequests { get; set; }
    public List<ManagerRequestDto> ManagerRequests { get; set; } = new();
    public List<OwnerIncidentDto> RecentIncidents { get; set; } = new();
}
