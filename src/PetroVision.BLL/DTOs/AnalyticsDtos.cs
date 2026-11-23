using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class DailyEventsDto
{
    public DateOnly Date { get; set; }
    public int Alerts { get; set; }
    public int Incidents { get; set; }
}

public class CategoryShareDto
{
    public DetectionType Type { get; set; }
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class CameraEventsDto
{
    public int CameraId { get; set; }
    public string Camera { get; set; } = string.Empty;
    public int Events { get; set; }
}

public class AnalyticsDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int Days { get; set; }

    public int Warnings { get; set; }
    public double? WarningsChangePercent { get; set; }   // vs the previous period of the same length
    public int Incidents { get; set; }
    public int OpenIncidents { get; set; }
    public int FalseAlarms { get; set; }
    public double FalseAlarmRate { get; set; }            // % of warnings dismissed
    public double? AvgAcknowledgeSeconds { get; set; }    // alerts and incidents, creation -> first handling

    public List<DailyEventsDto> EventsPerDay { get; set; } = new();
    public List<CategoryShareDto> Categories { get; set; } = new();
    public List<CameraEventsDto> TopCameras { get; set; } = new();
}
