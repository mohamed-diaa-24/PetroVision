using Microsoft.EntityFrameworkCore;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PetroVisionDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        var now = DateTime.UtcNow;

        // ---------- Departments ----------
        var unit1 = new Department { Name = "Unit 1" };
        var unit2 = new Department { Name = "Unit 2" };
        var unit3 = new Department { Name = "Unit 3" };
        var unit4 = new Department { Name = "Unit 4" };
        db.Departments.AddRange(unit1, unit2, unit3, unit4);

        // ---------- Users ----------
        var manager = new User { FullName = "Mohamed S.", EmployeeCode = "1001", Email = "manager@petrovision.com", PasswordHash = Hash("Manager@123"), Role = UserRole.Manager, Status = UserStatus.Active, Department = unit1, LastActiveAt = now };
        var owner   = new User { FullName = "Owner", EmployeeCode = "1002", Email = "owner@petrovision.com", PasswordHash = Hash("Owner@123"), Role = UserRole.Owner, Status = UserStatus.Active };
        var ahmed   = new User { FullName = "Ahmed S.", EmployeeCode = "4710", Email = "ahmed@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Active, Department = unit1, LastActiveAt = now.AddMinutes(-5) };
        var mona    = new User { FullName = "Mona K.", EmployeeCode = "4821", Email = "mona@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Active, Department = unit1, LastActiveAt = now.AddHours(-4) };
        var omar    = new User { FullName = "Omar H.", EmployeeCode = "4833", Email = "omar@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Expired, Department = unit1 };
        var khaled  = new User { FullName = "Khaled R.", EmployeeCode = "4850", Email = "khaled@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Pending, Department = unit1 };

        // Manager candidates waiting for the Owner's approval
        var candA = new User { FullName = "Manager Candidate A", EmployeeCode = "2001", Email = "manager.a@petrovision.com", PasswordHash = Hash("Manager@123"), Role = UserRole.Manager, Status = UserStatus.Pending, Department = unit2 };
        var candB = new User { FullName = "Manager Candidate B", EmployeeCode = "2002", Email = "manager.b@petrovision.com", PasswordHash = Hash("Manager@123"), Role = UserRole.Manager, Status = UserStatus.Pending, Department = unit3 };

        db.Users.AddRange(manager, owner, ahmed, mona, omar, khaled, candA, candB);

        db.AgentPermissions.AddRange(
            new AgentPermission { User = ahmed, Permission = PermissionType.ReassignIncidents },
            new AgentPermission { User = ahmed, Permission = PermissionType.PostAnnouncements },
            new AgentPermission { User = mona, Permission = PermissionType.ManageCameras });

        // ---------- Cameras ----------
        var cam1 = NewCamera("Cam 1", "Gate A", unit1, CameraHealth.Online, now, DetectionType.PPE);
        var cam2 = NewCamera("Cam 2", "Zone A", unit1, CameraHealth.Online, now, DetectionType.PPE, DetectionType.Fall);
        var cam3 = NewCamera("Cam 3", "Zone B", unit1, CameraHealth.Online, now.AddMinutes(-1), DetectionType.PPE, DetectionType.Fall);
        var cam4 = NewCamera("Cam 4", "Yard", unit1, CameraHealth.Offline, now.AddMinutes(-50), DetectionType.PPE);
        var cam5 = NewCamera("Cam 5", "Zone C", unit1, CameraHealth.Online, now, DetectionType.Fire);

        var u2cam1 = NewCamera("U2-Cam 1", "Gate", unit2, CameraHealth.Online, now, DetectionType.PPE);
        var u2cam2 = NewCamera("U2-Cam 2", "Yard", unit2, CameraHealth.Offline, now.AddHours(-3), DetectionType.PPE);
        var u3cam1 = NewCamera("U3-Cam 1", "Gate", unit3, CameraHealth.Online, now, DetectionType.PPE, DetectionType.Fire);
        var u4cam1 = NewCamera("U4-Cam 1", "Tank Area", unit4, CameraHealth.Online, now, DetectionType.Fire);

        db.Cameras.AddRange(cam1, cam2, cam3, cam4, cam5, u2cam1, u2cam2, u3cam1, u4cam1);

        // ---------- Alerts ----------
        var ackAlert = new Alert { Camera = cam1, Type = DetectionType.PPE, Level = AlertLevel.Low, Status = AlertStatus.Acknowledged, Reason = "PPE check", Confidence = 0.65, CreatedAt = now.AddMinutes(-18), HandledBy = ahmed, HandledAt = now.AddMinutes(-15) };
        var dismissedAlert = new Alert { Camera = cam1, Type = DetectionType.PPE, Level = AlertLevel.Low, Status = AlertStatus.Dismissed, DismissReason = DismissReason.WrongDetection, Reason = "Wrong detection", Confidence = 0.61, CreatedAt = now.AddHours(-2), HandledBy = manager, HandledAt = now.AddHours(-2) };
        db.Alerts.AddRange(
            new Alert { Camera = cam2, Type = DetectionType.PPE, Level = AlertLevel.Medium, Status = AlertStatus.New, Reason = "Missing safety helmet", Confidence = 0.81, CreatedAt = now.AddMinutes(-3) },
            ackAlert,
            dismissedAlert);

        // ---------- Incidents ----------
        var closedIncident = new Incident { Camera = cam5, Type = IncidentType.Fire, Status = IncidentStatus.Closed, Confidence = 0.88, Note = "Area checked and safe", Handler = ahmed, CreatedAt = now.AddHours(-1), ClosedAt = now.AddMinutes(-40) };
        db.Incidents.AddRange(
            new Incident { Camera = cam3, Type = IncidentType.Fall, Status = IncidentStatus.Active, Confidence = 0.91, CreatedAt = now },
            closedIncident);

        await db.SaveChangesAsync();

        // ---------- Audit log (needs the ids above) ----------
        db.AuditLogs.AddRange(
            Log(AuditAction.AlertDismissed, "Alert", dismissedAlert.Id, $"Alert #{dismissedAlert.Id}", manager, now.AddHours(-2), "Reason: WrongDetection"),
            Log(AuditAction.IncidentClosed, "Incident", closedIncident.Id, $"Incident #{closedIncident.Id}", ahmed, now.AddMinutes(-40), closedIncident.Note),
            Log(AuditAction.AlertAcknowledged, "Alert", ackAlert.Id, $"Alert #{ackAlert.Id}", ahmed, now.AddMinutes(-15)));

        await db.SaveChangesAsync();
    }

    private static AuditLog Log(AuditAction action, string type, int id, string item, User user, DateTime at, string? details = null) => new()
    {
        Action = action,
        EntityType = type,
        EntityId = id,
        Item = item,
        Details = details,
        UserId = user.Id,
        DepartmentId = user.DepartmentId,
        CreatedAt = at
    };

    private static Camera NewCamera(string name, string location, Department department, CameraHealth health, DateTime lastSeen, params DetectionType[] models) => new()
    {
        Name = name,
        Location = location,
        Department = department,
        Health = health,
        LastSeenAt = lastSeen,
        StreamUrl = $"rtsp://demo/{name.Replace(" ", "").ToLower()}",
        ModelConfigs = models.Select(m => new CameraModelConfig { Type = m }).ToList()
    };
}
